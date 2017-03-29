﻿using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using System.Timers;
using NServiceBus;

using NServiceBusTutorials.ActivePassive.Common;
using NServiceBusTutorials.ActivePassive.Consumer.Interfaces;
using NServiceBusTutorials.Common;

namespace NServiceBusTutorials.ActivePassive.Consumer.Consumer
{
    internal class ActivePassiveEndpointInstance : IActivePassiveEndpointInstance
    {
        private readonly Dictionary<StateTransition, State> _allowedTransitions;

        private readonly IManageDistributedLocks _distributedLockManager;

        private readonly IBuildEndpointInstances _endpointInstanceBuilder;

        private readonly IList<Subscription> _subscriptions = new List<Subscription>();

        private readonly Timer _heartbeatTimer = new Timer(2000);

        private readonly Timer _startupTimer = new Timer(10000);

        private readonly object _stateLock = new object();

        private State _currentState;

        private IEndpointInstance _endpointInstance;

        public ActivePassiveEndpointInstance(IBuildEndpointInstances endpointInstanceBuilder, IManageDistributedLocks distributedLockManager)
        {
            CurrentState = State.Initializing;

            _endpointInstanceBuilder = endpointInstanceBuilder;
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
                                OnStart();
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

        public async Task Pause()
        {
            await Task.Run(() => DoStateTransition(Command.Pause));
        }

        public async Task Resume()
        {
            await Task.Run(() => DoStateTransition(Command.Run));
        }

        public async Task<IEndpointInstance> Start()
        {
            try
            {
                await Task.Run(() => DoStateTransition(Command.Run));
                return _endpointInstance;
            }
            catch (SqlException ex)
            {
                ConsoleUtilities.WriteLineWithColor($"Exception when trying to run: {ex.Message}", ConsoleColor.Red);
                WaitOnSqlException();
                // TODO:  What is the correct thing to do if the endpoints fails to start?
                throw;
            }
            catch (Exception ex)
            {
                ConsoleUtilities.WriteLineWithColor($"Exception when trying to run: {ex.Message}", ConsoleColor.Red);
                Wait();
                // TODO:  What is the correct thing to do if the endpoints fails to start?
                throw;
            }
        }

        public Task Send(object message, SendOptions options)
        {
            if (_endpointInstance == null)
            {
                throw new Exception(message: "Endpoint not started.  Cannot Send.");
            }

            return _endpointInstance.Send(message, options);
        }

        public Task Send<T>(Action<T> messageConstructor, SendOptions options)
        {
            if (_endpointInstance == null)
            {
                throw new Exception(message: "Endpoint not started.  Cannot Send.");
            }

            return _endpointInstance.Send(messageConstructor, options);
        }

        public Task Publish(object message, PublishOptions options)
        {
            if (_endpointInstance == null)
            {
                throw new Exception(message: "Endpoint not started.  Cannot Publish.");
            }

            return _endpointInstance.Publish(message, options);
        }

        public Task Publish<T>(Action<T> messageConstructor, PublishOptions publishOptions)
        {
            if (_endpointInstance == null)
            {
                throw new Exception(message: "Endpoint not started.  Cannot Publish.");
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
            throw new NotImplementedException(message: "Unsubscrive not supported.");
        }

        public async Task Stop()
        {
            await Task.Run(() => DoStateTransition(Command.Stop));
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
                Console.Write(value: "Trying to heartbeat distributed lock:  ");
                if (CanGetOrUpdateDistributedLock())
                {
                    ConsoleUtilities.WriteLineWithColor(message: "Success!", color: ConsoleColor.Green);
                }
                else
                {
                    ConsoleUtilities.WriteLineWithColor(message: "Failed!  Waiting.", color: ConsoleColor.Red);
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

        private async void OnPause()
        {
            _heartbeatTimer.Stop();
            _startupTimer.Stop();
            await StopEndpoint();
            _distributedLockManager.ReleaseLock();
        }

        private async void OnStart()
        {
            _startupTimer.Stop();
            await StartEndpoint();
            _heartbeatTimer.Start();
        }

        private async void OnStartupTimerElapsed(object sender, ElapsedEventArgs e)
        {
            try
            {
                Console.Write(value: "Trying to get distributed lock:  ");
                _startupTimer.Stop();
                if (CanGetOrUpdateDistributedLock())
                {
                    Console.WriteLine(value: "Got the lock!  Running.");
                    await Start();
                }
                else
                {
                    Console.WriteLine(value: "Failed!  Waiting.");
                    _startupTimer.Start();
                }
            }
            catch (Exception ex)
            {
                ConsoleUtilities.WriteLineWithColor($"Exception:  {ex.Message}", ConsoleColor.Red);
                _startupTimer.Start();
            }
        }

        private async void OnStop()
        {
            _startupTimer.Stop();
            _heartbeatTimer.Stop();
            await StopEndpoint();
            _distributedLockManager.ReleaseLock();
        }

        private async void OnWait()
        {
            _heartbeatTimer.Stop();
            await StopEndpoint();
            _distributedLockManager.ReleaseLock();
            _startupTimer.Start();
        }

        private async Task<IEndpointInstance> StartEndpoint()
        {
            await StopEndpoint();

            try
            {
                var startableEndpoint = await _endpointInstanceBuilder.Create();
                _endpointInstance = await startableEndpoint.Start();

                var subscriptionTasks = _subscriptions
                    .Select(subscription => _endpointInstance.Subscribe(subscription.EventType, subscription.Options))
                    .ToArray();

                Task.WaitAll(subscriptionTasks);

                return _endpointInstance;
            }
            catch (Exception ex)
            {
                ConsoleUtilities.WriteLineWithColor($"Exception:  {ex.Message}", ConsoleColor.Red);
                Wait();
                // TODO:  What is the correct thing to do if the endpoints fails to start?
                throw;
            }
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

        private async void Wait()
        {
            await Task.Run(() => DoStateTransition(Command.Wait));
        }

        private async void WaitOnSqlException()
        {
            _heartbeatTimer.Stop();
            await StopEndpoint();
            _startupTimer.Start();
            _currentState = State.Waiting;
        }
    }
}