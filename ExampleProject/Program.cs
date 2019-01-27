﻿using System;
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
            SubmitAndRemoveExample();
            Program main = new Program();
            main.Execute();
        }

        private List<JobDescriptor> RemoteDescriptors = new List<JobDescriptor>();

        private static void SubmitAndRemoveExample()
        {
            SelfSubmitter newJobSubmitter = new SelfSubmitter();
            JobDescriptor jobDescriptor = newJobSubmitter.Submit();
            JobRemover jobRemover = new JobRemover(jobDescriptor.JobId);
            jobRemover.Remove();
        }

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
                Console.Out.WriteLine("Hello from 1nd nest level");
                // submit 1 level more
                SubmitNewCopyOfMyselfAndWaitForStart();
                WaitForCopiesToComplete();
                // do some job
            }
            if (SelfSubmitter.GetMyNestLevel() == 2)
            {
                // great, I was invoked on remote worker by another remote worker
                Console.Out.WriteLine("Hello from 2nd nest level");
                // do some job
            }
        }

        private void SubmitNewCopyOfMyselfAndWaitForStart()
        {
            SelfSubmitter selfSubmitter = new SelfSubmitter("file_with_input.txt");
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
                UserStandardOutputFileName = "stdout.txt",
                TargetOperatingSystem = TargetOperatingSystem.ANY
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
