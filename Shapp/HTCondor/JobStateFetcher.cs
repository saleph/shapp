using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace Shapp {
    /// <summary>
    /// Class responsible for prepareing python script for fetching info about a job.
    /// 
    /// It uses base script from Properties.Resources.GetJobStatusScript.
    /// Essencialy it puts job's clusterId and processNo into the string, which will be
    /// interpreted as python script.
    /// This task can be also done via just executing cli command like "condor_q" and "condor_history".
    /// And no - IronPython can't be used. HTCondor python api is precomplied int .pyd, it can't
    /// be used in IronPython environment.
    /// </summary>
    class JobStateFetcher {
        private const string JOB_STATUS_PROPERTY_LABEL = "JobStatus";

        private readonly JobId JobId;
        private readonly string PythonScriptWithFetcher;
        private readonly PythonScriptsExecutor pythonScriptExecutor;

        /// <summary>
        /// Constructs the status fetcher for one particular job id.
        /// </summary>
        /// <param name="jobId">job's id which state should be watched</param>
        public JobStateFetcher(JobId jobId) {
            JobId = jobId;
            PythonScriptWithFetcher = ConstructPythonScript(jobId);
            pythonScriptExecutor = new PythonScriptsExecutor(PythonScriptWithFetcher);
        }

        /// <summary>
        /// Polls htcondor api for the status of the job defined in constructor.
        /// </summary>
        /// <returns>current job state</returns>
        public JobState GetJobState() {
            pythonScriptExecutor.Execute();
            string jobProperties = pythonScriptExecutor.Response;
            if (jobProperties.Length == 0) {
                throw new ShappException("Fetching JobState failed");
            }
            Dictionary<string, string> jobStatesCache = GetJobStatesCacheFromXml(jobProperties);
            int jobStateId = GetJobStateId(jobStatesCache);
            JobState jobState = (JobState)jobStateId;
            C.log.Debug(string.Format("Got job state info about job {0}: {1}", JobId, jobState));
            return jobState;
        }

        private static Dictionary<string, string> GetJobStatesCacheFromXml(string jobProperties) {
            XmlDocument doc = new XmlDocument();
            doc.LoadXml(jobProperties);
            Dictionary<string, string> jobStatesCache = new Dictionary<string, string>();
            foreach (XmlNode n in doc.SelectNodes("/root/*")) {
                jobStatesCache[n.Name] = n.InnerText;
            }
            return jobStatesCache;
        }

        private string ConstructPythonScript(JobId jobId) {
            string pythonScript = Properties.Resources.GetJobStatusScript;
            return string.Format(pythonScript,
                jobId.ClusterId,
                jobId.ProcessId);
        }

        private int GetJobStateId(Dictionary<string, string> jobProperties) {
            try {
                return int.Parse(jobProperties[JOB_STATUS_PROPERTY_LABEL]);
            } catch (KeyNotFoundException) {
                throw new ShappException(string.Format("Attempt to get status of the job {0} - it doesn't exists", JobId));
            }
        }
    }
}
