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
    /// <summary>
    /// Class responsible for preparing python script for submitting new jobs (as batch programs).
    /// 
    /// It's a part of SelfSubmitter, but it can be used stand alone - for submitting new tasks
    /// in the same way as in pure htcondor CLI interface.
    /// 
    /// Note: After construction of class instance and proper configuration of the fields the 
    /// same job can be submitted multiple times.
    /// 
    /// For fields like: LogFileName, UserStaandardOutputFileName, StandardErrorFileName, 
    /// UserStandardInputFileName, InputFilesToTransferSpaceSeparated some additional
    /// parameters can be used as a part of its name:
    /// $(ClusterId) - it would be evaluated into cluster number after job submission.
    ///     For job "123.0" it would be "123".
    /// $(ProcId) - it would be evaluated into cluster number after job submission.
    ///     For job "123.0" it would be "0".
    /// </summary>
    public class NewJobSubmitter
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        private const string LINUX_TARGET_OPERATING_SYSTEM = "target.OpSys == \"LINUX\"";
        private const string WINDOWS_TARGET_OPERATING_SYSTEM = "target.OpSys == \"WINDOWS\"";

        /// <summary>
        /// Command to execute. 
        /// 
        /// When only relative path is passed (e.g. "batch.py", "Debug\SomeApp.exe")
        /// HTCondor will take such file from WorkingDirectory (note: if working directory is empty,
        /// the directory with current executable is taken as working directory).The command 
        /// can be also specified as absolute path (e.g. "/bin/sh", "/usr/bin/sleep", "C:\SomeApp.exe").
        /// </summary>
        public string Command = "";
        /// <summary>
        /// Base directory for this job. 
        /// 
        /// It affects all the fields operating on filenames 
        /// (Command, LogFileName, UserStaandardOutputFileName, StandardErrorFileName, 
        /// UserStandardInputFileName, InputFilesToTransferSpaceSeparated). 
        /// Defalut is directory in which current executable is executed (not stored! - 
        /// it may points to some temporary directory).
        /// </summary>
        public string WorkingDirectory = "";
        /// <summary>
        /// Filename for HTCondor log file for this job.
        /// 
        /// It will contain info about all the actions performed on the job:
        /// who submitted it, when, who was considered as executor and when.
        /// If there were any environmental problem - it would be there.
        /// 
        /// This can be freely changed or removed (assiging empty string would casue the
        /// log not to appear at all).
        /// </summary>
        public string LogFileName = "x_$(ClusterId).$(ProcId)_logs.log";
        /// <summary>
        /// Filename for standard output file for this job.
        /// 
        /// It will contain any printout onto stdout (like Console.Out.Write()).
        /// Same as "$ someApp &gt UserStandardOutputFileName".
        /// 
        /// This can be freely changed or removed (assiging empty string would casue the
        /// output file not to appear at all).
        /// </summary>
        public string UserStandardOutputFileName = "x_$(ClusterId).$(ProcId)_stdout.out";
        /// <summary>
        /// Filename for standard error file for this job.
        /// 
        /// It will contain any printout onto stderr (like Console.Error.Write() or stacktraces).
        /// Same as "$ someApp 2&gt StandardErrorFileName".
        /// 
        /// This can be freely changed or removed (assiging empty string would casue the
        /// error file not to appear at all).
        /// </summary>
        public string StandardErrorFileName = "x_$(ClusterId).$(ProcId)_stderr.err";
        /// <summary>
        /// Filename for standard input file for this job.
        /// 
        /// The content of this file will be redirected onto application stdin.
        /// Same as "$ someApp &lt UserStandardInputFileName".
        /// </summary>
        public string UserStandardInputFileName = "";
        /// <summary>
        /// Additional files to transfer. Each file should be space separated.
        /// </summary>
        public string InputFilesToTransferSpaceSeparated = "";
        /// <summary>
        /// Command line arguments for the application. E.g. "-g", "-skip src.cpp -m -O1".
        /// </summary>
        public string CommandCliArguments = "";
        /// <summary>
        /// Target operating system for the job. For more info refer to enum TargetOperatingSystem.
        /// </summary>
        public TargetOperatingSystem TargetOperatingSystem = TargetOperatingSystem.SAME_AS_CURRENT;
        
        /// <summary>
        /// Submits new job based on specified fields of this instance.
        /// 
        /// Can be called multiple times to submit exact copy of the task.
        /// </summary>
        /// <returns>job descriptor bound to newly submitted job</returns>
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
                BuildEnvironmentalVariables().Replace("\"", "\\\""),
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
            var envVarsList = new EnvVarsList()
            {
                IPAddress = GetThisNodeIpAddress(),
                NestLevel = JobEnvVariables.GetNestLevel() + 1
            };
            const string ENV_VAR_FORMAT = "{0}={1}";
            return string.Format(ENV_VAR_FORMAT,
                JobEnvVariables.SHAPP_ALL_ENV_VARS, envVarsList);
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
