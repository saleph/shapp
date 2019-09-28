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

namespace ExampleProject
{
    class Program
    {
        private const string inputFile = "input.txt";
        private readonly List<JobDescriptor> RemoteDescriptors = new List<JobDescriptor>();
        private static readonly int FILENAME_LENGTH = 15;
        private static readonly string FILENAMES_MAPPING_FORMAT = "[SHAPP] Filename mapping: '{0}':'{1}'";
        private const string FILENAMES_MAPPING_FORMAT_REGEX = "^\\[SHAPP\\] Filename mapping: '(.+)':'(.+)'$";
        private const string EXIT_PATH_FILE = "exit_path";
        private const string START_PATH_FILE = "start_path";
        private static Random random = new Random();
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
                var descriptors = new List<JobDescriptor>();
                var firstDesc = SubmitNewCopyOfMyselfAndWaitForStart();
                descriptors.Add(firstDesc);
                var secondDesc = SubmitNewCopyOfMyselfAndWaitForStart();
                descriptors.Add(secondDesc);
                
                // wait for children to complete
                descriptors.ForEach(descriptor => descriptor.JobCompletedEvent.WaitOne());
                ExtractInfoFromOutputFiles(firstDesc.JobId);
                ExtractInfoFromOutputFiles(secondDesc.JobId);
            } else if (SelfSubmitter.GetMyNestLevel() == 1) {
                Console.Out.WriteLine("Hello from 1nd nest level");
                // do some job, the main task

                // after that, build the files to transfer:
                int exitCode = 443;
                string startPathContent = "startPath content";
                SaveChildOutputToFiles(exitCode, startPathContent);
            }
        }

        private static void SaveChildOutputToFiles(int exitCode, string startPathContent)
        {
            var filenamesMap = new Dictionary<string, string>();
            string exitPathFilename = GetEffectiveFilename(EXIT_PATH_FILE, filenamesMap);
            File.WriteAllText(exitPathFilename, exitCode.ToString());

            string startPathFilename = GetEffectiveFilename(START_PATH_FILE, filenamesMap);
            File.WriteAllText(startPathFilename, startPathContent);

            // can be used again, the name is preserved
            string exitPathFilename2 = GetEffectiveFilename(EXIT_PATH_FILE, filenamesMap);
            Debug.Assert(exitPathFilename == exitPathFilename2);
        }

        private void ExtractInfoFromOutputFiles(JobId jid)
        {
            var childFilenamesMap = ReadFilenameMapping(jid); // loads the mapping from the child, can be done only after the child completes
            
            // from now, using the files is exaclty mirrored as in the child
            string exitPathFilename = GetEffectiveFilename(EXIT_PATH_FILE, childFilenamesMap);
            int exitCode = int.Parse(File.ReadAllText(exitPathFilename));
            Console.Out.WriteLine(string.Format("{0}: exitCode={1}", jid, exitCode));

            string startPathFilename = GetEffectiveFilename(START_PATH_FILE, childFilenamesMap);
            string startPath = File.ReadAllText(startPathFilename);
            Console.Out.WriteLine(string.Format("{0}: startPath={1}", jid, startPath));
        }

        private void BasicTree(string[] args) {
            if (SelfSubmitter.AmIRootProcess()) {
                var firstDesc = SubmitNewCopyOfMyselfAndWaitForStart();
                var secondDesc = SubmitNewCopyOfMyselfAndWaitForStart();
                WaitForAllCopiesToComplete();
            } else if (SelfSubmitter.GetMyNestLevel() == 1) {
                Console.Out.WriteLine("Hello from 1nd nest level");
                args.ToList().ForEach(s => Console.WriteLine(s));
                Console.WriteLine(File.ReadAllText(inputFile));
                SubmitNewCopyOfMyselfAndWaitForStart();
                WaitForAllCopiesToComplete();
            } else if (SelfSubmitter.GetMyNestLevel() == 2) {
                Console.Out.WriteLine("Hello from 2nd nest level");
                args.ToList().ForEach(s => Console.WriteLine(s));
                Console.WriteLine(File.ReadAllText(inputFile));
            }
        }

        private JobDescriptor SubmitNewCopyOfMyselfAndWaitForStart()
        {
            string[] inputFiles = { inputFile };
            string[] arguments = { "-s", "123", "--set-sth", "'new value'" };
            SelfSubmitter selfSubmitter = new SelfSubmitter(inputFiles, arguments);
            var remoteProcessDescriptor = selfSubmitter.Submit();
            remoteProcessDescriptor.JobStartedEvent.WaitOne();
            RemoteDescriptors.Add(remoteProcessDescriptor);
            return remoteProcessDescriptor;
        }

        private void WaitForAllCopiesToComplete()
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

        

        private static Dictionary<String, String> ReadFilenameMapping(JobId jid)
        {
            Dictionary<String, String> filenamesMap = new Dictionary<string, string>();

            using (StreamReader sr = new StreamReader(string.Format("x_{0}_stdout.out", jid))) {
                string line;
                while((line = sr.ReadLine()) != null) {  
                    Regex regex = new Regex(FILENAMES_MAPPING_FORMAT_REGEX);
                    Match match = regex.Match(line);
                    string filename = match.Groups[1].Value;
                    string effectiveFilename = match.Groups[2].Value;
                    filenamesMap.Add(filename, effectiveFilename);
                }
            }
            return filenamesMap;
        }
        private static string GetEffectiveFilename(string v, Dictionary<String, String> map)
        {
            if (!map.ContainsKey(v)) {
                map.Add(v, "x_shapp_" + RandomString(FILENAME_LENGTH) + ".txt");
                Console.Out.WriteLine(string.Format(FILENAMES_MAPPING_FORMAT, v, map[v]));
            }
            return map[v];
        }

        public static string RandomString(int length)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            return new string(Enumerable.Repeat(chars, length)
                .Select(s => s[random.Next(s.Length)]).ToArray());
        }

    }
}
