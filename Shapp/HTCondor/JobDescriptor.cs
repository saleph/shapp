using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Shapp {
    /// <summary>
    /// Descriptor for a job with specified ID. Provides asynchronic interface for
    /// job state analysis.
    /// </summary>
    public class JobDescriptor {

        #region PublicProperties
        /// <summary>
        /// JobId to which this descriptor is being bound.
        /// </summary>
        public readonly JobId JobId;
        /// <summary>
        /// State of submitted job. Thread-safe.
        /// </summary>
        public JobState State {
            get {
                lock (stateLock) {
                    return state;
                }
            }
            private set {
                JobState previous;
                JobState current;
                lock (stateLock) {
                    previous = state;
                    current = value;
                    state = current;
                }
                StateListener?.Invoke(previous, current, JobId);
            }
        }
        /// <summary>
        /// Event launched on job start (after being considered by matchmaker).
        /// NOTE! State changes are discrete (are being polled periodically).
        /// Stays in state true forever after being set.
        /// </summary>
        public ManualResetEvent JobStartedEvent = new ManualResetEvent(false);
        /// <summary>
        /// Event launched on job start (after being considered by matchmaker).
        /// NOTE! State changes are discrete (are being polled periodically).
        /// Stays in state true as long as the job is capable of communication.
        /// </summary>
        public ManualResetEvent JobReadyForCommunicationsEvent = new ManualResetEvent(false);
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
        /// <summary>
        /// Delegate definition used in StateListener.
        /// </summary>
        /// <param name="previous">Previous state of the job.</param>
        /// <param name="current">New state of the job.</param>
        /// <param name="jobId">JobId that was affected by this change.</param>
        public delegate void JobStateChanged(JobState previous, JobState current, JobId jobId);
        #endregion

        private event JobStateChanged StateListener;
        private readonly object stateLock = new object();
        private JobState state = JobState.IDLE;
        private readonly JobStateFetcher jobStateFetcher;
        private readonly JobRemover jobRemover;
        private Socket jobSocket = null;
        private readonly System.Timers.Timer timer = new System.Timers.Timer(C.DEFAULT_JOB_STATE_REFRESH_INTERVAL_MS);

        /// <summary>
        /// Initializes job's descriptor with its jobId. It is being used mostly by internals of
        /// the library, but JobDescriptor may be created for an arbitrary job id (e.g. the one
        /// saved in some file before the program shutdown).
        /// </summary>
        /// <param name="jobId">job's id to bound into descriptor</param>
        public JobDescriptor(JobId jobId) {
            JobId = jobId;
            StateListener += JobDescriptorEventLauncher;
            StateListener += JobDescriptorStateChangeLogger;
            jobStateFetcher = new JobStateFetcher(jobId);
            jobRemover = new JobRemover(jobId);
            SetupJobStatusPoller();
            Communications.Protocol.HelloFromChild.OnReceive += (socket, message) => {
                if (message.MyJobId.Equals(JobId)) {
                    jobSocket = socket;
                    JobReadyForCommunicationsEvent.Set();
                }
            };
        }

        /// <summary>
        /// Adds custom object state observer
        /// </summary>
        /// <param name="jobStateChangedListener"></param>
        public void AddCustomStateListener(JobStateChanged jobStateChangedListener) {
            StateListener += jobStateChangedListener;
        }

        /// <summary>
        /// Removes in the job in the hard way (using HTCondor mechanisms). It terminates
        /// the task rightaway, without any time to perform cleanup.
        /// </summary>
        public void HardRemove() {
            jobRemover.Remove();
        }

        public void SoftRemove() {

        }

        /// <summary>
        /// Overwrites default polling interval.
        /// </summary>
        /// <param name="intervalInMs">Polling interval in ms from range [100; +inf)</param>
        public void SetPollingInterval(int intervalInMs) {
            if (intervalInMs < C.LOWEST_POSSIBLE_REFRESH_RATE_MS) {
                throw new ArgumentException(
                    "Polling interval cannot be lower than " + C.LOWEST_POSSIBLE_REFRESH_RATE_MS + "ms");
            }

            timer.Interval = intervalInMs;
        }

        public void Send(object objectToSend) {
            AsynchronousCommunicationUtils.Send(jobSocket, objectToSend);
        }

        private void SetupJobStatusPoller() {
            timer.Elapsed += RefreshJobState;
            SetPollingInterval(C.DEFAULT_JOB_STATE_REFRESH_INTERVAL_MS);
            timer.Enabled = true;
        }

        private void JobDescriptorEventLauncher(JobState previous, JobState current, JobId jobId) {
            switch (current) {
                case JobState.RUNNING:
                    JobStartedEvent.Set();
                    break;
                case JobState.COMPLETED:
                    JobStartedEvent.Set();
                    JobCompletedEvent.Set();
                    JobReadyForCommunicationsEvent.Reset();
                    DisableProbingJobState();
                    break;
                case JobState.REMOVED:
                    JobStartedEvent.Set();
                    JobRemovedEvent.Set();
                    JobReadyForCommunicationsEvent.Reset();
                    DisableProbingJobState();
                    break;
            }
        }

        private void DisableProbingJobState() {
            timer.Enabled = false;
        }

        private void JobDescriptorStateChangeLogger(JobState previous, JobState current, JobId jobId) {
            C.log.Info(string.Format("Job {0} state has changed from {1} to {2}", jobId, previous, current));
        }

        private void RefreshJobState(object sender, System.Timers.ElapsedEventArgs e) {
            JobState readState = jobStateFetcher.GetJobState();
            if (readState == State)
                return;
            State = readState;
        }
    }
}
