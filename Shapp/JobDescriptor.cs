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
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public readonly JobId JobId;
        private JobState state;
        public JobState State
        {
            get => state;
            set
            {
                JobState previous = state;
                state = value;
                StateListener?.Invoke(previous, state);
            }
        }
        public ManualResetEvent JobStarted = new ManualResetEvent(false);
        public ManualResetEvent JobCompleted = new ManualResetEvent(false);
        public IPAddress WorkerIpAddress = null;

        private delegate void JobStateChanged(JobState previous, JobState current);
        private event JobStateChanged StateListener;

        public JobDescriptor(JobId jobId)
        {
            JobId = jobId;
            StateListener += JobDescriptorEventLauncher;
            StateListener += JobDescriptorStateChangeLogger;
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
                    break;
            }
        }

        private void JobDescriptorStateChangeLogger(JobState previous, JobState current)
        {
            log.InfoFormat("Job {0} state has changed from {1} to {2}", JobId, previous, current);
        }
    }
}
