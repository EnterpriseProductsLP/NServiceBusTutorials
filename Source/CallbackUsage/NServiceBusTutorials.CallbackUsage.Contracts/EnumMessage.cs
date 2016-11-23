using NServiceBus;

namespace NServiceBusTutorials.CallbackUsage.Contracts
{
    public class EnumMessage : IMessage
    {
    }

    public class EnumMessageResponse : IMessage
    {
        public Status Status;
    }

    public class ObjectMessage : IMessage
    {
    }

    public class ObjectMessageResponse : IMessage
    {
        public string Property { get; set; }
    }

    public class IntMessage : IMessage
    {
    }

    public class IntMessageResponse : IMessage
    {
        public int Value;
    }
}
