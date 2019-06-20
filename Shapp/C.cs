using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shapp
{
    class C
    {
        public const int DEFAULT_PORT = 11001;
        /// <summary>
        /// Default state refresh rate. Describes how often job state is being polled.
        /// </summary>
        public const int DEFAULT_JOB_STATE_REFRESH_INTERVAL_MS = 1000;

        #region Internal parameters
        public const int SERVER_BACKLOG_SIZE = 100;
        public const int LOWEST_POSSIBLE_REFRESH_RATE_MS = 100;
        public const string DEFAULT_IP_ADDRESS = "192.168.64.1";
        #endregion
    }
}
