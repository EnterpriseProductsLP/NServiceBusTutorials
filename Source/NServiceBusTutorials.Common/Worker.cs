namespace NServiceBusTutorials.Common
{
    public abstract class Worker
    {
        public enum WorkerState
        {
            // Initial state
            Initializing,

            // Worker is idle but waiting for the distributed lock.

            // Worker is TRANSITIONING to the idle state.

            // Worker is paused.
            Paused,

            // Working is TRANSITIONING to the paused state.
            Pausing,

            // Working is TRANSITIONING to the running state.
            Resuming,

            // Working is running.
            Running,

            // Working is TRANSITIONING to the running state.
            Starting,

            // Working is stopped and cannot be restarted.
            Stopped,

            // Working is TRANSITIONING to the stopped state.
            Stopping
        }

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
            if (!Stopped)
            {
                SetStopping();
            }
        }

        protected void Run()
        {
            SetWorkerState(OnStarting());

            while (!Stopping)
            {
                if (Running)
                {
                    DoStep();
                }
                else if (Pausing)
                {
                    SetWorkerState(OnPausing());
                }
                else if (Resuming)
                {
                    SetWorkerState(OnResuming());
                }
            }

            OnStopping();
        }

        protected abstract WorkerState OnStarting();

        protected abstract WorkerState OnPausing();

        protected abstract WorkerState OnResuming();

        protected abstract void OnStopping();

        protected abstract void DoStep();

        private void SetPaused()
        {
            SetWorkerState(WorkerState.Paused);
        }

        private void SetRunning()
        {
            SetWorkerState(WorkerState.Running);
        }

        private void SetPausing()
        {
            SetWorkerState(WorkerState.Pausing);
        }

        private void SetResuming()
        {
            SetWorkerState(WorkerState.Resuming);
        }

        private void SetStopped()
        {
            SetWorkerState(WorkerState.Stopped);
        }

        private void SetStopping()
        {
            SetWorkerState(WorkerState.Stopping);
        }

        private void SetWorkerState(WorkerState workerState)
        {
            lock (_stateLock)
            {
                _workerState = workerState;
            }
        }
    }
}