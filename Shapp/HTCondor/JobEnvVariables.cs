using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;

namespace Shapp
{
    /// <summary>
    /// Static class agregating access to environmental variables in scope of remote job monitoring.
    /// </summary>
    class JobEnvVariables
    {
        public const string SHAPP_ENV_VAR_NAMESPACE = "SHAPP_CONDOR_";
        /// <summary>
        /// EnvVarsList env variable name.
        /// </summary>
        public const string SHAPP_ALL_ENV_VARS = SHAPP_ENV_VAR_NAMESPACE + "ALL_ENV_VARS";
        
        private static readonly EnvVarsList JobVariables 
            = EnvVarsList.Deserialize(Environment.GetEnvironmentVariable(SHAPP_ALL_ENV_VARS));

        /// <summary>
        /// Acquires nest level of currently executing app instance. It's describes how deep in tree structure
        /// the current instance is executing. Root app instance has nest level equals 0. It's childred has 1 etc.
        /// </summary>
        /// <returns>Nest level</returns>
        public static int GetNestLevel()
        {
            return JobVariables.NestLevel;
        }

        /// <summary>
        /// Acquires IP address of parent submitter. 
        /// </summary>
        /// <returns>IPAddress of parent submitter</returns>
        public static IPAddress GetParentSubmitterIp()
        {
            return JobVariables.IPAddress;
        }

        public static int GetParentSubmitterDestinationPort()
        {
            return JobVariables.CommunicationPort;
        }

        public static int GetMyDestinationPortForChildren()
        {
            return JobVariables.CommunicationPort + 1;
        }
    }
}
