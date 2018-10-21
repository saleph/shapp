namespace Shapp
{
    internal class JobId
    {
        public int ClusterId;
        public int ProcessId;

        public override string ToString()
        {
            return string.Format("{0}.{1}", ClusterId, ProcessId);
        }
    }
}