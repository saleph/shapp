﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Shapp
{
    /// <summary>
    /// Helper class for recursive self submitting.
    /// 
    /// This class can be further expanded by some additional parameters defined in class NewJobSubmitter.
    /// Feel free to extend!
    /// </summary>
    public class SelfSubmitter
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        private NewJobSubmitter NewJobSubmitter;

        /// <summary>
        /// Default constructor of self submitter. It supports also the debug builds (all the content of your current executable
        /// directory will be transfered automatically such as dlls, pdbs, xmls etc.).
        /// </summary>
        /// <param name="additionalInputFiles">additional files with input data to transfer</param>
        public SelfSubmitter(string additionalInputFiles = "")
        {
            NewJobSubmitter = new NewJobSubmitter()
            {
                Command = GetExecutableName(),
                InputFilesToTransferSpaceSeparated = BuildAdditionalLibrariesToTransfer() + " " + additionalInputFiles,
            };
        }

        /// <summary>
        /// Submits new instance of currently executing applicaiton.
        /// </summary>
        /// <returns>job descriptor bound to newly created job</returns>
        public JobDescriptor Submit()
        {
            return NewJobSubmitter.SubmitNewJob();
        }

        /// <summary>
        /// Acquires IP address of parent submitter.
        /// </summary>
        /// <returns>IPAddress of the parent</returns>
        public static IPAddress GetMyParentIpAddress()
        {
            return JobEnvVariables.GetParentSubmitterIp();
        }

        /// <summary>
        /// Checks if currently executing application is a root process.
        /// </summary>
        /// <returns>true if currently executing app is root process; false otherwise</returns>
        public static bool AmIRootProcess()
        {
            return JobEnvVariables.GetNestLevel() == 0;
        }

        /// <summary>
        /// Checks if currently executing application is a child process.
        /// </summary>
        /// <returns>true if currently executing app is child process; false otherwise</returns>
        public static bool AmIChildProcess()
        {
            return !AmIRootProcess();
        }

        /// <summary>
        /// Checks for nest level of currently executing application.
        /// </summary>
        /// <returns>nest level of currently executing application (0 for root process, 1 for first level one etc.)</returns>
        public static int GetMyNestLevel()
        {
            return JobEnvVariables.GetNestLevel();
        }

        private string GetExecutableName()
        {
            const string EXECUTABLE_FILENAME_ENV_VAR_NAME = "CONDOR_SHAPP_EXECUTABLE_NAME";
            if (AmIRootProcess())
            {
                // It's a dirty workaround for a way that HTCondor executes commands
                // After submission (when the app is running as a job on remote machine)
                // AppDomain.CurrentDomain.FriendlyName will return "condor_exec.exe". This can't
                // be used for further submission of the jobs. This Env variable will be preserved for every
                // further instance of this app.
                Environment.SetEnvironmentVariable(EXECUTABLE_FILENAME_ENV_VAR_NAME, AppDomain.CurrentDomain.FriendlyName, EnvironmentVariableTarget.Process);
            }
            return Environment.GetEnvironmentVariable(EXECUTABLE_FILENAME_ENV_VAR_NAME);
        }

        private static string BuildAdditionalLibrariesToTransfer()
        {
            string directory = Path.GetDirectoryName(GetExecutableLocation());
            string[] dlls = Directory.GetFiles(directory, "*.dll", SearchOption.TopDirectoryOnly);
            string[] configs = Directory.GetFiles(directory, "*.config", SearchOption.TopDirectoryOnly);
            string[] xmls = Directory.GetFiles(directory, "*.xml", SearchOption.TopDirectoryOnly);
            string[] pdbs = Directory.GetFiles(directory, "*.pdb", SearchOption.TopDirectoryOnly);
            string[] filesListWithoutPaths = new string[0];
            filesListWithoutPaths = filesListWithoutPaths.Concat(dlls).Concat(configs).Concat(xmls).Concat(pdbs).Select(s => Path.GetFileName(s)).ToArray();

            string additionalLibrariesToTransfer = string.Join(", ", filesListWithoutPaths);
            log.DebugFormat("Files to transfer: {0}", additionalLibrariesToTransfer);
            return additionalLibrariesToTransfer;
        }

        private static string GetExecutableLocation()
        {
            return System.Reflection.Assembly.GetExecutingAssembly().Location;
        }
    }
}
