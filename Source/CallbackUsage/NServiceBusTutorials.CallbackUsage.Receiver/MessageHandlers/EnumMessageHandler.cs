using System.Threading.Tasks;
using NServiceBus;
using NServiceBus.Logging;
using NServiceBusTutorials.CallbackUsage.Contracts;

namespace NServiceBusTutorials.CallbackUsage.Receiver.MessageHandlers
{
    internal class EnumMessageHandler : IHandleMessages<EnumMessage>
    {
        private static ILog log = LogManager.GetLogger<EnumMessageHandler>();

        public Task Handle(EnumMessage message, IMessageHandlerContext context)
        {
            log.Info("Enum message received.  Returning.");
            var responseMessage = new EnumMessageResponse {Status = Status.OK};
            return context.Reply(responseMessage);
        }
    }
}
