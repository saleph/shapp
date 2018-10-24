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
        // this should probably be tuned during the program - with e.g. counting how many jobs were submitted so far (times 2 e.g.)
        private const int MAX_HISTORY_ENTRIES_TO_QUERY = 1000;
        private const string JOB_STATUS_PROPERTY_LABEL = "JobStatus";
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private Dictionary<string, Dictionary<string, string>> jobStatesCache = null;

        public JobState GetJobState(JobId jobId)
        {
            UpdateCache();
            Dictionary<string, string> jobProperties = GetPropertiesOfTheJob(jobId);
            int jobStateId = GetJobStateId(jobProperties);
            return (JobState)jobStateId;
        }

        private void UpdateCache()
        {
            string pythonScirpt = ConstructPythonScript();
            PythonScriptsExecutor executor = new PythonScriptsExecutor(pythonScirpt);
            executor.Execute();
            string jobStatuses = executor.Response;
            jobStatesCache = JsonConvert.DeserializeObject<Dictionary<string, Dictionary<string, string>>>(jobStatuses);
        }

        private string ConstructPythonScript()
        {
            string pythonScript = Properties.Resources.GetJobStatusScript;
            return string.Format(pythonScript, MAX_HISTORY_ENTRIES_TO_QUERY);
        }

        private Dictionary<string, string> GetPropertiesOfTheJob(JobId jobId)
        {
            if (!jobStatesCache.TryGetValue(jobId.ToString(), out Dictionary<string, string> jobProperties))
            {
                throw new ShappException(string.Format("Attempt to get state of not existing JobId {0}", jobId));
            }

            return jobProperties;
        }

        private static int GetJobStateId(Dictionary<string, string> jobProperties)
        {
            return int.Parse(jobProperties[JOB_STATUS_PROPERTY_LABEL]);
        }
    }
}
