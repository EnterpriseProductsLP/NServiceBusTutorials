using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Timers;
using NServiceBus;
using NServiceBusTutorials.ActivePassive.Common;
using NServiceBusTutorials.ActivePassive.Consumer.Interfaces;
using NServiceBusTutorials.Common;
using NServiceBusTutorials.Common.Extensions;

namespace NServiceBusTutorials.ActivePassive.Consumer
{
    internal class ActivePassiveEndpointInstance : IActivePassiveEndpointInstance
    {
        private enum Command
        {
            Pause,

            Run,

            Stop,

            Wait
        }

        private enum State
        {
            Initializing,

            Paused,

            Running,

            Stopped,

            Waiting
        }

        private class StateTransition
        {
            private readonly Command _command;

            private readonly State _currentState;

            public StateTransition(State currentState, Command command)
            {
                _currentState = currentState;
                _command = command;
            }

            public override bool Equals(object obj)
            {
                if (ReferenceEquals(null, obj))
                {
                    return false;
                }

                if (ReferenceEquals(this, obj))
                {
                    return true;
                }

                return obj.GetType() == GetType() && Equals((StateTransition)obj);
            }

            public override int GetHashCode()
            {
                unchecked
                {
                    return (_command.GetHashCode() * 397) ^ (int)_currentState;
                }
            }

            private bool Equals(StateTransition other)
            {
                return Equals(_command, other._command) && _currentState == other._currentState;
            }
        }

        internal class Subscription
        {
            public Subscription(Type eventType, SubscribeOptions options)
            {
                EventType = eventType;
                Options = options;
            }

            public Type EventType { get; }

            public SubscribeOptions Options { get; }
        }

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

        private State DoStateTransition(Command command)
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

                            // TODO log
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

        public async Task Start()
        {
            await Task.Run(() => DoStateTransition(Command.Run));
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
            await Task.Run(() => DoStateTransition(Command.Stop));
        }

        private bool CanGetOrUpdateDistributedLock()
        {
            return _distributedLockManager.GetOrMaintainLock().Inline();
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
            _heartbeatTimer.Stop();
            Console.Write("Trying to heartbeat distributed lock:  ");
            if (CanGetOrUpdateDistributedLock())
            {
                ConsoleUtilities.WriteLineWithColor("Success!", ConsoleColor.Green);
                _heartbeatTimer.Start();
            }
            else
            {
                ConsoleUtilities.WriteLineWithColor("Failed!  Waiting.", ConsoleColor.Red);
                Wait();
            }
        }

        private async Task OnPause()
        {
            _heartbeatTimer.Stop();
            _startupTimer.Stop();
            await StopEndpoint();
            await _distributedLockManager.ReleaseLock();
        }

        private async Task OnStart()
        {
            _startupTimer.Stop();
            await StartEndpoint();
            _heartbeatTimer.Start();
        }

        private async void OnStartupTimerElapsed(object sender, ElapsedEventArgs e)
        {
            try
            {
                Console.Write("Trying to get distributed lock:  ");
                _startupTimer.Stop();
                if (CanGetOrUpdateDistributedLock())
                {
                    Console.WriteLine("Got the lock!  Running.");
                    await Start();
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

        private async Task OnStop()
        {
            _startupTimer.Stop();
            _heartbeatTimer.Stop();
            await StopEndpoint();
            await _distributedLockManager.ReleaseLock();
        }

        private async Task OnWait()
        {
            _heartbeatTimer.Stop();
            await StopEndpoint();
            await _distributedLockManager.ReleaseLock();
            _startupTimer.Start();
        }

        private async Task StartEndpoint()
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
            }
            catch (Exception ex)
            {
                ConsoleUtilities.WriteLineWithColor($"Exception:  {ex.Message}", ConsoleColor.Red);
                Wait();
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

        private async Task Wait()
        {
            await Task.Run(() => DoStateTransition(Command.Wait));
        }
    }
}