using System;
using System.Threading;

using NServiceBus;

using NServiceBusTutorials.ActivePassive.Contracts;
using NServiceBusTutorials.Common;

namespace NServiceBusTutorials.ActivePassive.Publisher
{
    internal class MessagePublisher : Worker
    {
        private readonly IStartableEndpoint _startableEndpoint;

        private IEndpointInstance _endpointInstance;

        public MessagePublisher(IStartableEndpoint startableEndpoint)
        {
            _startableEndpoint = startableEndpoint;
        }

        protected override void Setup()
        {
            _endpointInstance = _startableEndpoint.Start().GetAwaiter().GetResult();
        }

        protected override void TearDown()
        {
            _endpointInstance.Stop().GetAwaiter().GetResult();
        }

        protected override void DoStep()
        {
            var identifier = Guid.NewGuid();
            var workMessage = new WorkQueued { Identifier = identifier };
            _endpointInstance.Publish(message: workMessage).GetAwaiter().GetResult();
            Console.WriteLine($"Sent a WorkQueued message with Identifier: {identifier}");
            Thread.Sleep(2000);
        }
    }
}