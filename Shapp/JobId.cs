using System;

namespace Shapp
{
    public class JobId
    {
        public int ClusterId;
        public int ProcessId;

        public JobId() { }

        public JobId(string jobIdAsString)
        {
            ParseJobId(jobIdAsString);
        }

        private void ParseJobId(string jobIdAsString)
        {
            string[] split = jobIdAsString.Split('.');
            if (split.Length != 2)
                throw ShappException(string.Format("'{0}' is not a valid JobId", jobIdAsString));
            try
            {
                ClusterId = int.Parse(split[0]);
                ProcessId = int.Parse(split[1]);
            }
            catch (FormatException e)
            {
                throw ShappException(string.Format("'{0}' parse failed: {1}", jobIdAsString, e.Message));
            }
        }

        private Exception ShappException(string v)
        {
            throw new NotImplementedException();
        }

        public override string ToString()
        {
            return string.Format("{0}.{1}", ClusterId, ProcessId);
        }
    }
}