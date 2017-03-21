namespace NServiceBusTutorials.Common
{
    public abstract class Worker
    {
        private readonly object _stopLock = new object();

        private bool _stopped;

        private bool _stopping;

        public bool Stopping
        {
            get
            {
                lock (_stopLock)
                {
                    return _stopping;
                }
            }
        }

        public bool Stopped
        {
            get
            {
                lock (_stopLock)
                {
                    return _stopped;
                }
            }
        }

        public void Start()
        {
            try
            {
                Run();
            }
            finally
            {
                SetStopped();
            }
        }

        public void Stop()
        {
            lock (_stopLock)
            {
                _stopping = true;
            }
        }

        protected abstract void Setup();

        protected void Run()
        {
            Setup();

            while (!Stopping)
            {
                DoStep();
            }

            TearDown();
        }

        protected abstract void TearDown();

        protected abstract void DoStep();

        private void SetStopped()
        {
            lock (_stopLock)
            {
                _stopped = true;
            }
        }
    }
}
