namespace NServiceBusTutorials.Common
{
    public enum WorkerState
    {
        Initializing,
        Paused,
        Pausing,
        Resuming,
        Running,
        Starting,
        Stopped,
        Stopping
    }

    public abstract class Worker
    {
        private readonly object _stateLock = new object();

        private WorkerState _workerState = WorkerState.Initializing;

        public bool Initializing
        {
            get
            {
                lock (_stateLock)
                {
                    return _workerState == WorkerState.Initializing;
                }
            }
        }

        public bool Paused
        {
            get
            {
                lock (_stateLock)
                {
                    return _workerState == WorkerState.Paused;
                }
            }
        }

        public bool Pausing
        {
            get
            {
                lock (_stateLock)
                {
                    return _workerState == WorkerState.Pausing;
                }
            }
        }

        public bool Resuming
        {
            get
            {
                lock (_stateLock)
                {
                    return _workerState == WorkerState.Resuming;
                }
            }
        }

        public bool Running
        {
            get
            {
                lock (_stateLock)
                {
                    return _workerState == WorkerState.Running;
                }
            }
        }

        public bool Starting
        {
            get
            {
                lock (_stateLock)
                {
                    return _workerState == WorkerState.Starting;
                }
            }
        }

        public bool Stopping
        {
            get
            {
                lock (_stateLock)
                {
                    return _workerState == WorkerState.Stopping;
                }
            }
        }

        public bool Stopped
        {
            get
            {
                lock (_stateLock)
                {
                    return _workerState == WorkerState.Stopped;
                }
            }
        }

        public void Pause()
        {
            SetPausing();
        }

        public void Resume()
        {
            SetResuming();
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
            SetStopping();
        }

        protected abstract void OnRunning();

        protected void Run()
        {
            OnRunning();

            while (!Stopping)
            {
                if (Running)
                {
                    DoStep();
                }
                else if (Pausing)
                {
                    OnPausing();
                }
                else if (Resuming)
                {
                    OnResuming();
                }
            }

            OnStopping();
        }

        protected abstract void OnPausing();

        protected abstract void OnResuming();

        protected abstract void OnStopping();

        protected abstract void DoStep();

        protected void SetPaused()
        {
            lock (_stateLock)
            {
                _workerState = WorkerState.Paused;
            }
        }

        private void SetPausing()
        {
            lock (_stateLock)
            {
                _workerState = WorkerState.Pausing;
            }
        }

        private void SetResuming()
        {
            lock (_stateLock)
            {
                _workerState = WorkerState.Resuming;
            }
        }

        protected void SetRunning()
        {
            lock (_stateLock)
            {
                _workerState = WorkerState.Running;
            }
        }

        private void SetStopped()
        {
            lock (_stateLock)
            {
                _workerState = WorkerState.Stopped;
            }
        }

        private void SetStopping()
        {
            lock (_stateLock)
            {
                _workerState = WorkerState.Stopping;
            }
        }
    }
}
