using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Shapp;
using Shapp.Communications.Protocol;

namespace ExampleProject
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args[0].Equals("s"))
            {
                ServerExample();
            } else
            {
                ClientExample();
            }
            //SubmitAndRemoveExample();
            //Program main = new Program();
            //main.Execute();
        }

        private static void ServerExample()
        {
            AsynchronousServer server = new AsynchronousServer();
            AutoResetEvent receivedDone = new AutoResetEvent(false);
            server.NewMessageReceivedEvent += (objectRecv, sock) =>
            {
                if (objectRecv is ISystemMessage hello)
                    hello.Dispatch(sock);

                if (objectRecv is string s)
                {
                    Console.Out.WriteLine(s);
                    receivedDone.Set();
                }
            };
            server.Start();
            //receivedDone.WaitOne();
            while (true)
            {
                Thread.Sleep(1000);
                Console.Out.WriteLine("Received msgs so far: {0}", AsynchronousCommunicationUtils.reception);
            }
        }

        private static void ClientExample()
        {
            ParentCommunicator.Initialize();
            ParentCommunicator.Send(new HelloFromChild()
            {
                MyJobId = "123.456"
            });
            //ParentCommunicator.Send("some string from child


            while(true)
            {
                Thread.Sleep(1000);
                Console.Out.WriteLine("Received msgs so far: {0}", AsynchronousCommunicationUtils.reception);
            }
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
            if (SelfSubmitter.AmIRootProcess()) {
                SubmitNewCopyOfMyselfAndWaitForStart();
                SubmitNewCopyOfMyselfAndWaitForStart();
                WaitForCopiesToComplete();
            } else if (SelfSubmitter.GetMyNestLevel() == 1) {
                Console.Out.WriteLine("Hello from 1nd nest level");
                SubmitNewCopyOfMyselfAndWaitForStart();
                WaitForCopiesToComplete();
            } else if (SelfSubmitter.GetMyNestLevel() == 2) {
                Console.Out.WriteLine("Hello from 2nd nest level");
            }
        }

        private void SubmitNewCopyOfMyselfAndWaitForStart()
        {
            string[] filesToAttach = { "file_with_input.txt" };
            SelfSubmitter selfSubmitter = new SelfSubmitter(filesToAttach);
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
