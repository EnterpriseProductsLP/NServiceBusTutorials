﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using NServiceBus;
using NServiceBus.Performance.TimeToBeReceived;
using NServiceBus.Routing;
using NServiceBus.Transport;

namespace NServiceBusTutorials.FileSystemTransport.Transport
{
    internal class FileTransportInfrastructure : TransportInfrastructure
    {
        public override TransportReceiveInfrastructure ConfigureReceiveInfrastructure()
        {
            return new TransportReceiveInfrastructure(
                messagePumpFactory: () => new FileTransportMessagePump(),
                queueCreatorFactory: () => new FileTransportQueueCreator(),
                preStartupCheck: () => Task.FromResult(StartupCheckResult.Success)
            );
        }

        public override TransportSendInfrastructure ConfigureSendInfrastructure()
        {
            return new TransportSendInfrastructure(
                dispatcherFactory: () => new Dispatcher(),
                preStartupCheck: () => Task.FromResult(StartupCheckResult.Success)
            );
        }

        public override TransportSubscriptionInfrastructure ConfigureSubscriptionInfrastructure()
        {
            throw new NotImplementedException();
        }

        public override EndpointInstance BindToLocalEndpoint(EndpointInstance endpointInstance)
        {
            return endpointInstance;
        }

        public override string ToTransportAddress(LogicalAddress logicalAddress)
        {
            var endpointInstance = logicalAddress.EndpointInstance;
            var discriminator = endpointInstance.Discriminator ?? "";
            var qualifier = logicalAddress.Qualifier ?? "";
            return Path.Combine(endpointInstance.Endpoint, discriminator, qualifier);
        }

        public override IEnumerable<Type> DeliveryConstraints
        {
            get
            {
                yield return typeof(DiscardIfNotReceivedBefore);
            }
        }

        public override TransportTransactionMode TransactionMode => TransportTransactionMode.ReceiveOnly;

        public override OutboundRoutingPolicy OutboundRoutingPolicy => new OutboundRoutingPolicy(
            sends: OutboundRoutingType.Unicast,
            publishes: OutboundRoutingType.Unicast,
            replies: OutboundRoutingType.Unicast
        );
    }
}
