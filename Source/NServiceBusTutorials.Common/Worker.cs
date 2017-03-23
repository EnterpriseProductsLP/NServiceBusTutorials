// -----------------------------------------------------------------------
// <copyright file="Worker.cs" company="Enterprise Products Partners L.P. (Enterprise)">
// © Copyright 2012 - 2017, Enterprise Products Partners L.P. (Enterprise), All Rights Reserved.
// Permission to use, copy, modify, or distribute this software source code, binaries or
// related documentation, is strictly prohibited, without written consent from Enterprise.
// For inquiries about the software, contact Enterprise: Enterprise Products Company Law
// Department, 1100 Louisiana, 10th Floor, Houston, Texas 77002, phone 713-381-6500.
// </copyright>
// -----------------------------------------------------------------------
namespace NServiceBusTutorials.Common
{
    public abstract class Worker
    {
        private readonly object _stateLock = new object();

        private WorkerState _workerState = WorkerState.Initializing;

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

        protected void Run()
        {
            SetWorkerState(OnRunning());

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

        protected abstract WorkerState OnRunning();

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
