using System.Threading.Tasks;
using NServiceBus;
using NServiceBus.Logging;
using NServiceBusTutorials.CallbackUsage.Contracts;

namespace NServiceBusTutorials.CallbackUsage.Receiver.MessageHandlers
{
    class ObjectMessageHandler : IHandleMessages<ObjectMessage>
    {
        private static readonly ILog Log = LogManager.GetLogger<ObjectMessageHandler>();

        public Task Handle(ObjectMessage message, IMessageHandlerContext context)
        {
            Log.Info("Object message received.  Returning.");
            var responseMessage = new ObjectMessageResponse {Property = "PropertyValue"};
            return context.Reply(responseMessage);
        }
    }
}