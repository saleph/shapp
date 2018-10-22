using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shapp
{
    public class NewJobSubmitter
    {
        public string Command = "";
        public string WorkingDirectory = System.Reflection.Assembly.GetExecutingAssembly().Location;
        public string LogFileName = "";
        public string UserStandardOutputFileName = "";
        public string StandardErrorFileName = "";
        public string UserStandardInputFileName = "";
        public string InputFilesToTransferSpaceSeparated = "";
        public string CommandCliArguments = "";
        public string ShouldTransferFiles = "YES";

        public JobId SubmitNewJob()
        {
            string pythonScirpt = ConstructPythonScript();
            Console.Out.WriteLine(pythonScirpt);
            return new JobId();
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
                InputFilesToTransferSpaceSeparated,
                CommandCliArguments,
                ShouldTransferFiles);
        }
    }
}
