// -----------------------------------------------------------------------
// <copyright file="WorkConsumer.cs" company="Enterprise Products Partners L.P. (Enterprise)">
// © Copyright 2012 - 2017, Enterprise Products Partners L.P. (Enterprise), All Rights Reserved.
// Permission to use, copy, modify, or distribute this software source code, binaries or
// related documentation, is strictly prohibited, without written consent from Enterprise.
// For inquiries about the software, contact Enterprise: Enterprise Products Company Law
// Department, 1100 Louisiana, 10th Floor, Houston, Texas 77002, phone 713-381-6500.
// </copyright>
// -----------------------------------------------------------------------
using System.Timers;

using NServiceBus;

using NServiceBusTutorials.ActivePassive.Consumer.Interfaces;
using NServiceBusTutorials.ActivePassive.Contracts;
using NServiceBusTutorials.Common;
using NServiceBusTutorials.Common.Extensions;

namespace NServiceBusTutorials.ActivePassive.Consumer
{
    internal class WorkConsumer : Worker
    {
        private readonly IManageDistributedLocks _distributedLockManager;

        private readonly EndpointConfigurationBuilder _endpointConfigurationBuilder;

        private readonly Timer _heartbeatTimer = new Timer(2000);

        private readonly Timer _startupTimer = new Timer(10000);

        private IEndpointInstance _endpointInstance;

        public WorkConsumer(EndpointConfigurationBuilder endpointConfigurationBuilder, IManageDistributedLocks distributedLockManager)
        {
            _endpointConfigurationBuilder = endpointConfigurationBuilder;
            _distributedLockManager = distributedLockManager;
            _heartbeatTimer.Elapsed += OnHeartbeatTimerElapsed;
            _startupTimer.Elapsed += OnStartupTimerElapsed;
        }

        protected override WorkerState OnPausing()
        {
            _heartbeatTimer.Stop();
            StopEndpoint();
            _distributedLockManager.ReleaseLock();
            _startupTimer.Start();
            return WorkerState.Paused;
        }

        protected override WorkerState OnResuming()
        {
            try
            {
                _startupTimer.Stop();
                StartEndpoint();
                _heartbeatTimer.Start();
                return WorkerState.Running;
            }
            catch
            {
                return WorkerState.Pausing;
            }
        }

        protected override WorkerState OnStarting()
        {
            try
            {
                if (CanGetOrUpdateDistributedLock())
                {
                    return OnResuming();
                }

                _startupTimer.Start();
                return WorkerState.Running;
            }
            catch
            {
                return WorkerState.Pausing;
            }
        }

        protected override void OnStopping()
        {
            _startupTimer.Stop();
            _heartbeatTimer.Stop();
            StopEndpoint();
            _distributedLockManager.ReleaseLock();
        }

        protected override void DoStep()
        {
        }

        private void OnHeartbeatTimerElapsed(object sender, ElapsedEventArgs e)
        {
            try
            {
                if (!CanGetOrUpdateDistributedLock())
                {
                    Pause();
                }
            }
            catch
            {
                Pause();
            }
        }

        private bool CanGetOrUpdateDistributedLock()
        {
            return _distributedLockManager.GetOrMaintainLock();
        }

        private void OnStartupTimerElapsed(object sender, ElapsedEventArgs e)
        {
            try
            {
                _startupTimer.Stop();
                if (CanGetOrUpdateDistributedLock())
                {
                    Resume();
                }
            }
            catch
            {
                _startupTimer.Start();
            }
        }

        private void StartEndpoint()
        {
            // Stop any existing endpoint so we don't have two.
            StopEndpoint();

            var endpointConfiguration = _endpointConfigurationBuilder.GetEndpointConfiguration(Endpoints.Consumer, errorQueue: Endpoints.ErrorQueue);
            var recoverability = endpointConfiguration.Recoverability();
            recoverability.Immediate(immediate =>
                {
                    immediate.NumberOfRetries(1);
                });
            var startableEndpoint = Endpoint.Create(endpointConfiguration).Inline();
            _endpointInstance = startableEndpoint.Start().Inline();
        }

        private void StopEndpoint()
        {
            if (_endpointInstance == null)
            {
                return;
            }

            _endpointInstance.Stop().Inline();
            _endpointInstance = null;
        }
    }
}