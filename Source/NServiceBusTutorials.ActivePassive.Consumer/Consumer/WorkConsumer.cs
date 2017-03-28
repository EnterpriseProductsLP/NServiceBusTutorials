using System;
using System.Collections.Generic;
using System.Data.SqlClient;
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
        private readonly Dictionary<StateTransition, State> _allowedTransitions;

        private readonly IManageDistributedLocks _distributedLockManager;

        private readonly EndpointConfigurationBuilder _endpointConfigurationBuilder;

        private readonly object _endpointLock = new object();

        private readonly Timer _heartbeatTimer = new Timer(2000);

        private readonly Timer _startupTimer = new Timer(10000);

        private readonly object _stateLock = new object();

        private State _currentState;

        private IEndpointInstance _endpointInstance;

        public WorkConsumer(EndpointConfigurationBuilder endpointConfigurationBuilder, IManageDistributedLocks distributedLockManager)
        {
            CurrentState = State.Initializing;

            _endpointConfigurationBuilder = endpointConfigurationBuilder;
            _distributedLockManager = distributedLockManager;
            _heartbeatTimer.Elapsed += OnHeartbeatTimerElapsed;
            _startupTimer.Elapsed += OnStartupTimerElapsed;

            _allowedTransitions = new Dictionary<StateTransition, State>
                                      {
                                          // Transitions from Initializing
                                          {
                                              new StateTransition(State.Initializing, Command.Run), State.Running
                                          },
                                          {
                                              new StateTransition(State.Initializing, Command.Stop), State.Stopped
                                          },
                                          {
                                              new StateTransition(State.Initializing, Command.Wait), State.Waiting
                                          },

                                          // Transitions from Paused
                                          {
                                              new StateTransition(State.Paused, Command.Run), State.Running
                                          },
                                          {
                                              new StateTransition(State.Paused, Command.Stop), State.Stopped
                                          },
                                          {
                                              new StateTransition(State.Paused, Command.Wait), State.Waiting
                                          },

                                          // Transitions from Running
                                          {
                                              new StateTransition(State.Running, Command.Pause), State.Paused
                                          },
                                          {
                                              new StateTransition(State.Running, Command.Stop), State.Stopped
                                          },
                                          {
                                              new StateTransition(State.Running, Command.Wait), State.Waiting
                                          },

                                          // Transitions from Waiting
                                          {
                                              new StateTransition(State.Waiting, Command.Pause), State.Paused
                                          },
                                          {
                                              new StateTransition(State.Waiting, Command.Run), State.Running
                                          },
                                          {
                                              new StateTransition(State.Waiting, Command.Stop), State.Stopped
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
                            if (CanGetOrUpdateDistributedLock())
                            {
                                OnRun();
                            }
                            else
                            {
                                nextState = DoStateTransition(Command.Wait);
                            }
                        }
                        catch
                        {
                            nextState = DoStateTransition(Command.Wait);
                        }
                        break;

                    case Command.Stop:
                        OnStop();
                        break;

                    case Command.Wait:
                        OnWait();
                        break;
                }

                CurrentState = nextState;
            }

            return CurrentState;
        }

        public void Pause()
        {
            DoStateTransition(Command.Pause);
        }

        public void Resume()
        {
            DoStateTransition(Command.Run);
        }

        public void Run()
        {
            try
            {
                DoStateTransition(Command.Run);
            }
            catch (SqlException ex)
            {
                ConsoleUtilities.WriteLineWithColor($"Exception when trying to run: {ex.Message}", ConsoleColor.Red);
                WaitOnSqlException();
            }
            catch (Exception ex)
            {
                ConsoleUtilities.WriteLineWithColor($"Exception when trying to run: {ex.Message}", ConsoleColor.Red);
                Wait();
            }
        }

        public void Stop()
        {
            DoStateTransition(Command.Stop);
        }

        private bool CanGetOrUpdateDistributedLock()
        {
            return _distributedLockManager.GetOrMaintainLock();
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

        private void OnHeartbeatTimerElapsed(object sender, ElapsedEventArgs e)
        {
            try
            {
                Console.Write("Trying to heartbeat distributed lock:  ");
                if (CanGetOrUpdateDistributedLock())
                {
                    ConsoleUtilities.WriteLineWithColor("Success!", ConsoleColor.Green);
                }
                else
                {
                    ConsoleUtilities.WriteLineWithColor("Failed!  Waiting.", ConsoleColor.Red);
                    Wait();
                }
            }
            catch (SqlException ex)
            {
                ConsoleUtilities.WriteLineWithColor($"Heartbeat failed: {ex.Message}", ConsoleColor.Red);
                WaitOnSqlException();
            }
            catch (Exception ex)
            {
                ConsoleUtilities.WriteLineWithColor($"Heartbeat failed: {ex.Message}", ConsoleColor.Red);
                Wait();
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
                    Run();
                }
                else
                {
                    Console.WriteLine("Failed!  Waiting.");
                    _startupTimer.Start();
                }
            }
            catch (Exception ex)
            {
                ConsoleUtilities.WriteLineWithColor($"Exception:  {ex.Message}", ConsoleColor.Red);
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

            try
            {
                lock (_endpointLock)
                {
                    var endpointConfiguration = _endpointConfigurationBuilder.GetEndpointConfiguration(Endpoints.Consumer, errorQueue: Endpoints.ErrorQueue);
                    var recoverability = endpointConfiguration.Recoverability();
                    recoverability.Immediate(
                        immediate =>
                            {
                                immediate.NumberOfRetries(0);
                            });
                    var startableEndpoint = Endpoint.Create(endpointConfiguration).Inline();
                    _endpointInstance = startableEndpoint.Start().Inline();
                }
            }
            catch(Exception ex)
            {
                ConsoleUtilities.WriteLineWithColor($"Exception:  {ex.Message}", ConsoleColor.Red);
                Wait();
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

        private void Wait()
        {
            DoStateTransition(Command.Wait);
        }

        private void WaitOnSqlException()
        {
            _heartbeatTimer.Stop();
            StopEndpoint();
            _startupTimer.Start();
            _currentState = State.Waiting;
        }
    }
}