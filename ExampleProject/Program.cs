using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Shapp;

namespace ExampleProject
{
    class Program
    {
        private const string inputFile = "input.txt";
        private List<JobDescriptor> RemoteDescriptors = new List<JobDescriptor>();

        static void Main(string[] args)
        {
            //if (args[0].Equals("s"))
            //{
            //    ServerExample();
            //} else
            //{
            //    ClientExample();
            //}
            //SubmitAndRemoveExample();
            Program main = new Program();
            main.Execute(args);
        }

        //private static void ServerExample()
        //{
        //    AsynchronousServer server = new AsynchronousServer();
        //    AutoResetEvent receivedDone = new AutoResetEvent(false);
        //    server.NewMessageReceivedEvent += (objectRecv, sock) =>
        //    {
        //        Console.Out.Write("recv: ");
        //        if (objectRecv is string s)
        //        {
        //            Console.Out.WriteLine(s);
        //        }
        //        string res = "elo response from serv";
        //        server.Send(sock, res);
        //        receivedDone.Set();
        //    };
        //    server.Start();
        //    receivedDone.WaitOne();
        //    server.Stop();
        //}

        //private static void ClientExample()
        //{
        //    AsynchronousClient client = new AsynchronousClient();
        //    AutoResetEvent receivedDone = new AutoResetEvent(false);
        //    client.NewMessageReceivedEvent += (objectRecv, sock) =>
        //    {
        //        Console.Out.Write("recv: ");
        //        if (objectRecv is string s)
        //        {
        //            Console.Out.WriteLine(s);
        //        }
        //        string res = "elo response from serv";
        //        client.Send(res);
        //        receivedDone.Set();
        //    };
        //    client.Connect(IPAddress.Parse("192.168.56.1"));
        //    string msg = "elo from client";
        //    client.Send(msg);
        //    receivedDone.WaitOne();
        //    client.Stop();
        //}

        //private static void SubmitAndRemoveExample()
        //{
        //    SelfSubmitter newJobSubmitter = new SelfSubmitter();
        //    JobDescriptor jobDescriptor = newJobSubmitter.Submit();
        //    JobRemover jobRemover = new JobRemover(jobDescriptor.JobId);
        //    jobRemover.Remove();
        //}

        public void Execute(string[] args)
        {
            if (SelfSubmitter.AmIRootProcess()) {
                File.WriteAllText(inputFile, "some input file content");
                args.ToList().ForEach(s => Console.WriteLine(s));
                SubmitNewCopyOfMyselfAndWaitForStart();
                SubmitNewCopyOfMyselfAndWaitForStart();
                WaitForCopiesToComplete();
            } else if (SelfSubmitter.GetMyNestLevel() == 1) {
                Console.Out.WriteLine("Hello from 1nd nest level");
                args.ToList().ForEach(s => Console.WriteLine(s));
                Console.WriteLine(File.ReadAllText(inputFile));
                SubmitNewCopyOfMyselfAndWaitForStart();
                WaitForCopiesToComplete();
            } else if (SelfSubmitter.GetMyNestLevel() == 2) {
                Console.Out.WriteLine("Hello from 2nd nest level");
                args.ToList().ForEach(s => Console.WriteLine(s));
                Console.WriteLine(File.ReadAllText(inputFile));
            }
        }

        private void SubmitNewCopyOfMyselfAndWaitForStart()
        {
            string[] inputFiles = { inputFile };
            string[] arguments = { "-s", "123", "--set-sth", "'new value'" };
            SelfSubmitter selfSubmitter = new SelfSubmitter(inputFiles, arguments);
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
