using System.Threading.Tasks;
using NServiceBus;
using NServiceBus.Logging;
using NServiceBusTutorials.FileSystemTransport.Contracts;

namespace NServiceBusTutorials.FileSystemTransport.ApplicationTwo.MessageHandlers
{
    public class MessageAHandler : IHandleMessages<MessageA>
    {
        private static readonly ILog Log = LogManager.GetLogger<MessageAHandler>();

        public Task Handle(MessageA message, IMessageHandlerContext context)
        {
            Log.Info("MessageA handled");
            Log.Info("Replying with MessageB");
            return context.Reply(new MessageB());
        }
    }
}