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
        private const string START_PATH_FILE = "start_path";
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
                DoTheChildJob();
            }
            return 0;
        }

        private int DoTheParentJob() {
            var descriptors = new List<JobDescriptor>();
            int i = 0;
            for (; i < WORKERS_POOL_SIZE; ++i) {
                // just an examle, same model also can be used
                string[] modelFilesForTask = { "model.xml", "model" + i + ".xml" };
                string[] arguments = { "-arg1", modelFilesForTask[0], "-arg2", modelFilesForTask[1] };
                var descriptor = SubmitNewCopyOfMyselfAndWaitForStart(modelFilesForTask, arguments);
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
                    var startPathContent = GetStartPath(jobId);
                    // processing of the start path
                    return 0;
                } else {
                    string[] modelFilesForTask = { "model" + ++i + ".xml" };
                    var descriptor = SubmitNewCopyOfMyselfAndWaitForStart(modelFilesForTask);
                    descriptors.Add(descriptor);
                }
            }
        }

        private static void DoTheChildJob() {
            Console.Out.WriteLine("Hello from 1nd nest level");
            // do some job, the main task

            // after that, build the files to transfer:
            int exitCode = 443;
            string startPathContent = "startPath content";
            SaveChildOutputToFiles(exitCode, startPathContent);
        }

        private static void SaveChildOutputToFiles(int exitCode, string startPathContent)
        {
            var filenamesMap = new Dictionary<string, string>();
            string exitPathFilename = GetEffectiveFilename(EXIT_CODE_FILE, filenamesMap);
            File.WriteAllText(exitPathFilename, exitCode.ToString());

            string startPathFilename = GetEffectiveFilename(START_PATH_FILE, filenamesMap);
            File.WriteAllText(startPathFilename, startPathContent);
        }

        private int GetExitCode(JobId jid) {
            var childFilenamesMap = ReadFilenameMapping(jid); // loads the mapping from the child, can be done only after the child completes
            // from now, using the files is exaclty mirrored as in the child
            string exitCodeFilename = GetEffectiveFilename(EXIT_CODE_FILE, childFilenamesMap);
            int exitCode = int.Parse(File.ReadAllText(exitCodeFilename));
            return exitCode;
        }

        private string GetStartPath(JobId jid)
        {
            var childFilenamesMap = ReadFilenameMapping(jid); // loads the mapping from the child, can be done only after the child completes
            string startPathFilename = GetEffectiveFilename(START_PATH_FILE, childFilenamesMap);
            string startPath = File.ReadAllText(startPathFilename);
            return startPath;
        }

        private JobDescriptor SubmitNewCopyOfMyselfAndWaitForStart(string[] inputFiles = null, string[] arguments = null)
        {
            SelfSubmitter selfSubmitter = new SelfSubmitter(inputFiles, arguments);
            var remoteProcessDescriptor = selfSubmitter.Submit();
            remoteProcessDescriptor.JobStartedEvent.WaitOne();
            RemoteDescriptors.Add(remoteProcessDescriptor);
            return remoteProcessDescriptor;
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
