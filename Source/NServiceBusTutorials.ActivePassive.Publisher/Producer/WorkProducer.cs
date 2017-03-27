using System;
using System.Collections.Generic;
using System.Timers;
using NServiceBus;
using NServiceBusTutorials.ActivePassive.Contracts;
using NServiceBusTutorials.Common;
using NServiceBusTutorials.Common.Extensions;

namespace NServiceBusTutorials.ActivePassive.Publisher.Producer
{
    internal class WorkProducer
    {
        private readonly Timer _publicationTimer = new Timer(500);

        private readonly Dictionary<StateTransition, ProcessState> _allowedTransitions;

        private readonly EndpointConfigurationBuilder _endpointConfigurationBuilder;

        private readonly object _stateLock = new object();

        private readonly object _endpointLock = new object();

        private bool _canTransition = true;

        private ProcessState _currentState;

        private IEndpointInstance _endpointInstance;

        public WorkProducer(EndpointConfigurationBuilder endpointConfigurationBuilder)
        {
            CurrentState = ProcessState.Initializing;

            _endpointConfigurationBuilder = endpointConfigurationBuilder;
            _publicationTimer.Elapsed += OnPublicationTimerOnElapsed;

            _allowedTransitions = new Dictionary<StateTransition, ProcessState>
            {
                // Transitions from Initializing
                {
                    new StateTransition(ProcessState.Initializing, Command.Run), ProcessState.Running
                },
                {
                    new StateTransition(ProcessState.Initializing, Command.Stop), ProcessState.Stopped
                },

                // Transitions from Paused
                {
                    new StateTransition(ProcessState.Paused, Command.Run), ProcessState.Running
                },
                {
                    new StateTransition(ProcessState.Paused, Command.Stop), ProcessState.Stopped
                },

                // Transitions from Running
                {
                    new StateTransition(ProcessState.Running, Command.Pause), ProcessState.Paused
                },
                {
                    new StateTransition(ProcessState.Running, Command.Stop), ProcessState.Stopped
                }
            };
        }

        private void OnPublicationTimerOnElapsed(object sender, ElapsedEventArgs elapsedEventArgs)
        {
            var identifier = Guid.NewGuid();
            var workEvent = new WorkEvent { Identifier = identifier };
            _endpointInstance.Publish(workEvent).Inline();
            Console.WriteLine($"Sent a WorkEvent with Identifier: {identifier}");
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
                            OnRun();
                        }
                        catch
                        {
                            nextState = SetState(Command.Stop);
                        }
                        break;

                    case Command.Stop:
                        OnStop();
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

        private void OnPause()
        {
            _publicationTimer.Stop();
            StopEndpoint();
        }

        private void OnRun()
        {
            StartEndpoint();
            _publicationTimer.Start();
        }

        private void OnStop()
        {
            _publicationTimer.Stop();
            StopEndpoint();
        }

        private void StartEndpoint()
        {
            // Stop any existing endpoint so we don't have two.
            StopEndpoint();

            lock (_endpointLock)
            {
                var endpointConfiguration = _endpointConfigurationBuilder.GetEndpointConfiguration(Endpoints.Publisher, errorQueue: Endpoints.ErrorQueue);
                var recoverability = endpointConfiguration.Recoverability();
                recoverability.Immediate(
                    immediate => { immediate.NumberOfRetries(1); });
                var startableEndpoint = Endpoint.Create(endpointConfiguration).Inline();
                _endpointInstance = startableEndpoint.Start().Inline();
            }
        }

        private void StopEndpoint()
        {
            lock (_endpointLock)
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
}
