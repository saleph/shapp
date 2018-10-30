using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Shapp
{
    public class JobDescriptor
    {
        private const int JOB_STATE_REFRESH_INTERVAL_MS = 1000;
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        #region PublicProperties
        public readonly JobId JobId;
        public JobState State
        {
            get
            {
                lock (stateLock)
                {
                    return state;
                }
            }
            private set
            {
                JobState previous;
                JobState current;
                lock (stateLock)
                {
                    previous = state;
                    current = value;
                    state = current;
                }
                StateListener?.Invoke(previous, current);
            }
        }
        public ManualResetEvent JobStarted = new ManualResetEvent(false);
        public ManualResetEvent JobCompleted = new ManualResetEvent(false);
        public ManualResetEvent JobRemoved = new ManualResetEvent(false);
        public IPAddress WorkerIpAddress = null;
        #endregion

        private readonly object stateLock = new object();
        private JobState state = JobState.IDLE;
        private delegate void JobStateChanged(JobState previous, JobState current);
        private event JobStateChanged StateListener;
        private readonly JobStateFetcher JobStateFetcher;
        private System.Timers.Timer Timer = new System.Timers.Timer(JOB_STATE_REFRESH_INTERVAL_MS);

        public JobDescriptor(JobId jobId)
        {
            JobId = jobId;
            StateListener += JobDescriptorEventLauncher;
            StateListener += JobDescriptorStateChangeLogger;
            JobStateFetcher = new JobStateFetcher(jobId);
            SetupJobStatusPoller();
        }

        private void SetupJobStatusPoller()
        {
            Timer.Elapsed += RefreshJobState;
            Timer.Interval = JOB_STATE_REFRESH_INTERVAL_MS;
            Timer.Enabled = true;
        }

        private void JobDescriptorEventLauncher(JobState previous, JobState current)
        {
            switch (current)
            {
                case JobState.RUNNING:
                    JobStarted.Set();
                    break;
                case JobState.COMPLETED:
                    JobCompleted.Set();
                    DisableProbingJobState();
                    break;
                case JobState.REMOVED:
                    JobRemoved.Set();
                    DisableProbingJobState();
                    break;
            }
        }

        private void DisableProbingJobState()
        {
            Timer.Enabled = false;
        }

        private void JobDescriptorStateChangeLogger(JobState previous, JobState current)
        {
            log.InfoFormat("Job {0} state has changed from {1} to {2}", JobId, previous, current);
        }

        private void RefreshJobState(object sender, System.Timers.ElapsedEventArgs e)
        {
            JobState readState = JobStateFetcher.GetJobState();
            if (readState == State)
                return;
            State = readState;
        }
    }
}
