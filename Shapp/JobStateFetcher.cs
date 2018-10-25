using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Shapp
{
    public class JobStateFetcher
    {
        private const string JOB_STATUS_PROPERTY_LABEL = "JobStatus";
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        
        private readonly JobId JobId;
        private readonly string PythonScriptWithFetcher;
        private readonly PythonScriptsExecutor pythonScriptExecutor;

        public JobStateFetcher(JobId jobId)
        {
            JobId = jobId;
            PythonScriptWithFetcher = ConstructPythonScript(jobId);
            pythonScriptExecutor = new PythonScriptsExecutor(PythonScriptWithFetcher);
        }

        public JobState GetJobState()
        {
            pythonScriptExecutor.Execute();
            string jobProperties = pythonScriptExecutor.Response;
            Dictionary<string, string> jobStatesCache = JsonConvert.DeserializeObject<Dictionary<string, string>>(jobProperties);
            int jobStateId = GetJobStateId(jobStatesCache);
            JobState jobState = (JobState)jobStateId;
            log.InfoFormat("Got job state info about job {0}: {1}", JobId, jobState);
            return jobState;
        }

        private string ConstructPythonScript(JobId jobId)
        {
            string pythonScript = Properties.Resources.GetJobStatusScript;
            return string.Format(pythonScript, 
                jobId.ClusterId, 
                jobId.ProcessId);
        }

        private int GetJobStateId(Dictionary<string, string> jobProperties)
        {
            try
            {
                return int.Parse(jobProperties[JOB_STATUS_PROPERTY_LABEL]);
            } catch (KeyNotFoundException)
            {
                throw new ShappException(string.Format("Attempt to get status of the job {0} - it doesn't exists", JobId));
            }
        }
    }
}
