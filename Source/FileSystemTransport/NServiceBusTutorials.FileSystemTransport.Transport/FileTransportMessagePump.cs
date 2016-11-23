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
    public class FileTransportMessagePump : IPushMessages
    {
        private static readonly ILog log = LogManager.GetLogger<FileTransportMessagePump>();

        private CancellationToken _cancellationToken;
        private CancellationTokenSource _cancellationTokenSource;
        private SemaphoreSlim _concurrencyLimiter;
        private Task _messagePumpTask;
        private Func<ErrorContext, Task<ErrorHandleResult>> _onError;
        private string _path;
        private Func<MessageContext, Task> _pipeline;
        private bool _purgeOnStartup;
        private ConcurrentDictionary<Task, Task> _runningReceiveTasks;

        public Task Init(Func<MessageContext, Task> onMessage, Func<ErrorContext, Task<ErrorHandleResult>> onError,
            CriticalError criticalError, PushSettings settings)
        {
            _onError = onError;
            _pipeline = onMessage;
            _path = BaseDirectoryBuilder.BuildBasePath(settings.InputQueue);
            _purgeOnStartup = settings.PurgeOnStartup;
            return Task.CompletedTask;
        }

        public void Start(PushRuntimeSettings limitations)
        {
            _runningReceiveTasks = new ConcurrentDictionary<Task, Task>();
            _concurrencyLimiter = new SemaphoreSlim(limitations.MaxConcurrency);
            _cancellationTokenSource = new CancellationTokenSource();
            _cancellationToken = _cancellationTokenSource.Token;

            if (_purgeOnStartup)
            {
                Directory.Delete(_path, true);
                Directory.CreateDirectory(_path);
            }

            _messagePumpTask = Task.Factory
                .StartNew(
                    function: ProcessMessages,
                    cancellationToken: CancellationToken.None,
                    creationOptions: TaskCreationOptions.LongRunning,
                    scheduler: TaskScheduler.Default
                )
                .Unwrap();
        }

        public async Task Stop()
        {
            _cancellationTokenSource.Cancel();

            var timeoutTask = Task.Delay(TimeSpan.FromSeconds(30), _cancellationTokenSource.Token);
            var allTasks = _runningReceiveTasks.Values.Concat(new[]
            {
                _messagePumpTask
            });

            var finishedTask = await Task.WhenAny(Task.WhenAll(allTasks), timeoutTask).ConfigureAwait(false);

            if (finishedTask.Equals(timeoutTask))
            {
                log.Error("The message pump failed to stop within the time allowed(30s)");
            }

            _concurrencyLimiter.Dispose();
            _runningReceiveTasks.Clear();
        }

        private async Task ProcessMessages()
        {
            try
            {
                await InnerProcessMessages().ConfigureAwait(false);
            }
            catch (OperationCanceledException)
            {
                // For graceful shutdown purposes.
            }
            catch (Exception ex)
            {
                log.Error("File Message pump failed", ex);
            }

            if (!_cancellationToken.IsCancellationRequested)
            {
                await ProcessMessages().ConfigureAwait(false);
            }
        }

        private async Task InnerProcessMessages()
        {
            while (!_cancellationTokenSource.IsCancellationRequested)
            {
                var filesFound = false;

                foreach (var filePath in Directory.EnumerateFiles(_path, "*.*"))
                {
                    filesFound = true;
                    await ProcessFile(filePath).ConfigureAwait(false);
                }

                if (!filesFound)
                {
                    await Task.Delay(10, _cancellationToken).ConfigureAwait(false);
                }
            }
        }

        private async Task ProcessFile(string filePath)
        {
            var nativeMessageId = Path.GetFileNameWithoutExtension(filePath);

            await _concurrencyLimiter.WaitAsync(_cancellationToken).ConfigureAwait(false);

            Func<Task> processFileWithTransaction = async () =>
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
            var task = Task.Run(processFileWithTransaction, _cancellationToken);

            task.ContinueWith(t =>
                    {
                        Task toBeRemoved;
                        _runningReceiveTasks.TryRemove(t, out toBeRemoved);
                    },
                    TaskContinuationOptions.ExecuteSynchronously)
                .Ignore();

            _runningReceiveTasks.AddOrUpdate(task, task, (k, v) => task).Ignore();
        }

        private async Task ProcessFileWithTransaction(string filePath, string messageId)
        {
            using (var transaction = new DirectoryBasedTransaction(_path))
            {
                transaction.BeginTransaction(filePath);

                var message = File.ReadAllLines(transaction.FileToProcess);
                var bodyPath = message.First();
                var json = string.Join("", message.Skip(1));
                var headers = HeaderSerializer.DeSerialize(json);

                string ttbrString;
                if (headers.TryGetValue(Headers.TimeToBeReceived, out ttbrString))
                {
                    var ttbr = TimeSpan.Parse(ttbrString);

                    // File.Move preserves create time.
                    var sentTime = File.GetCreationTimeUtc(transaction.FileToProcess);

                    if (sentTime + ttbr > DateTime.UtcNow)
                    {
                        return;
                    }

                    var body = File.ReadAllBytes(bodyPath);
                    var transportTransaction = new TransportTransaction();
                    transportTransaction.Set(transaction);

                    var shouldCommit = await HandleMessageWithRetries(messageId, headers, body, transportTransaction, 1);

                    if (shouldCommit)
                    {
                        transaction.Commit();
                    }
                }
            }
        }

        private async Task<bool> HandleMessageWithRetries(string messageId, Dictionary<string, string> headers,
            byte[] body, TransportTransaction transportTransaction, int processingAttempt)
        {
            try
            {
                var receiveCancellationTokenSource = new CancellationTokenSource();
                var pushContext = new MessageContext(
                    messageId: messageId,
                    headers: new Dictionary<string, string>(headers),
                    body: body,
                    transportTransaction: transportTransaction,
                    receiveCancellationTokenSource: receiveCancellationTokenSource,
                    context: new ContextBag());

                await _pipeline(pushContext).ConfigureAwait(false);

                return !receiveCancellationTokenSource.IsCancellationRequested;
            }
            catch (Exception ex)
            {
                var errorContext = new ErrorContext(ex, headers, messageId, body, transportTransaction, processingAttempt);
                var errorHandlingResult = await _onError(errorContext);

                if (errorHandlingResult == ErrorHandleResult.RetryRequired)
                {
                    return await HandleMessageWithRetries(messageId, headers, body, transportTransaction, ++processingAttempt);
                }

                return true;
            }
        }
    }
}