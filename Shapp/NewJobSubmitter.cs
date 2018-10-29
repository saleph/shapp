using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Shapp
{
    public class NewJobSubmitter
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        private const string LINUX_TARGET_OPERATING_SYSTEM = "target.OpSys == \"LINUX\"";
        private const string WINDOWS_TARGET_OPERATING_SYSTEM = "target.OpSys == \"WINDOWS\"";


        public string Command = "";
        public string WorkingDirectory = "";
        public string LogFileName = "";
        public string UserStandardOutputFileName = "";
        public string StandardErrorFileName = "";
        public string UserStandardInputFileName = "";
        public string InputFilesToTransferSpaceSeparated = "";
        public string CommandCliArguments = "";
        public string AdditionalJobEnvironmentalVariables = "";
        public TargetOperatingSystem TargetOperatingSystem = TargetOperatingSystem.SAME_AS_CURRENT;
        

        public JobDescriptor SubmitNewJob()
        {
            ValidateParameters();
            string pythonScirpt = ConstructPythonScript();
            PythonScriptsExecutor executor = new PythonScriptsExecutor(pythonScirpt);
            executor.Execute();
            string jobIdAsString = executor.Response;
            JobId jobId = new JobId(jobIdAsString);
            LogNewJobSubmission(jobId);
            return new JobDescriptor(jobId);
        }

        private void ValidateParameters()
        {
            if (Command.Length == 0)
                throw new ShappException("Newly submitting job cannot have empty Command");
            if (LogFileName.Length == 0)
                log.Warn("You skipped a definion of LogFileName parameter for newly submitted job. " +
                    "You won't be able to watch on it's state.");
        }

        private string ConstructPythonScript()
        {
            string pythonScript = Properties.Resources.SubmitNewJobScript;
            return string.Format(pythonScript,
                Command,
                WorkingDirectory.Replace(@"\", @"\\"),
                LogFileName,
                UserStandardOutputFileName,
                StandardErrorFileName,
                UserStandardInputFileName,
                InputFilesToTransferSpaceSeparated,
                CommandCliArguments,
                BuildEnvironmentalVariables(),
                BuildRequirements());
        }

        private string BuildRequirements()
        {
            string requirements = "";
            switch (TargetOperatingSystem)
            {
                case TargetOperatingSystem.SAME_AS_CURRENT:
                    requirements = "";
                    break;
                case TargetOperatingSystem.ONLY_LINUX:
                    requirements = LINUX_TARGET_OPERATING_SYSTEM;
                    break;
                case TargetOperatingSystem.ONLY_WINDOWS:
                    requirements = WINDOWS_TARGET_OPERATING_SYSTEM;
                    break;
                case TargetOperatingSystem.ANY:
                    requirements = string.Format("{0} || {1}", LINUX_TARGET_OPERATING_SYSTEM, WINDOWS_TARGET_OPERATING_SYSTEM);
                    break;
            }
            return requirements;
        }

        private string BuildEnvironmentalVariables()
        {
            return string.Format("{0}={1} {2}={3} {4}",
                JobEnvVariables.NEST_LEVEL_NAME, JobEnvVariables.GetNestLevel() + 1,
                JobEnvVariables.PARENT_SUBMITTER_IP_NAME, GetThisNodeIpAddress(),
                AdditionalJobEnvironmentalVariables);
        }

        private static IPAddress GetThisNodeIpAddress()
        {
            using (Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, 0))
            {
                socket.Connect("8.8.8.8", 65530);
                IPEndPoint endPoint = socket.LocalEndPoint as IPEndPoint;
                return endPoint.Address;
            }
        }

        private void LogNewJobSubmission(JobId jobId)
        {
            string logEntry = string.Format("A job with id {0} was submitted with arguments:\n", jobId);
            foreach (var field in this.GetType().GetFields())
            {
                logEntry += field.Name + " = " + field.GetValue(this) + "\n";
            }
            log.Info(logEntry);
        }
    }
}
