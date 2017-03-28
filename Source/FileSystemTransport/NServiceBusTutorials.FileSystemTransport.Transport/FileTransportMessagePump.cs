using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using NServiceBus;
using NServiceBus.Extensibility;
using NServiceBus.Logging;
using NServiceBus.Transport;
using NServiceBusTutorials.Common.Extensions;

namespace NServiceBusTutorials.FileSystemTransport.Transport
{
    internal class FileTransportMessagePump : IPushMessages
    {
        private static readonly ILog Log = LogManager.GetLogger<FileTransportMessagePump>();

        private CancellationToken _cancellationToken;
        private CancellationTokenSource _cancellationTokenSource;
        private SemaphoreSlim _concurrencyLimiter;
        private string _messageDirectory;
        private Task _messagePumpTask;
        private Func<ErrorContext, Task<ErrorHandleResult>> _onError;
        private Func<MessageContext, Task> _pipeline;
        private bool _purgeOnStartup;
        private ConcurrentDictionary<Task, Task> _runningReceiveTasks;
        private const int PumpMessageCheckDelay = 100;

        public Task Init(Func<MessageContext, Task> onMessage, Func<ErrorContext, Task<ErrorHandleResult>> onError, CriticalError criticalError, PushSettings settings)
        {
            _onError = onError;
            _pipeline = onMessage;
            _messageDirectory = DirectoryBuilder.BuildBasePath(settings.InputQueue);
            _purgeOnStartup = settings.PurgeOnStartup;
            return Task.CompletedTask;
        }

        public void Start(PushRuntimeSettings pushRuntimeSettings)
        {
            HydrateFields(pushRuntimeSettings);
            PurgeMessageDirectory();
            StartMessagePump();
        }

        public async Task Stop()
        {
            CancelTasks();
            await WaitForTasksToFinish();
            CleanupResources();
        }

        private async Task<bool> HandleMessage(string messageId, Dictionary<string, string> headers, byte[] body, TransportTransaction transportTransaction)
        {
            var receiveCancellationTokenSource = new CancellationTokenSource();
            var pushContext = new MessageContext(
                messageId,
                new Dictionary<string, string>(headers),
                body,
                transportTransaction,
                receiveCancellationTokenSource,
                new ContextBag());

            await _pipeline(pushContext).ConfigureAwait(false);

            return !receiveCancellationTokenSource.IsCancellationRequested;
        }

        private async Task<bool> HandleMessageRetry(string messageId, Dictionary<string, string> headers, byte[] body, TransportTransaction transportTransaction, int processingAttempt, Exception ex)
        {
            var errorContext = new ErrorContext(ex, headers, messageId, body, transportTransaction, processingAttempt);
            var errorHandlingResult = await _onError(errorContext);

            if (errorHandlingResult == ErrorHandleResult.RetryRequired)
            {
                return await HandleMessageWithRetries(messageId, headers, body, transportTransaction, ++processingAttempt);
            }

            return true;
        }

        private async Task<bool> HandleMessageWithRetries(string messageId, Dictionary<string, string> headers, byte[] body, TransportTransaction transportTransaction, int processingAttempt)
        {
            try
            {
                return await HandleMessage(messageId, headers, body, transportTransaction);
            }
            catch (Exception ex)
            {
                return await HandleMessageRetry(messageId, headers, body, transportTransaction, processingAttempt, ex);
            }
        }

        private async Task InnerProcessMessages()
        {
            while (!_cancellationTokenSource.IsCancellationRequested)
            {
                var filesFound = false;

                foreach (var filePath in Directory.EnumerateFiles(_messageDirectory, "*.*"))
                {
                    filesFound = true;
                    await ProcessFile(filePath).ConfigureAwait(false);
                }

                if (!filesFound)
                {
                    await Task.Delay(PumpMessageCheckDelay, _cancellationToken).ConfigureAwait(false);
                }
            }
        }

        private async Task ProcessFile(string filePath)
        {
            var nativeMessageId = Path.GetFileNameWithoutExtension(filePath);
            await _concurrencyLimiter.WaitAsync(_cancellationToken).ConfigureAwait(false);
            Func<Task> processFileTaskFactory = async () =>
            {
                try
                {
                    await ProcessFileWithTransaction(filePath, nativeMessageId).ConfigureAwait(false);
                }
                finally
                {
                    _concurrencyLimiter.Release();
                }
            };
            var task = Task.Run(processFileTaskFactory, _cancellationToken);

            Action<Task> removeRunningTask = runningTask =>
            {
                Task toBeRemoved;
                _runningReceiveTasks.TryRemove(runningTask, out toBeRemoved);
            };
            task.ContinueWith(removeRunningTask, TaskContinuationOptions.ExecuteSynchronously).Ignore();
            _runningReceiveTasks.AddOrUpdate(task, task, (k, v) => task).Ignore();
        }

        private async Task ProcessFileWithTransaction(string filePath, string messageId)
        {
            using (var transaction = new DirectoryBasedTransaction(_messageDirectory))
            {
                transaction.BeginTransaction(filePath);

                var messageFile = File.ReadAllLines(transaction.FileToProcess);
                var bodyPath = messageFile.First();
                var messageHeaderJson = string.Join("", messageFile.Skip(1));
                var messageHeaders = HeaderSerializer.DeSerialize(messageHeaderJson);

                if (RemoveFileIfExpired(messageHeaders, transaction))
                {
                    return;
                }

                var fileContents = File.ReadAllBytes(bodyPath);
                var transportTransaction = new TransportTransaction();
                transportTransaction.Set(transaction);

                var shouldCommit = await HandleMessageWithRetries(messageId, messageHeaders, fileContents, transportTransaction, 1);
                if (shouldCommit)
                {
                    transaction.Commit();
                }
            }
        }

        private async Task ProcessMessages()
        {
            try
            {
                await InnerProcessMessages().ConfigureAwait(false);
            }
            catch (OperationCanceledException)
            {
                // For graceful shutdown purposes
            }
            catch (Exception ex)
            {
                Log.Error("File Message pump failed", ex);
            }

            if (!_cancellationToken.IsCancellationRequested)
            {
                await ProcessMessages().ConfigureAwait(false);
            }
        }

        private void CancelTasks()
        {
            _cancellationTokenSource.Cancel();
        }

        private void CleanupResources()
        {
            _concurrencyLimiter.Dispose();
            _runningReceiveTasks.Clear();
        }

        private void HydrateFields(PushRuntimeSettings pushRuntimeSettings)
        {
            _runningReceiveTasks = new ConcurrentDictionary<Task, Task>();
            _concurrencyLimiter = new SemaphoreSlim(pushRuntimeSettings.MaxConcurrency);
            _cancellationTokenSource = new CancellationTokenSource();
            _cancellationToken = _cancellationTokenSource.Token;
        }

        private void PurgeMessageDirectory()
        {
            if (!_purgeOnStartup)
            {
                return;
            }

            Directory.Delete(_messageDirectory, true);
            Directory.CreateDirectory(_messageDirectory);
        }

        private static bool RemoveFileIfExpired(Dictionary<string, string> messageHeaders, DirectoryBasedTransaction transaction)
        {
            string timeToBeReturnedString;
            if (messageHeaders.TryGetValue(Headers.TimeToBeReceived, out timeToBeReturnedString))
            {
                // This works because moving the file inside the transaction preserves create time
                var sentTime = File.GetCreationTimeUtc(transaction.FileToProcess);
                var timeToBeReturned = TimeSpan.Parse(timeToBeReturnedString);
                if (sentTime + timeToBeReturned < DateTime.UtcNow)
                {
                    return true;
                }
            }
            return false;
        }

        private void StartMessagePump()
        {
            _messagePumpTask = Task.Factory
                .StartNew(
                    ProcessMessages,
                    CancellationToken.None,
                    TaskCreationOptions.LongRunning,
                    TaskScheduler.Default)
                .Unwrap();
        }

        private async Task WaitForTasksToFinish()
        {
            var timeoutTask = Task.Delay(TimeSpan.FromSeconds(30), _cancellationTokenSource.Token);
            var allTasks = _runningReceiveTasks.Values.Concat(new[]
            {
                _messagePumpTask
            });

            var finishedTask = await Task.WhenAny(Task.WhenAll(allTasks), timeoutTask).ConfigureAwait(false);

            if (finishedTask.Equals(timeoutTask))
            {
                Log.Error("The message pump failed to stop with in the time allowed(30s)");
            }
        }
    }
}