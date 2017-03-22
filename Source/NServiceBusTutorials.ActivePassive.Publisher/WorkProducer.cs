using System;
using System.Threading;

using NServiceBus;

using NServiceBusTutorials.ActivePassive.Contracts;
using NServiceBusTutorials.Common;
using NServiceBusTutorials.Common.Extensions;

namespace NServiceBusTutorials.ActivePassive.Publisher
{
    internal class WorkProducer : Worker
    {
        private readonly IStartableEndpoint _startableEndpoint;

        private IEndpointInstance _endpointInstance;

        public WorkProducer(IStartableEndpoint startableEndpoint)
        {
            _startableEndpoint = startableEndpoint;
        }

        protected override void OnRunning()
        {
            _endpointInstance = _startableEndpoint.Start().Inline();
            SetRunning();
        }

        protected override void OnPausing()
        {
            SetPaused();
        }

        protected override void OnResuming()
        {
            SetRunning();
        }

        protected override void OnStopping()
        {
            _endpointInstance.Stop().Inline();
        }

        protected override void DoStep()
        {
            var identifier = Guid.NewGuid();
            var workEvent = new WorkEvent { Identifier = identifier };
            _endpointInstance.Publish(message: workEvent).Inline();
            Console.WriteLine($"Sent a WorkEvent with Identifier: {identifier}");
            Thread.Sleep(2000);
        }
    }
}