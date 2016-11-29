using System.Threading.Tasks;
using NServiceBus;
using NServiceBus.Logging;

namespace NServiceBusTutorials.CallbackUsage.Receiver.MessageHandlers
{
    class ObjectMessageHandler : IHandleMessages<ObjectMessage>
    {
        private static ILog log = LogManager.GetLogger<ObjectMessageHandler>();

        public Task Handle(ObjectMessage message, IMessageHandlerContext context)
        {
            log.Info("Object message received.  Returning.");
            var responseMessage = new ObjectMessageResponse {Property = "PropertyValue"};
            return context.Reply(responseMessage);
        }
    }
}