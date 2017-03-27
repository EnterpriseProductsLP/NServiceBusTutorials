using System.Threading.Tasks;
using NServiceBus;
using NServiceBus.Logging;
using NServiceBusTutorials.CallbackUsage.Contracts;

namespace NServiceBusTutorials.CallbackUsage.Receiver.MessageHandlers
{
    internal class EnumMessageHandler : IHandleMessages<EnumMessage>
    {
        private static readonly ILog Log = LogManager.GetLogger<EnumMessageHandler>();

        public Task Handle(EnumMessage message, IMessageHandlerContext context)
        {
            Log.Info("Enum message received.  Returning.");
            var responseMessage = new EnumMessageResponse {Status = Status.Ok};
            return context.Reply(responseMessage);
        }
    }
}
