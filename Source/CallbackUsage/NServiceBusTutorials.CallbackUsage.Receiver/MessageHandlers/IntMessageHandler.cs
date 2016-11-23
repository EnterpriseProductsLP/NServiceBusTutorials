using System.Threading.Tasks;
using NServiceBus;
using NServiceBus.Logging;
using NServiceBusTutorials.CallbackUsage.Contracts;

namespace NServiceBusTutorials.CallbackUsage.Receiver.MessageHandlers
{
    class IntMessageHandler : IHandleMessages<IntMessage>
    {
        static ILog log = LogManager.GetLogger<IntMessageHandler>();

        public Task Handle(IntMessage message, IMessageHandlerContext context)
        {
            log.Info("Int message received.  Returning.");
            var responseMessage = new IntMessageResponse {Value = 10};
            return context.Reply(responseMessage);
        }
    }
}