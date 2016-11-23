using System.Threading.Tasks;
using NServiceBus;
using NServiceBus.Logging;
using NServiceBusTutorials.FileSystemTransport.Contracts;

namespace NServiceBusTutorials.FileSystemTransport.ApplicationTwo.MessageHandlers
{
    public class MessageAHandler : IHandleMessages<MessageA>
    {
        static ILog log = LogManager.GetLogger<MessageAHandler>();

        public Task Handle(MessageA message, IMessageHandlerContext context)
        {
            log.Info("MessageA handled");
            log.Info("Replying with MessageB");
            return context.Reply(new MessageB());
        }
    }
}