using NServiceBus;

namespace NServiceBusTutorials.CallbackUsage.Contracts
{
    public class ObjectMessageResponse : IMessage
    {
        public string Property { get; set; }
    }
}