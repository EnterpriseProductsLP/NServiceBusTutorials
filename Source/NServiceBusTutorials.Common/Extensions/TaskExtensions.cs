using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace NServiceBusTutorials.Common.Extensions
{
    public static class TaskExtensions
    {
        public static void Ignore(this Task task)
        {
        }

        public static void Inline(this Task task)
        {
            AsyncHelpers.RunSync(() => task);
        }

        public static TResult Inline<TResult>(this Task<TResult> task)
        {
            return AsyncHelpers.RunSync(() => task);
        }

        private static class AsyncHelpers
        {
            public static void RunSync(Func<Task> task)
            {
                var oldContext = SynchronizationContext.Current;
                var synch = new ExclusiveSynchronizationContext();
                SynchronizationContext.SetSynchronizationContext(synch);
                synch.Post(
                    async λ =>
                        {
                            try
                            {
                                await task();
                            }
                            catch (Exception e)
                            {
                                synch.InnerException = e;
                                throw;
                            }
                            finally
                            {
                                synch.EndMessageLoop();
                            }
                        },
                    null);
                synch.BeginMessageLoop();

                SynchronizationContext.SetSynchronizationContext(oldContext);
            }

            public static T RunSync<T>(Func<Task<T>> task)
            {
                var oldContext = SynchronizationContext.Current;
                var synch = new ExclusiveSynchronizationContext();
                SynchronizationContext.SetSynchronizationContext(synch);
                var ret = default(T);
                synch.Post(
                    async λ =>
                        {
                            try
                            {
                                ret = await task();
                            }
                            catch (Exception e)
                            {
                                synch.InnerException = e;
                                throw;
                            }
                            finally
                            {
                                synch.EndMessageLoop();
                            }
                        },
                    null);
                synch.BeginMessageLoop();
                SynchronizationContext.SetSynchronizationContext(oldContext);
                return ret;
            }

            private class ExclusiveSynchronizationContext : SynchronizationContext
            {
                readonly Queue<Tuple<SendOrPostCallback, object>> items =
                    new Queue<Tuple<SendOrPostCallback, object>>();

                private readonly AutoResetEvent workItemsWaiting = new AutoResetEvent(false);

                private bool done;

                public Exception InnerException { private get; set; }

                public override void Send(SendOrPostCallback d, object state)
                {
                    throw new NotSupportedException("We cannot send to our same thread");
                }

                public override void Post(SendOrPostCallback d, object state)
                {
                    lock (items)
                    {
                        items.Enqueue(Tuple.Create(d, state));
                    }
                    workItemsWaiting.Set();
                }

                public void EndMessageLoop()
                {
                    Post(λ => done = true, null);
                }

                public void BeginMessageLoop()
                {
                    while (!done)
                    {
                        Tuple<SendOrPostCallback, object> task = null;
                        lock (items)
                        {
                            if (items.Count > 0)
                            {
                                task = items.Dequeue();
                            }
                        }
                        if (task != null)
                        {
                            task.Item1(task.Item2);
                            if (InnerException != null) // the method threw an exeption
                            {
                                throw new AggregateException("AsyncHelpers.Run method threw an exception.", InnerException);
                            }
                        }
                        else
                        {
                            workItemsWaiting.WaitOne();
                        }
                    }
                }

                public override SynchronizationContext CreateCopy()
                {
                    return this;
                }
            }
        }
    }
}