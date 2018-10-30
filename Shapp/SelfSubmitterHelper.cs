using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shapp
{
    public class SelfSubmitterHelper
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        private NewJobSubmitter NewJobSubmitter;

        public SelfSubmitterHelper()
        {
            NewJobSubmitter = new NewJobSubmitter()
            {
                Command = Path.GetFileName(GetExecutableLocation()),
                InputFilesToTransferSpaceSeparated = BuildAdditionalLibrariesToTransfer()
            };
            NewJobSubmitter.SubmitNewJob();
        }

        private string BuildAdditionalLibrariesToTransfer()
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

        public bool AmIRootProcess()
        {
            return JobEnvVariables.GetNestLevel() == 0;
        }

        public bool AmIChildProcess()
        {
            return !AmIRootProcess();
        }
    }
}
