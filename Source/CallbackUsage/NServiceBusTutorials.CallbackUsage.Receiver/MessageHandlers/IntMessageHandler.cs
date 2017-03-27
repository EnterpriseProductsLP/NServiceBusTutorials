using System.Threading.Tasks;
using NServiceBus;
using NServiceBus.Logging;
using NServiceBusTutorials.CallbackUsage.Contracts;

namespace NServiceBusTutorials.CallbackUsage.Receiver.MessageHandlers
{
    internal class IntMessageHandler : IHandleMessages<IntMessage>
    {
        private static readonly ILog Log = LogManager.GetLogger<IntMessageHandler>();

        public Task Handle(IntMessage message, IMessageHandlerContext context)
        {
            Log.Info("Int message received.  Returning.");
            var responseMessage = new IntMessageResponse {Value = 10};
            return context.Reply(responseMessage);
        }
    }
}