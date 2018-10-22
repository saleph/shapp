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

        public string Command = "";
        public string WorkingDirectory = "";
        public string LogFileName = "";
        public string UserStandardOutputFileName = "";
        public string StandardErrorFileName = "";
        public string UserStandardInputFileName = "";
        public string InputFilesToTransferSpaceSeparated = "";
        public string CommandCliArguments = "";
        public string ShouldTransferFiles = "YES";

        public JobId SubmitNewJob()
        {
            ValidateParameters();
            string pythonScirpt = ConstructPythonScript();
            PythonScriptsExecutor executor = new PythonScriptsExecutor(pythonScirpt);
            executor.Execute();
            string jobIdAsString = executor.Response;
            return new JobId(jobIdAsString);
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
                ShouldTransferFiles);
        }
    }
}
