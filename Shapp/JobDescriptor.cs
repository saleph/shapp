using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Shapp
{
    /// <summary>
    /// Descriptor for a job with specified ID. Provides asynchronic interface for
    /// job state analysis.
    /// </summary>
    public class JobDescriptor
    {
        private const int JOB_STATE_REFRESH_INTERVAL_MS = 1000;
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        #region PublicProperties
        /// <summary>
        /// JobId to which this descriptor is being bound.
        /// </summary>
        public readonly JobId JobId;
        /// <summary>
        /// State of submitted job. Thread-safe.
        /// </summary>
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
        /// <summary>
        /// Event launched on job start (after being considered by matchmaker).
        /// NOTE! State changes are discrete (are being polled periodically).
        /// Stays in state true forever after being set.
        /// </summary>
        public ManualResetEvent JobStartedEvent = new ManualResetEvent(false);
        /// <summary>
        /// Event launched when the job is properly ended.
        /// NOTE! State changes are discrete (are being polled periodically).
        /// Stays in state true forever after being set.
        /// </summary>
        public ManualResetEvent JobCompletedEvent = new ManualResetEvent(false);
        /// <summary>
        /// Event launched when the job was removed (e.g. $ condor_rm -all).
        /// NOTE! State changes are discrete (are being polled periodically).
        /// Stays in state true forever after being set.
        /// </summary>
        public ManualResetEvent JobRemovedEvent = new ManualResetEvent(false);
        #endregion

        private readonly object stateLock = new object();
        private JobState state = JobState.IDLE;
        private delegate void JobStateChanged(JobState previous, JobState current);
        private event JobStateChanged StateListener;
        private readonly JobStateFetcher JobStateFetcher;
        private System.Timers.Timer Timer = new System.Timers.Timer(JOB_STATE_REFRESH_INTERVAL_MS);

        /// <summary>
        /// Initializes job's descriptor with its jobId. It is being used mostly by internals of
        /// the library, but JobDescriptor may be created for an arbitrary job id (e.g. the one
        /// saved in some file before the program shutdown).
        /// </summary>
        /// <param name="jobId">job's id to bound into descriptor</param>
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
                    JobStartedEvent.Set();
                    break;
                case JobState.COMPLETED:
                    JobStartedEvent.Set();
                    JobCompletedEvent.Set();
                    DisableProbingJobState();
                    break;
                case JobState.REMOVED:
                    JobStartedEvent.Set();
                    JobRemovedEvent.Set();
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
