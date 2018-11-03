using System;
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
                Command = Path.GetFileName(GetExecutableLocation()),
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

        private static string BuildAdditionalLibrariesToTransfer()
        {
            string directory = Path.GetDirectoryName(GetExecutableLocation());
            string[] dlls = Directory.GetFiles(directory, "*.dll", SearchOption.TopDirectoryOnly);
            string[] configs = Directory.GetFiles(directory, "*.config", SearchOption.TopDirectoryOnly);
            string[] xmls = Directory.GetFiles(directory, "*.xml", SearchOption.TopDirectoryOnly);
            string[] pdbs = Directory.GetFiles(directory, "*.pdb", SearchOption.TopDirectoryOnly);
            string[] filesListWithoutPaths = dlls.Concat(configs).Concat(xmls).Concat(pdbs).Select(s => Path.GetFileName(s)).ToArray();

            string additionalLibrariesToTransfer = string.Join(" ", filesListWithoutPaths);
            log.DebugFormat("Files to transfer: {0}", additionalLibrariesToTransfer);
            return additionalLibrariesToTransfer;
        }

        private static string GetExecutableLocation()
        {
            return System.Reflection.Assembly.GetExecutingAssembly().Location;
        }
    }
}
