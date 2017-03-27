using System;
using System.Collections.Generic;
using System.Timers;
using NServiceBus;
using NServiceBusTutorials.ActivePassive.Consumer.Interfaces;
using NServiceBusTutorials.ActivePassive.Contracts;
using NServiceBusTutorials.Common;
using NServiceBusTutorials.Common.Extensions;

namespace NServiceBusTutorials.ActivePassive.Consumer.Consumer
{
    internal class WorkConsumer
    {
        private readonly Dictionary<StateTransition, ProcessState> _allowedTransitions;

        private readonly IManageDistributedLocks _distributedLockManager;

        private readonly EndpointConfigurationBuilder _endpointConfigurationBuilder;

        private readonly Timer _heartbeatTimer = new Timer(2000);

        private readonly Timer _startupTimer = new Timer(10000);

        private readonly object _stateLock = new object();

        private bool _canTransition = true;

        private ProcessState _currentState;

        private IEndpointInstance _endpointInstance;

        public WorkConsumer(EndpointConfigurationBuilder endpointConfigurationBuilder, IManageDistributedLocks distributedLockManager)
        {
            CurrentState = ProcessState.Initializing;

            _endpointConfigurationBuilder = endpointConfigurationBuilder;
            _distributedLockManager = distributedLockManager;
            _heartbeatTimer.Elapsed += OnHeartbeatTimerElapsed;
            _startupTimer.Elapsed += OnStartupTimerElapsed;

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

        public bool CanPause
        {
            get
            {
                lock (_stateLock)
                {
                    return _canTransition && CanSetState(Command.Pause);
                }
            }
        }

        public bool CanResume
        {
            get
            {
                lock (_stateLock)
                {
                    return _canTransition && CanSetState(Command.Run);
                }
            }
        }

        public bool CanStart
        {
            get
            {
                lock (_stateLock)
                {
                    return _canTransition && CanSetState(Command.Run);
                }
            }
        }

        public bool CanStop
        {
            get
            {
                lock (_stateLock)
                {
                    return _canTransition && CanSetState(Command.Stop);
                }
            }
        }

        public ProcessState CurrentState
        {
            get
            {
                lock (_stateLock)
                {
                    return _currentState;
                }
            }
            private set
            {
                lock (_stateLock)
                {
                    _currentState = value;
                }
            }
        }

        public void Pause()
        {
            SetState(Command.Pause);
        }

        public void Resume()
        {
            SetState(Command.Run);
        }

        public ProcessState SetState(Command command)
        {
            Console.WriteLine($"Attempting to {command}");
            lock (_stateLock)
            {
                _canTransition = false;
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
                            else
                            {
                                nextState = SetState(Command.Wait);
                            }
                        }
                        catch
                        {
                            nextState = SetState(Command.Stop);
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
                _canTransition = true;
            }

            return CurrentState;
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

        public void Stop()
        {
            SetState(Command.Stop);
        }

        private bool CanGetOrUpdateDistributedLock()
        {
            return _distributedLockManager.GetOrMaintainLock();
        }

        private bool CanSetState(Command command)
        {
            lock (_stateLock)
            {
                var stateTransition = new StateTransition(CurrentState, command);
                return _allowedTransitions.ContainsKey(stateTransition);
            }
        }

        private ProcessState GetNext(Command command)
        {
            lock (_stateLock)
            {
                var stateTransition = new StateTransition(CurrentState, command);
                if (_allowedTransitions.TryGetValue(stateTransition, out ProcessState nextState))
                {
                    return nextState;
                }
            }

            var exception = new Exception($"Invalid transition:  {CurrentState} --> {command}");
            Console.WriteLine(exception.Message);
            throw exception;
        }

        private void OnHeartbeatTimerElapsed(object sender, ElapsedEventArgs e)
        {
            try
            {
                Console.Write("Trying to heartbeat distributed lock:  ");
                if (CanGetOrUpdateDistributedLock())
                {
                    Console.WriteLine("Success!");
                }
                else
                {
                    Console.WriteLine("Failed!  Waiting.");
                    SetState(Command.Wait);
                }
            }
            catch
            {
                Console.WriteLine("Heartbeat failed.");
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
                Console.Write("Trying to get distributed lock:  ");
                _startupTimer.Stop();
                if (CanGetOrUpdateDistributedLock())
                {
                    Console.WriteLine("Got the lock!  Running.");
                    SetState(Command.Run);
                }
                else
                {
                    Console.WriteLine("Failed!  Waiting.");
                    _startupTimer.Start();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception:  {ex.Message}");
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
                immediate => { immediate.NumberOfRetries(1); });
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
