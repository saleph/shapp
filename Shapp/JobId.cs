using System;

namespace Shapp
{
    public class JobId
    {
        public int ClusterId;
        public int ProcessId;

        public JobId(string jobIdAsString)
        {
            ParseJobId(jobIdAsString);
        }

        public JobId(int clusterId, int processId)
        {
            ClusterId = clusterId;
            ProcessId = processId;
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

        public override string ToString()
        {
            return string.Format("{0}.{1}", ClusterId, ProcessId);
        }
    }
}