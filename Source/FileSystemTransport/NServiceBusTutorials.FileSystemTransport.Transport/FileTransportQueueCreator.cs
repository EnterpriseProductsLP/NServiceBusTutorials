using System.IO;
using System.Threading.Tasks;
using NServiceBus.Transport;

namespace NServiceBusTutorials.FileSystemTransport.Transport
{
    internal class FileTransportQueueCreator : ICreateQueues
    {
        public Task CreateQueueIfNecessary(QueueBindings queueBindings, string identity)
        {
            foreach (var sendingAddress in queueBindings.SendingAddresses)
            {
                BuildQueueDirectory(sendingAddress);
            }

            foreach (var receivingAddress in queueBindings.ReceivingAddresses)
            {
                BuildQueueDirectory(receivingAddress);
            }

            return Task.CompletedTask;
        }

        private static void BuildQueueDirectory(string address)
        {
            var queuePath = DirectoryBuilder.BuildBasePath(address);

            var committedPath = Path.Combine(queuePath, ".committed");
            Directory.CreateDirectory(committedPath);

            var bodiesPath = Path.Combine(queuePath, ".bodies");
            Directory.CreateDirectory(bodiesPath);
        }

    }
}