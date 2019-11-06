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
            //SubmitAndRemoveExample();
            //Program main = new Program();
            //main.Execute();
            Program main = new Program();
            main.ExecuteWithCommunication();
        }

        public int Execute(string[] args) {
            AsynchronousServer server = new AsynchronousServer();
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
                MyJobId = "123.456"
            });


            int k = 30;
            while (--k > 0) {
                Thread.Sleep(1000);
                Console.Out.WriteLine("Received msgs so far: {0}", AsynchronousCommunicationUtils.reception);
            }
            ParentCommunicator.Stop();
        }

        private static void DoTheChildJob(string modelFilename, string startPath) {
            // do some job, the main task
            Tuple<int, string> exitCodeAndCounterExample = PerformComputation(modelFilename, startPath);

            // after that, build the files to transfer:
            int exitCode = exitCodeAndCounterExample.Item1;
            string counterExample = exitCodeAndCounterExample.Item2;
            SaveChildOutputToFiles(exitCode, counterExample);
        }

        public void ExecuteWithCommunication() {
            if (SelfSubmitter.AmIRootProcess()) {
                AsynchronousServer server = new AsynchronousServer();
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
                SubmitNewCopyOfMyselfAndWaitForStart();
                while (true) {
                    Thread.Sleep(1000);
                    Console.Out.WriteLine("Received msgs so far: {0}", AsynchronousCommunicationUtils.reception);
                }
            } else if (SelfSubmitter.GetMyNestLevel() == 1) {
                ClientExample();
            }
        }

        public void Execute() {
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

        private void SubmitNewCopyOfMyselfAndWaitForStart() {
            string[] filesToAttach = { "file_with_input.txt" };
            SelfSubmitter selfSubmitter = new SelfSubmitter(filesToAttach);
            var remoteProcessDescriptor = selfSubmitter.Submit();
            RemoteDescriptors.Add(remoteProcessDescriptor);
            return remoteProcessDescriptor;
        }

        private static Dictionary<String, String> ReadFilenameMapping(JobId jid) {
            Dictionary<String, String> filenamesMap = new Dictionary<string, string>();

            using (StreamReader sr = new StreamReader(string.Format("x_{0}_stdout.out", jid))) {
                string line;
                while ((line = sr.ReadLine()) != null) {
                    Regex regex = new Regex(FILENAMES_MAPPING_FORMAT_REGEX);
                    Match match = regex.Match(line);
                    string filename = match.Groups[1].Value;
                    string effectiveFilename = match.Groups[2].Value;
                    filenamesMap.Add(filename, effectiveFilename);
                }
            }
            return filenamesMap;
        }
        private static string GetEffectiveFilename(string file, Dictionary<String, String> map) {
            if (!map.ContainsKey(file)) {
                map.Add(file, "x_shapp_" + file + "_" + RandomString(FILENAME_LENGTH) + ".txt");
                Console.Out.WriteLine(string.Format(FILENAMES_MAPPING_FORMAT, file, map[file]));
            }
            return map[file];
        }

        private static void SubmitNewJob() {
            NewJobSubmitter newJobSubmitter = new NewJobSubmitter {
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

        private static Log(string s) {
            Console.Out.WriteLine(s);
        }
    }
}
