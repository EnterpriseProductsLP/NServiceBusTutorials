using System;
using System.IO;

namespace NServiceBusTutorials.FileSystemTransport.Transport
{
    internal class DirectoryBasedTransaction : IDisposable
    {
        private readonly string _basePath;
        private bool _committed;
        private readonly string _transactionDir;

        public DirectoryBasedTransaction(string basePath)
        {
            _basePath = basePath;
            var transactionId = Guid.NewGuid().ToString();
            _transactionDir = Path.Combine(basePath, ".pending", transactionId);
        }

        public string FileToProcess { get; private set; }

        public void BeginTransaction(string incomingFilePath)
        {
            Directory.CreateDirectory(_transactionDir);
            FileToProcess = Path.Combine(_transactionDir, Path.GetFileName(incomingFilePath));
            File.Move(incomingFilePath, FileToProcess);
        }

        public void Commit() => _committed = true;

        public void Dispose()
        {
            if (!_committed)
            {
                // Roll back by moving the file back to the main dir
                File.Move(FileToProcess, Path.Combine(_basePath, Path.GetFileName(FileToProcess)));
            }

            Directory.Delete(_transactionDir, true);
        }
    }
}
