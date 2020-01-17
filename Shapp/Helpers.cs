﻿using System;
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

namespace Shapp {
    public class Helper {
        private const string inputFile = "input.txt";
        private static readonly int FILENAME_LENGTH = 15;
        private static readonly string FILENAMES_MAPPING_FORMAT = "[SHAPP] Filename mapping: '{0}':'{1}'";
        private const string FILENAMES_MAPPING_FORMAT_REGEX = "^\\[SHAPP\\] Filename mapping: '(.+)':'(.+)'$";
        private const string EXIT_CODE_FILE = "exit_path";
        private const string COUNTER_EXAMPLE_FILE = "start_path";
        private static readonly Random random = new Random();
        private readonly List<JobDescriptor> RemoteDescriptors = new List<JobDescriptor>();

        public static void SaveChildOutputToFiles(int exitCode, string counterExampleContent) {
            var filenamesMap = new Dictionary<string, string>();
            string exitCodeFilename = GetEffectiveFilename(EXIT_CODE_FILE, filenamesMap);
            File.WriteAllText(exitCodeFilename, exitCode.ToString());

            if (counterExampleContent != null) {
                string counterExampleFilename = GetEffectiveFilename(COUNTER_EXAMPLE_FILE, filenamesMap);
                File.WriteAllText(counterExampleFilename, counterExampleContent);
            }
        }

        public int GetExitCode(JobId jid) {
            var childFilenamesMap = ReadFilenameMapping(jid); // loads the mapping from the child, can be done only after the child completes
            // from now, using the files is exaclty mirrored as in the child
            string exitCodeFilename = GetEffectiveFilename(EXIT_CODE_FILE, childFilenamesMap);
            int exitCode = int.Parse(File.ReadAllText(exitCodeFilename));
            return exitCode;
        }

        public string GetCounterExample(JobId jid) {
            var childFilenamesMap = ReadFilenameMapping(jid); // loads the mapping from the child, can be done only after the child completes
            string counterExampleFilename = GetEffectiveFilename(COUNTER_EXAMPLE_FILE, childFilenamesMap);
            string counterExample = File.ReadAllText(counterExampleFilename);
            return counterExample;
        }

        public JobDescriptor SubmitNewCopyOfMyself(string[] inputFiles = null, string[] arguments = null) {
            var selfSubmitter = new SelfSubmitter(inputFiles, arguments);
            var descriptor = selfSubmitter.Submit();
            RemoteDescriptors.Add(descriptor);
            return descriptor;
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

        public static string RandomString(int length) {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            return new string(Enumerable.Repeat(chars, length)
                .Select(s => s[random.Next(s.Length)]).ToArray());
        }
        public static JobDescriptor WaitForAnyJobToEnd(List<JobDescriptor> descriptors) {
            var descriptorEvents = descriptors.Select(descriptor => descriptor.JobCompletedEvent).ToArray();
            // wait for some child to complete
            var indexOfCompetedEvent = WaitHandle.WaitAny(descriptorEvents);
            // get completed task's descriptor
            var completedEvent = descriptorEvents[indexOfCompetedEvent];
            var completedTaskDescriptor = descriptors.Find(descriptor => descriptor.JobCompletedEvent == completedEvent);
            return completedTaskDescriptor;
        }
    }
}
