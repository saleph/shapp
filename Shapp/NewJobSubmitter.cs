using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shapp
{
    class NewJobSubmitter
    {
        public string Command = "";
        public string WorkingDirectory = System.Reflection.Assembly.GetExecutingAssembly().Location;
        public string LogFileName = "";
        public string UserStandardOutputFileName = "";
        public string StandardErrorFileName = "";
        public string UserStandardInputFileName = "";
        // space separated
        public string InputFilesToTransfer = "";
        public string CommandCliArguments = "";
        public string ShouldTransferFiles = "YES";

        public JobId SubmitNewJob()
        {
            string pythonScirpt = ConstructPythonScript();
        }

        private string ConstructPythonScript()
        {
            string pythonScript = Properties.Resources.SubmitNewJobScript;
            return string.Format(pythonScript,
                    Command,
                WorkingDirectory,
                LogFileName,
                UserStandardOutputFileName,
                StandardErrorFileName,
                UserStandardInputFileName,
                InputFilesToTransfer,
                CommandCliArguments,
                ShouldTransferFiles);
        }
    }
}
