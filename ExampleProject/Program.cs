using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Shapp;

namespace ExampleProject
{
    class Program
    {
        static void Main(string[] args)
        {
            //SubmitNewJob();
            Program main = new Program();
            main.Execute();
        }

        private List<JobDescriptor> RemoteDescriptors = new List<JobDescriptor>();

        public void Execute()
        {
            if (SelfSubmitter.AmIRootProcess())
            {
                SubmitNewCopyOfMyselfAndWaitForStart();
                SubmitNewCopyOfMyselfAndWaitForStart();
                WaitForCopiesToComplete();
            }
            if (SelfSubmitter.GetMyNestLevel() == 1)
            {
                // great, I was invoked on remote worker by another remote worker
                Console.Out.WriteLine("Hello from 2nd nest level");
                // do some job
            }

            if (SelfSubmitter.AmIRootProcess())
            {
                // Open a IPSocket as a server
            }

            if (SelfSubmitter.AmIChildProcess())
            {
                IPAddress iPAddress = SelfSubmitter.GetMyParentIpAddress();
                // connect to a socket at ipAddress
            }
        }

        private void SubmitNewCopyOfMyselfAndWaitForStart()
        {
            SelfSubmitter selfSubmitter = new SelfSubmitter();
            var remoteProcessDescriptor = selfSubmitter.Submit();
            remoteProcessDescriptor.JobStartedEvent.WaitOne();
            RemoteDescriptors.Add(remoteProcessDescriptor);
        }

        private void WaitForCopiesToComplete()
        {
            foreach (var descriptor in RemoteDescriptors)
            {
                descriptor.JobCompletedEvent.WaitOne();
            }
        }

        private static void SubmitNewJob()
        {
            NewJobSubmitter newJobSubmitter = new NewJobSubmitter
            {
                Command = "batch.py",
                LogFileName = "logjob.log",
                UserStandardOutputFileName = "output.out"
            };
            JobDescriptor jobDescriptor = newJobSubmitter.SubmitNewJob();
            Console.Out.WriteLine("Job submitted");
            jobDescriptor.JobStartedEvent.WaitOne();
            Console.Out.WriteLine("Job started");
            jobDescriptor.JobCompletedEvent.WaitOne();
            Console.Out.WriteLine("Job completed");
        }
    }
}
