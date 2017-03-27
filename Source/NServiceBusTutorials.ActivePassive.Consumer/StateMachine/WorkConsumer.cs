using System;
using System.Collections.Generic;
using System.Timers;

using NServiceBus;

using NServiceBusTutorials.ActivePassive.Consumer.Interfaces;
using NServiceBusTutorials.ActivePassive.Contracts;
using NServiceBusTutorials.Common;
using NServiceBusTutorials.Common.Extensions;

namespace NServiceBusTutorials.ActivePassive.Consumer.StateMachine
{
    internal class WorkConsumer
    {
        private readonly Dictionary<StateTransition, ProcessState> _allowedTransitions;

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

            CurrentState = ProcessState.Initializing;
            _allowedTransitions = new Dictionary<StateTransition, ProcessState>
                                      {
                                          // Transitions from Initializing
                                          {
                                              new StateTransition(ProcessState.Initializing, Command.Run), ProcessState.Running
                                          },
                                          {
                                              new StateTransition(ProcessState.Initializing, Command.Stop), ProcessState.Stopped
                                          },
                                          {
                                              new StateTransition(ProcessState.Initializing, Command.Wait), ProcessState.Waiting
                                          },

                                          // Transitions from Paused
                                          {
                                              new StateTransition(ProcessState.Paused, Command.Run), ProcessState.Running
                                          },
                                          {
                                              new StateTransition(ProcessState.Paused, Command.Stop), ProcessState.Stopped
                                          },
                                          {
                                              new StateTransition(ProcessState.Paused, Command.Wait), ProcessState.Waiting
                                          },

                                          // Transitions from Running
                                          {
                                              new StateTransition(ProcessState.Running, Command.Pause), ProcessState.Paused
                                          },
                                          {
                                              new StateTransition(ProcessState.Running, Command.Stop), ProcessState.Stopped
                                          },
                                          {
                                              new StateTransition(ProcessState.Running, Command.Wait), ProcessState.Waiting
                                          },

                                          // Transitions from Waiting
                                          {
                                              new StateTransition(ProcessState.Waiting, Command.Pause), ProcessState.Paused
                                          },
                                          {
                                              new StateTransition(ProcessState.Waiting, Command.Run), ProcessState.Running
                                          },
                                          {
                                              new StateTransition(ProcessState.Waiting, Command.Stop), ProcessState.Stopped
                                          }
                                      };
        }

        public ProcessState CurrentState { get; private set; }

        public void Pause()
        {
            SetState(Command.Pause);
        }

        public void Resume()
        {
            SetState(Command.Run);
        }

        public void Stop()
        {
            SetState(Command.Stop);
        }

        public void Start()
        {
            try
            {
                SetState(Command.Run);
            }
            catch
            {
                SetState(Command.Stop);
            }
        }

        private ProcessState GetNext(Command command)
        {
            var stateTransition = new StateTransition(CurrentState, command);
            if (_allowedTransitions.TryGetValue(stateTransition, out ProcessState nextState))
            {
                return nextState;
            }

            throw new Exception($"Invalid transition:  {CurrentState} --> {command}");
        }

        public ProcessState SetState(Command command)
        {
            var nextState = GetNext(command);

            switch (command)
            {
                case Command.Pause:
                    OnPause();
                    break;

                case Command.Run:
                    try
                    {
                        if (CanGetOrUpdateDistributedLock())
                        {
                            OnRun();
                        }
                    }
                    catch
                    {
                        nextState = SetState(Command.Pause);
                    }
                    break;

                case Command.Stop:
                    OnStop();
                    break;

                case Command.Wait:
                    OnWait();
                    break;

                default: throw new ArgumentOutOfRangeException(nameof(command), command, null);
            }

            CurrentState = nextState;

            return CurrentState;
        }

        private bool CanGetOrUpdateDistributedLock()
        {
            return _distributedLockManager.GetOrMaintainLock();
        }

        private bool CannotGetOrUpdateDistributredLock()
        {
            return !CanGetOrUpdateDistributedLock();
        }

        private void OnHeartbeatTimerElapsed(object sender, ElapsedEventArgs e)
        {
            try
            {
                if (CannotGetOrUpdateDistributredLock())
                {
                    SetState(Command.Wait);
                }
            }
            catch
            {
                SetState(Command.Pause);
            }
        }

        private void OnPause()
        {
            _heartbeatTimer.Stop();
            _startupTimer.Stop();
            StopEndpoint();
            _distributedLockManager.ReleaseLock();
        }

        private void OnRun()
        {
            _startupTimer.Stop();
            StartEndpoint();
            _heartbeatTimer.Start();
        }

        private void OnStartupTimerElapsed(object sender, ElapsedEventArgs e)
        {
            try
            {
                _startupTimer.Stop();
                if (CanGetOrUpdateDistributedLock())
                {
                    SetState(Command.Run);
                }
                else
                {
                    _startupTimer.Start();
                }
            }
            catch
            {
                _startupTimer.Start();
            }
        }

        private void OnStop()
        {
            _startupTimer.Stop();
            _heartbeatTimer.Stop();
            StopEndpoint();
            _distributedLockManager.ReleaseLock();
        }

        private void OnWait()
        {
            _heartbeatTimer.Stop();
            StopEndpoint();
            _distributedLockManager.ReleaseLock();
            _startupTimer.Start();
        }

        private void StartEndpoint()
        {
            // Stop any existing endpoint so we don't have two.
            StopEndpoint();

            var endpointConfiguration = _endpointConfigurationBuilder.GetEndpointConfiguration(Endpoints.Consumer, errorQueue: Endpoints.ErrorQueue);
            var recoverability = endpointConfiguration.Recoverability();
            recoverability.Immediate(
                immediate =>
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