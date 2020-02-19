using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shapp {
    public static class C {
        static public SimpleLogger log = new SimpleLogger();
        public const int DEFAULT_PORT = 11001;
        /// <summary>
        /// Default state refresh rate. Describes how often job state is being polled.
        /// </summary>
        public const int DEFAULT_JOB_STATE_REFRESH_INTERVAL_MS = 3000;

        #region Internal parameters
        internal const int SERVER_BACKLOG_SIZE = 20;
        internal const int LOWEST_POSSIBLE_REFRESH_RATE_MS = 100;
        internal const string DEFAULT_IP_ADDRESS = "192.168.164.65";
        internal static readonly int eventWaitTime = 100000;
        internal static readonly int numberOfBytesToShowFromReceivedMsg = 50;
        internal static readonly int socketConnectAttempts = 10;
        internal static readonly int socketConnectAttemptTimeoutMs = 50000;
        internal static string PROTOCOL_LOG_HEADER = "Communications.Protocol";

        #endregion
    }
}
