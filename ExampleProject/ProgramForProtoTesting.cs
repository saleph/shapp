using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Shapp;
using Shapp.Communications.Protocol;

namespace ExampleProject {
    class ProgramForProtoTesting {
        public static void MainMethod(string[] args) {
            if (args[0].Equals("s")) {
                ServerExample();
            } else {
                ClientExample();
            }
            //ProgramForProtoTesting main = new ProgramForProtoTesting();
            //main.ExecuteWithCommunication();
        }

        public static void ServerExample() {
            AsynchronousServer server = new AsynchronousServer(Shapp.C.DEFAULT_PORT);
            AutoResetEvent receivedDone = new AutoResetEvent(false);
            server.NewMessageReceivedEvent += (objectRecv, sock) => {
                if (objectRecv is ISystemMessage hello)
                    hello.Dispatch(sock);

                if (objectRecv is string s) {
                    Console.Out.WriteLine(s);
                    receivedDone.Set();
                }
            };
            server.Start();
            //receivedDone.WaitOne();
            int k = 30;
            while (--k > 0) {
                Thread.Sleep(1000);
                Console.Out.WriteLine("Received msgs so far: {0}", AsynchronousCommunicationUtils.reception);
            }
        }

        private static void ClientExample() {
            ParentCommunicator.Initialize();
            ParentCommunicator.Send(new HelloFromChild() {
                MyJobId = new JobId("123.456")
            });


            int k = 30;
            while (--k > 0) {
                Thread.Sleep(1000);
                Console.Out.WriteLine("Received msgs so far: {0}", AsynchronousCommunicationUtils.reception);
            }
            ParentCommunicator.Stop();
        }

        public void Execute() {
            if (SelfSubmitter.AmIRootProcess()) {
                SubmitNewCopyOfMyself();
                SubmitNewCopyOfMyself();
            } else if (SelfSubmitter.GetMyNestLevel() == 1) {
                Console.Out.WriteLine("Hello from 1nd nest level");
                SubmitNewCopyOfMyself();
            } else if (SelfSubmitter.GetMyNestLevel() == 2) {
                Console.Out.WriteLine("Hello from 2nd nest level");
            }
        }

        private JobDescriptor SubmitNewCopyOfMyself() {
            string[] filesToAttach = { "file_with_input.txt" };
            SelfSubmitter selfSubmitter = new SelfSubmitter(filesToAttach);
            var remoteProcessDescriptor = selfSubmitter.Submit();
            return remoteProcessDescriptor;
        }

        private static void SubmitNewJob() {
            NewJobSubmitter newJobSubmitter = new NewJobSubmitter {
                Command = "batch.py",
                UserStandardOutputFileName = "stdout.txt",
                TargetOperatingSystem = TargetOperatingSystem.ANY
            };
            JobDescriptor jobDescriptor = newJobSubmitter.SubmitNewJob();
            Log("Job submitted");
            jobDescriptor.JobStartedEvent.WaitOne();
            Log("Job started");
            jobDescriptor.JobCompletedEvent.WaitOne();
            Log("Job completed");
        }

        private static void Log(string s) {
            Console.Out.WriteLine(s);
        }

        public void ExecuteWithCommunication() {
            if (SelfSubmitter.AmIRootProcess()) {
                AsynchronousServer server = new AsynchronousServer(C.DEFAULT_PORT);
                AutoResetEvent receivedDone = new AutoResetEvent(false);
                server.NewMessageReceivedEvent += (objectRecv, sock) => {
                    if (objectRecv is ISystemMessage hello)
                        hello.Dispatch(sock);

                    if (objectRecv is string s) {
                        Console.Out.WriteLine(s);
                        receivedDone.Set();
                    }
                };
                server.Start();
                //receivedDone.WaitOne();
                SubmitNewCopyOfMyself();
                while (true) {
                    Thread.Sleep(1000);
                    Console.Out.WriteLine("Received msgs so far: {0}", AsynchronousCommunicationUtils.reception);
                }
            } else if (SelfSubmitter.GetMyNestLevel() == 1) {
                ClientExample();
            }
        }
    }
}
