using NServiceBus;

namespace NServiceBusTutorials.CallbackUsage.Contracts
{
    public class IntMessageResponse : IMessage
    {
        public int Value;
    }
}