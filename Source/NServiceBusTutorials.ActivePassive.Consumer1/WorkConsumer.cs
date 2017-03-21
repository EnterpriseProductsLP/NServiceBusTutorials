using System.Threading;

using NServiceBus;

using NServiceBusTutorials.Common;

namespace NServiceBusTutorials.ActivePassive.Consumer1
{
    internal class WorkConsumer : Worker
    {
        private readonly IStartableEndpoint _startableEndpoint;

        private IEndpointInstance _endpointInstance;

        public WorkConsumer(IStartableEndpoint startableEndpoint)
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
            // All actual work is handled by a WorkEvent handler.
            Thread.Sleep(2000);
        }
    }
}

