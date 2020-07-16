using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace FakeSurveyGenerator.Infrastructure.Persistence
{
    // Courtesy of this gist: https://gist.github.com/ChristopherHaws/b1c54b95838f1513bfb74fa1c8e408f3
    public static class AsyncUtilities
    {
        /// <summary>
        /// Execute's an async Task{T} method which has a void return value synchronously
        /// </summary>
        /// <param name="task">Task{T} method to execute</param>
        public static void RunSync(Func<Task> task)
        {
            var oldContext = SynchronizationContext.Current;
            var sync = new ExclusiveSynchronizationContext();
            SynchronizationContext.SetSynchronizationContext(sync);

            sync.Post(async _ =>
            {
                try
                {
                    await task();
                }
                catch (Exception e)
                {
                    sync.InnerException = e;
                    throw;
                }
                finally
                {
                    sync.EndMessageLoop();
                }
            }, null);

            sync.BeginMessageLoop();

            SynchronizationContext.SetSynchronizationContext(oldContext);
        }

        /// <summary>
        /// Execute's an async Task{T} method which has a T return type synchronously
        /// </summary>
        /// <typeparam name="T">Return Type</typeparam>
        /// <param name="task">Task{T} method to execute</param>
        /// <returns></returns>
        public static T RunSync<T>(Func<Task<T>> task)
        {
            var oldContext = SynchronizationContext.Current;
            var sync = new ExclusiveSynchronizationContext();
            SynchronizationContext.SetSynchronizationContext(sync);
            T ret = default;

            sync.Post(async _ =>
            {
                try
                {
                    ret = await task();
                }
                catch (Exception e)
                {
                    sync.InnerException = e;
                    throw;
                }
                finally
                {
                    sync.EndMessageLoop();
                }
            }, null);
            sync.BeginMessageLoop();
            SynchronizationContext.SetSynchronizationContext(oldContext);
            return ret;
        }

        private class ExclusiveSynchronizationContext : SynchronizationContext, IDisposable
        {
            private readonly AutoResetEvent workItemsWaiting = new AutoResetEvent(false);
            private readonly Queue<Tuple<SendOrPostCallback, Object>> items = new Queue<Tuple<SendOrPostCallback, Object>>();
            private bool done;

            public Exception InnerException { get; set; }

            public void Dispose()
            {
                this.workItemsWaiting?.Dispose();
            }

            public override void Send(SendOrPostCallback d, Object state)
            {
                throw new NotSupportedException("We cannot send to our same thread");
            }

            public override void Post(SendOrPostCallback d, Object state)
            {
                lock (this.items)
                {
                    this.items.Enqueue(Tuple.Create(d, state));
                }

                this.workItemsWaiting.Set();
            }

            public void EndMessageLoop()
            {
                this.Post(_ => this.done = true, null);
            }

            public void BeginMessageLoop()
            {
                while (!this.done)
                {
                    Tuple<SendOrPostCallback, object> task = null;
                    lock (this.items)
                    {
                        if (this.items.Count > 0)
                        {
                            task = this.items.Dequeue();
                        }
                    }

                    if (task != null)
                    {
                        task.Item1(task.Item2);
                        if (this.InnerException != null) // the method threw an exeption
                        {
                            throw new AggregateException("AsyncHelpers.Run method threw an exception.", this.InnerException);
                        }
                    }
                    else
                    {
                        this.workItemsWaiting.WaitOne();
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
