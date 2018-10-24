using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Shapp
{
    class JobDescriptor
    {
        public JobId JobId = null;
        public JobState State
        {
            get
            {
                return State;
            }
            set
            {
                JobState previous = State;
                State = value;
                StateListener?.Invoke(previous, State);
            }
        }
        public ManualResetEvent JobStarted = new ManualResetEvent(false);
        public ManualResetEvent JobCompleted = new ManualResetEvent(false);
        public IPAddress WorkerIpAddress = null;

        private delegate void JobStateChanged(JobState previous, JobState current);
        private event JobStateChanged StateListener;

        public JobDescriptor()
        {
            StateListener += JobDescriptorEventLauncher;
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
    }
}
