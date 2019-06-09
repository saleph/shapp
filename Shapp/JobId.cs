using System;
using System.Runtime.Serialization;

namespace Shapp
{
    /// <summary>
    /// Job's id reprezentation.
    /// </summary>
    [Serializable]
    [DataContract]
    public class JobId
    {
        /// <summary>
        /// Cluster id is the ordinal number of submitted job (it's globaly synchronized).
        /// Increments on each new job submission.
        /// </summary>
        [DataMember]
        public int ClusterId { get; private set; }
        /// <summary>
        /// Process id is the ordinal number of submitted job in scope of one cluster id.
        /// If you enqueue 8 same work descriptor in HTCondor, same copy of the job will be
        /// added with consequent numbers, e.g. 122.0, 122.1, 122.3 etc.
        /// </summary>
        [DataMember]
        public int ProcessId { get; private set; }

        /// <summary>
        /// Construct job id from HTCondor format string.
        /// </summary>
        /// <param name="jobIdAsString">HTCondor formatted string ([cluster].[process])</param>
        public JobId(string jobIdAsString)
        {
            ParseJobId(jobIdAsString);
            ValidateIds();
        }

        /// <summary>
        /// Construct job id from explicit cluster and processId
        /// </summary>
        /// <param name="clusterId">job's cluster id</param>
        /// <param name="processId">job's process id</param>
        public JobId(int clusterId, int processId)
        {
            ClusterId = clusterId;
            ProcessId = processId;
            ValidateIds();
        }

        private void ParseJobId(string jobIdAsString)
        {
            string[] split = jobIdAsString.Split('.');
            if (split.Length != 2)
                throw new ShappException(string.Format("'{0}' is not a valid JobId", jobIdAsString));
            try
            {
                ClusterId = int.Parse(split[0]);
                ProcessId = int.Parse(split[1]);
            }
            catch (FormatException e)
            {
                throw new ShappException(string.Format("'{0}' parse failed: {1}", jobIdAsString, e.Message));
            }
        }

        private void ValidateIds()
        {
            if (ClusterId < 1)
                throw new ShappException("ClusterId of the job can't be < 1");
            if (ProcessId < 0)
                throw new ShappException("ProcessId of the job can't be < 0");
        }

        public override string ToString()
        {
            return string.Format("{0}.{1}", ClusterId, ProcessId);
        }
    }
}