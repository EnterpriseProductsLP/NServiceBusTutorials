using System.Threading.Tasks;
using NServiceBus;
using NServiceBus.Logging;
using NServiceBusTutorials.CallbackUsage.Contracts;

namespace NServiceBusTutorials.CallbackUsage.Receiver.MessageHandlers
{
    class ObjectMessageHandler : IHandleMessages<ObjectMessage>
    {
        static ILog log = LogManager.GetLogger<EnumMessageHandler>();

        public Task Handle(ObjectMessage message, IMessageHandlerContext context)
        {
            log.Info("Object message received.  Returning.");
            var responseMessage = new ObjectMessageResponse {Property = "PropertyValue"};
            return context.Reply(responseMessage);
        }
    }
}