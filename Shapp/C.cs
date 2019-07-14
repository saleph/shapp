using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shapp
{
    static class C
    {
        public static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        public const int DEFAULT_PORT = 11001;
        /// <summary>
        /// Default state refresh rate. Describes how often job state is being polled.
        /// </summary>
        public const int DEFAULT_JOB_STATE_REFRESH_INTERVAL_MS = 1000;

        #region Internal parameters
        internal const int SERVER_BACKLOG_SIZE = 100;
        internal const int LOWEST_POSSIBLE_REFRESH_RATE_MS = 100;
        internal const string DEFAULT_IP_ADDRESS = "192.168.64.1";
        internal static string PROTOCOL_LOG_HEADER = "Communications.Protocol";

        #endregion
    }
}
