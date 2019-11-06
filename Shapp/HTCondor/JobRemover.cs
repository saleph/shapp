using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shapp {
    /// <summary> 
    /// Class responsible for prepareing python script for removing a job.
    /// 
    /// It uses base script from Properties.Resources.GetJobStatusScript.
    /// </summary>
    public class JobRemover {
        private readonly JobId JobId;
        private readonly string PythonScriptWithRemover;
        private readonly PythonScriptsExecutor pythonScriptExecutor;

        /// <summary>
        /// Constructs the job remover for one particular job id.
        /// </summary>
        /// <param name="jobId">job's id which state should be removed</param>
        public JobRemover(JobId jobId) {
            JobId = jobId;
            PythonScriptWithRemover = ConstructPythonScript(jobId);
            pythonScriptExecutor = new PythonScriptsExecutor(PythonScriptWithRemover);
        }

        /// <summary>
        /// Order the job to be removed.
        /// </summary>
        public void Remove() {
            pythonScriptExecutor.Execute();
            C.log.Info(string.Format("Removed job from queue with id: {0}", JobId));
        }

        private string ConstructPythonScript(JobId jobId) {
            string pythonScript = Properties.Resources.RemoveJobScript;
            return string.Format(pythonScript,
                jobId);
        }
    }
}
