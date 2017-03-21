using System.Threading;

using NServiceBus;

using NServiceBusTutorials.ActivePassive.Contracts;
using NServiceBusTutorials.Common;
using NServiceBusTutorials.Common.Extensions;

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
            _endpointInstance = _startableEndpoint.Start().Inline();
            _endpointInstance.Subscribe<WorkEvent>().Inline();
        }

        protected override void TearDown()
        {
            _endpointInstance.Stop().Inline();
        }

        protected override void DoStep()
        {
            // All actual work is handled by a WorkEvent handler.
            Thread.Sleep(2000);
        }
    }
}

