using NServiceBus;

namespace NServiceBusTutorials.CallbackUsage.Contracts
{
    public class EnumMessageResponse : IMessage
    {
        public Status Status;
    }
}