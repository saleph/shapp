using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shapp
{
    public class NewJobSubmitter
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        private const string LINUX_TARGET_OPERATING_SYSTEM = "target.OpSys == \"LINUX\"";
        private const string WINDOWS_TARGET_OPERATING_SYSTEM = "target.OpSys == \"WINDOWS\"";
        private const string NEST_LEVEL_ENV_VARIABLE_NAME = "CONDOR_SHAPP_NEST_LEVEL";
        private const string PARENT_SUBMITTER_IP_ENV_VARIABLE_NAME = "PARENT_SUBMITTER_IP";

        public string Command = "";
        public string WorkingDirectory = "";
        public string LogFileName = "";
        public string UserStandardOutputFileName = "";
        public string StandardErrorFileName = "";
        public string UserStandardInputFileName = "";
        public string InputFilesToTransferSpaceSeparated = "";
        public string CommandCliArguments = "";
        public string AdditionalJobEnvironmentalVariables = "";
        private string ShouldTransferFiles = "IF_NEEDED";
        private string Requirements = "";
        

        public JobDescriptor SubmitNewJob()
        {
            ValidateParameters();
            string pythonScirpt = ConstructPythonScript();
            PythonScriptsExecutor executor = new PythonScriptsExecutor(pythonScirpt);
            executor.Execute();
            string jobIdAsString = executor.Response;
            JobId jobId = new JobId(jobIdAsString);
            return new JobDescriptor(jobId);
        }

        private void ValidateParameters()
        {
            if (Command.Length == 0)
                throw new ShappException("Newly submitting job cannot have empty Command");
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
                ShouldTransferFiles,
                BuildEnvironmentalVariables(),
                Requirements);
        }

        private string BuildEnvironmentalVariables()
        {
            return string.Format("{0}={1} {2}={3} {4}",
                NEST_LEVEL_ENV_VARIABLE_NAME, GetNestLevel() + 1,
                PARENT_SUBMITTER_IP_ENV_VARIABLE_NAME, GetThisNodeIpAddress(),
                AdditionalJobEnvironmentalVariables);
        }

        private static string GetThisNodeIpAddress()
        {
            return "ip";
        }

        private static int GetNestLevel()
        {
            string nestLevel = Environment.GetEnvironmentVariable(NEST_LEVEL_ENV_VARIABLE_NAME);
            return ParseNumericalEnvVariable(nestLevel);
        }

        private static int ParseNumericalEnvVariable(string nestLevel)
        {
            return string.IsNullOrEmpty(nestLevel) ? 0 : int.Parse(nestLevel);
        }
    }
}
