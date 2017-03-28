using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using NServiceBus;

using NServiceBusTutorials.ActivePassive.Common;
using NServiceBusTutorials.Common;

namespace NServiceBusTutorials.ActivePassive.Consumer.Consumer
{
    public class ActivePassiveEndpointInstance : IActivePassiveEndpointInstance
    {
        private readonly IEndpointInstanceBuilder _endpointInstanceBuilder;

        private readonly IList<Subscription> _subscriptions = new List<Subscription>();

        private IEndpointInstance _endpointInstance;

        public ActivePassiveEndpointInstance(IEndpointInstanceBuilder endpointInstanceBuilder)
        {
            _endpointInstanceBuilder = endpointInstanceBuilder;
        }

        public async Task<IEndpointInstance> Start()
        {
            return await StartEndpoint();
        }

        public Task Send(object message, SendOptions options)
        {
            if (_endpointInstance == null)
            {
                throw new Exception("Endpoint not started.  Cannot Send.");
            }

            return _endpointInstance.Send(message, options);
        }

        public Task Send<T>(Action<T> messageConstructor, SendOptions options)
        {
            if (_endpointInstance == null)
            {
                throw new Exception("Endpoint not started.  Cannot Send.");
            }

            return _endpointInstance.Send(messageConstructor, options);
        }

        public Task Publish(object message, PublishOptions options)
        {
            if (_endpointInstance == null)
            {
                throw new Exception("Endpoint not started.  Cannot Publish.");
            }

            return _endpointInstance.Publish(message, options);
        }

        public Task Publish<T>(Action<T> messageConstructor, PublishOptions publishOptions)
        {
            if (_endpointInstance == null)
            {
                throw new Exception("Endpoint not started.  Cannot Publish.");
            }

            return _endpointInstance.Publish(messageConstructor, publishOptions);
        }

        public Task Subscribe(Type eventType, SubscribeOptions options)
        {
            _subscriptions.Add(new Subscription(eventType, options));

            return _endpointInstance?.Subscribe(eventType, options) ?? Task.CompletedTask;
        }

        public Task Unsubscribe(Type eventType, UnsubscribeOptions options)
        {
            throw new NotImplementedException("Unsubscrive not supported.");
        }

        public async Task Stop()
        {
            await StopEndpoint();
        }

        private async Task<IEndpointInstance> StartEndpoint()
        {
            await StopEndpoint();

            var startableEndpoint = await _endpointInstanceBuilder.Create();
            _endpointInstance = await startableEndpoint.Start();

            var subscriptionTasks = _subscriptions
                .Select(subscription => _endpointInstance.Subscribe(subscription.EventType, subscription.Options))
                .ToArray();

            Task.WaitAll(subscriptionTasks);

            return _endpointInstance;
        }

        private async Task StopEndpoint()
        {
            if (_endpointInstance == null)
            {
                return;
            }

            await _endpointInstance.Stop();
            _endpointInstance = null;
        }
    }
}