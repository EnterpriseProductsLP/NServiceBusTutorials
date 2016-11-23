using System.Threading.Tasks;
using NServiceBus;
using NServiceBus.Logging;
using NServiceBusTutorials.CallbackUsage.Contracts;

namespace NServiceBusTutorials.CallbackUsage.Receiver.MessageHandlers
{
    public class EnumMessageHandler : IHandleMessages<EnumMessage>
    {
        static ILog log = LogManager.GetLogger<EnumMessageHandler>();

        public Task Handle(EnumMessage message, IMessageHandlerContext context)
        {
            log.Info("Enum message received.  Returning.");
            var responseMessage = new EnumMessageResponse {Status = Status.OK};
            return context.Reply(responseMessage);
        }
    }

    class IntMessageHandler : IHandleMessages<IntMessage>
    {
        static ILog log = LogManager.GetLogger<EnumMessageHandler>();

        public Task Handle(IntMessage message, IMessageHandlerContext context)
        {
            log.Info("Int message received.  Returning.");
            var responseMessage = new IntMessageResponse {Value = 10};
            return context.Reply(responseMessage);
        }
    }

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
