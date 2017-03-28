using System.Threading.Tasks;
using NServiceBus;
using NServiceBus.Logging;
using NServiceBusTutorials.FileSystemTransport.Contracts;

namespace NServiceBusTutorials.FileSystemTransport.ApplicationOne.MessageHandlers
{
    public class MessageBHandler : IHandleMessages<MessageB>
    {
        private static readonly ILog Log = LogManager.GetLogger<MessageBHandler>();

        public Task Handle(MessageB message, IMessageHandlerContext context)
        {
            Log.Info("MessageB handled");
            return Task.CompletedTask;
        }
    }
}