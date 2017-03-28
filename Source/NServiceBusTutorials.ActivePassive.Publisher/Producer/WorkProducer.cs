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
        private readonly Dictionary<StateTransition, State> _allowedTransitions;

        private readonly EndpointConfigurationBuilder _endpointConfigurationBuilder;

        private readonly object _endpointLock = new object();

        private readonly Timer _publicationTimer = new Timer(500);

        private readonly object _stateLock = new object();

        private State _currentState;

        private IEndpointInstance _endpointInstance;

        public WorkProducer(EndpointConfigurationBuilder endpointConfigurationBuilder)
        {
            CurrentState = State.Initializing;

            _endpointConfigurationBuilder = endpointConfigurationBuilder;
            _publicationTimer.Elapsed += OnPublicationTimerOnElapsed;

            _allowedTransitions = new Dictionary<StateTransition, State>
                                      {
                                          // Transitions from Initializing
                                          {
                                              new StateTransition(State.Initializing, Command.Run), State.Running
                                          },
                                          {
                                              new StateTransition(State.Initializing, Command.Stop), State.Stopped
                                          },

                                          // Transitions from Paused
                                          {
                                              new StateTransition(State.Paused, Command.Run), State.Running
                                          },
                                          {
                                              new StateTransition(State.Paused, Command.Stop), State.Stopped
                                          },

                                          // Transitions from Running
                                          {
                                              new StateTransition(State.Running, Command.Pause), State.Paused
                                          },
                                          {
                                              new StateTransition(State.Running, Command.Stop), State.Stopped
                                          }
                                      };
        }

        public bool Stopped
        {
            get
            {
                lock (_stateLock)
                {
                    return _currentState == State.Stopped;
                }
            }
        }

        private State CurrentState
        {
            get
            {
                lock (_stateLock)
                {
                    return _currentState;
                }
            }
            set
            {
                lock (_stateLock)
                {
                    _currentState = value;
                }
            }
        }

        private void OnPublicationTimerOnElapsed(object sender, ElapsedEventArgs elapsedEventArgs)
        {
            var identifier = Guid.NewGuid();
            var workEvent = new WorkCommand
                                {
                                    Identifier = identifier
                                };
            _endpointInstance.Send(Endpoints.Consumer, workEvent).Inline();
            Console.WriteLine($"Sent a WorkEvent with Identifier: {identifier}");
        }

        public void Pause()
        {
            DoStateTransition(Command.Pause);
        }

        public void Resume()
        {
            DoStateTransition(Command.Run);
        }

        public State DoStateTransition(Command command)
        {
            Console.WriteLine($"Attempting to {command}");
            lock (_stateLock)
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
                            OnRun();
                        }
                        catch
                        {
                            nextState = DoStateTransition(Command.Stop);
                        }
                        break;

                    case Command.Stop:
                        OnStop();
                        break;
                }

                CurrentState = nextState;
            }

            return CurrentState;
        }

        public void Run()
        {
            try
            {
                DoStateTransition(Command.Run);
            }
            catch
            {
                DoStateTransition(Command.Stop);
            }
        }

        public void Stop()
        {
            DoStateTransition(Command.Stop);
        }

        private State GetNext(Command command)
        {
            lock (_stateLock)
            {
                State nextState;
                var stateTransition = new StateTransition(CurrentState, command);
                if (_allowedTransitions.TryGetValue(stateTransition, out nextState))
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

            try
            {
                lock (_endpointLock)
                {
                    var endpointConfiguration = _endpointConfigurationBuilder.GetEndpointConfiguration(Endpoints.Publisher, errorQueue: Endpoints.ErrorQueue);
                    var startableEndpoint = Endpoint.Create(endpointConfiguration).Inline();
                    _endpointInstance = startableEndpoint.Start().Inline();
                }
            }
            catch (Exception ex)
            {
                ConsoleUtilities.WriteLineWithColor($"Exception:  {ex.Message}", ConsoleColor.Red);
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