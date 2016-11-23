using System.IO;
using System.Threading.Tasks;
using NServiceBus.Transport;

namespace NServiceBusTutorials.FileSystemTransport.Transport
{
    public class FileTransportQueueCreator : ICreateQueues
    {
        public Task CreateQueueIfNecessary(QueueBindings queueBindings, string identity)
        {
            foreach (var sendingAddress in queueBindings.SendingAddresses)
            {
                CreateQueueDirectory(sendingAddress);
            }

            foreach (var receivingAddress in queueBindings.ReceivingAddresses)
            {
                CreateQueueDirectory(receivingAddress);
            }

            return Task.CompletedTask;
        }

        private void CreateQueueDirectory(string address)
        {
            var fullPath = BaseDirectoryBuilder.BuildBasePath(address);
            var committedPath = Path.Combine(fullPath, ".committed");
            Directory.CreateDirectory(committedPath);

            var bodiesPath = Path.Combine(fullPath, ".bodies");
            Directory.CreateDirectory(bodiesPath);
        }
    }
}