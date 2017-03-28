using System;

using NServiceBus;

namespace NServiceBusTutorials.ActivePassive.Consumer.Consumer
{
    internal class Subscription
    {
        public Subscription(Type eventType, SubscribeOptions options)
        {
            EventType = eventType;
            Options = options;
        }

        public Type EventType { get; }

        public SubscribeOptions Options { get; }
    }
}