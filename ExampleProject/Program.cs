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
        private const string EXIT_CODE_FILE = "exit_path";
        private const string COUNTER_EXAMPLE_FILE = "start_path";
        private static readonly Random random = new Random();
        private const int WORKERS_POOL_SIZE = 10;
        static void Main(string[] args)
        {
            Program main = new Program();
            main.Execute(args);
        }

        public int Execute(string[] args)
        {
            if (SelfSubmitter.AmIRootProcess()) {
                return DoTheParentJob();
            } else if (SelfSubmitter.GetMyNestLevel() == 1) {
                // WARNING! I assume here that args are like follows:
                // string[] arguments = { "--model", modelFilesForTask[0], "--startpath", modelFilesForTask[1] };
                // if this changes, modify those values
                string modelFilename = args[1];
                string startPathFilename = args[3];
                DoTheChildJob(modelFilename, startPathFilename);
            }
            return 0;
        }

        private int DoTheParentJob() {
            var descriptors = new List<JobDescriptor>();
            int i = 0;
            for (; i < WORKERS_POOL_SIZE; ++i) {
                string[] modelFilesForTask = { "model.xml", "startpath" + i + ".xml" };
                string[] arguments = { "--model", modelFilesForTask[0], "--startpath", modelFilesForTask[1] };
                var descriptor = SubmitNewCopyOfMyself(modelFilesForTask, arguments);
                descriptors.Add(descriptor);
            }

            while (true) {
                var descriptorEvents = descriptors.Select(descriptor => descriptor.JobCompletedEvent).ToArray();
                // wait for some child to complete
                var indexOfCompetedEvent = WaitHandle.WaitAny(descriptorEvents);
                // get completed task's descriptor
                var completedEvent = descriptorEvents[indexOfCompetedEvent];
                var completedTaskDescriptor = descriptors.Find(descriptor => descriptor.JobCompletedEvent == completedEvent);
                // cleanup the active descriptors removing the completed one
                descriptors.Remove(completedTaskDescriptor);
                // gather results
                var jobId = completedTaskDescriptor.JobId;
                var exitCode = GetExitCode(jobId);
                if (exitCode == 0) {
                    // everything is done, tearing down everything
                    descriptors.ForEach(descriptor => descriptor.HardRemove());
                    var counterExampleContent = GetCounterExample(jobId);
                    // processing of the counterExample
                    return 0;
                } else {
                    string[] modelFilesForTask = { "model.xml", "startpath" + ++i + ".xml" };
                    string[] arguments = { "--model", modelFilesForTask[0], "--startpath", modelFilesForTask[1] };
                    var descriptor = SubmitNewCopyOfMyself(modelFilesForTask, arguments);
                    descriptors.Add(descriptor);
                }
            }
        }

        private static void DoTheChildJob(string modelFilename, string startPath) {
            // do some job, the main task
            Tuple<int, string> exitCodeAndCounterExample = PerformComputation(modelFilename, startPath);

            // after that, build the files to transfer:
            int exitCode = exitCodeAndCounterExample.Item1;
            string counterExample = exitCodeAndCounterExample.Item2;
            SaveChildOutputToFiles(exitCode, counterExample);
        }

        private static Tuple<int, string> PerformComputation(string modelFilename, string startPath) {
            // do the task


            int exitCode = 1;
            string counterExample = null; // empty if not found
            return Tuple.Create(exitCode, counterExample);
        }

        private static void SaveChildOutputToFiles(int exitCode, string counterExampleContent) {
            var filenamesMap = new Dictionary<string, string>();
            string exitCodeFilename = GetEffectiveFilename(EXIT_CODE_FILE, filenamesMap);
            File.WriteAllText(exitCodeFilename, exitCode.ToString());

            if (counterExampleContent != null) {
                string counterExampleFilename = GetEffectiveFilename(COUNTER_EXAMPLE_FILE, filenamesMap);
                File.WriteAllText(counterExampleFilename, counterExampleContent);
            }
        }

        private int GetExitCode(JobId jid) {
            var childFilenamesMap = ReadFilenameMapping(jid); // loads the mapping from the child, can be done only after the child completes
            // from now, using the files is exaclty mirrored as in the child
            string exitCodeFilename = GetEffectiveFilename(EXIT_CODE_FILE, childFilenamesMap);
            int exitCode = int.Parse(File.ReadAllText(exitCodeFilename));
            return exitCode;
        }

        private string GetCounterExample(JobId jid) {
            var childFilenamesMap = ReadFilenameMapping(jid); // loads the mapping from the child, can be done only after the child completes
            string counterExampleFilename = GetEffectiveFilename(COUNTER_EXAMPLE_FILE, childFilenamesMap);
            string counterExample = File.ReadAllText(counterExampleFilename);
            return counterExample;
        }

        private JobDescriptor SubmitNewCopyOfMyself(string[] inputFiles = null, string[] arguments = null) {
            SelfSubmitter selfSubmitter = new SelfSubmitter(inputFiles, arguments);
            var remoteProcessDescriptor = selfSubmitter.Submit();
            RemoteDescriptors.Add(remoteProcessDescriptor);
            return remoteProcessDescriptor;
        }

        private static Dictionary<String, String> ReadFilenameMapping(JobId jid) {
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
        private static string GetEffectiveFilename(string v, Dictionary<String, String> map) {
            if (!map.ContainsKey(v)) {
                map.Add(v, "x_shapp_" + RandomString(FILENAME_LENGTH) + ".txt");
                Console.Out.WriteLine(string.Format(FILENAMES_MAPPING_FORMAT, v, map[v]));
            }
            return map[v];
        }

        public static string RandomString(int length) {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            return new string(Enumerable.Repeat(chars, length)
                .Select(s => s[random.Next(s.Length)]).ToArray());
        }

    }
}
