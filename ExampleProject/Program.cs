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

namespace ExampleProject {
    class Program {
        public Helper helper = new Helper();

        private const int WORKERS_POOL_SIZE = 10;
        static void Main(string[] args) {
            ACOExample.Run();
            //ACO.ACOWithShappExample.Run(args);
            //ProgramForProtoTesting.MainMethod(args);
            //Program main = new Program();
            //main.Execute(args);
        }

        public int Execute(string[] args) {
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
                var descriptor = helper.SubmitNewCopyOfMyself(modelFilesForTask, arguments);
                descriptors.Add(descriptor);
            }

            while (true) {
                JobDescriptor completedTaskDescriptor = Helper.WaitForAnyJobToEnd(descriptors);
                completedTaskDescriptor.HardRemove();
                // cleanup the active descriptors removing the completed one
                descriptors.Remove(completedTaskDescriptor);
                // gather results
                var jobId = completedTaskDescriptor.JobId;
                var exitCode = helper.GetExitCode(jobId);
                if (exitCode == 0) {
                    // everything is done, tearing down everything
                    descriptors.ForEach(descriptor => descriptor.HardRemove());
                    var counterExampleContent = helper.GetCounterExample(jobId);
                    // processing of the counterExample
                    return 0;
                } else {
                    string[] modelFilesForTask = { "model.xml", "startpath" + ++i + ".xml" };
                    string[] arguments = { "--model", modelFilesForTask[0], "--startpath", modelFilesForTask[1] };
                    var descriptor = helper.SubmitNewCopyOfMyself(modelFilesForTask, arguments);
                    descriptors.Add(descriptor);
                }
            }
        }



        private void DoTheChildJob(string modelFilename, string startPath) {
            // do some job, the main task
            Tuple<int, string> exitCodeAndCounterExample = PerformComputation(modelFilename, startPath);

            // after that, build the files to transfer:
            int exitCode = exitCodeAndCounterExample.Item1;
            string counterExample = exitCodeAndCounterExample.Item2;
            Helper.SaveChildOutputToFiles(exitCode, counterExample);
        }

        private static Tuple<int, string> PerformComputation(string modelFilename, string startPath) {
            // do the task
            int exitCode = 1;
            string counterExample = null; // empty if not found
            return Tuple.Create(exitCode, counterExample);
        }

    }
}
