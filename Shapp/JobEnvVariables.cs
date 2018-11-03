using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Shapp
{
    /// <summary>
    /// Static class agregating access to environmental variables in scope of remote job monitoring.
    /// </summary>
    class JobEnvVariables
    {
        private const string SHAPP_ENV_VAR_NAMESPACE = "SHAPP_CONDOR_";
        /// <summary>
        /// Nest level environmental variable name.
        /// </summary>
        public const string NEST_LEVEL_NAME = SHAPP_ENV_VAR_NAMESPACE + "NEST_LEVEL";
        /// <summary>
        /// Parent submitter IP address variable name.
        /// </summary>
        public const string PARENT_SUBMITTER_IP_NAME = SHAPP_ENV_VAR_NAMESPACE + "PARENT_SUBMITTER_IP";

        /// <summary>
        /// Acquires nest level of currently executing app instance. It's describes how deep in tree structure
        /// the current instance is executing. Root app instance has nest level equals 0. It's childred has 1 etc.
        /// </summary>
        /// <returns>Nest level.</returns>
        public static int GetNestLevel()
        {
            string nestLevel = Environment.GetEnvironmentVariable(NEST_LEVEL_NAME);
            return ParseNumericalEnvVariable(nestLevel);
        }

        /// <summary>
        /// Acquires IP address of parent submitter. 
        /// If such address is not available the ShappException is being thrown.
        /// </summary>
        /// <returns>IPAddress of parent submitter</returns>
        public static IPAddress GetParentSubmitterIp()
        {
            string iPAddressAsString = Environment.GetEnvironmentVariable(PARENT_SUBMITTER_IP_NAME);
            IPAddress iPAddress = ParseIpAddressFromEnvVariable(iPAddressAsString);
            return iPAddress;
        }

        private static IPAddress ParseIpAddressFromEnvVariable(string iPAddressAsString)
        {
            try
            {
                return IPAddress.Parse(iPAddressAsString);
            }
            catch (ArgumentNullException)
            {
                throw new ShappException(string.Format("{0} env variable was not specified", PARENT_SUBMITTER_IP_NAME));
            }
            catch (FormatException)
            {
                throw new ShappException(string.Format("{0} env variable value: {1} can't be interpreted as IPAddress", 
                    PARENT_SUBMITTER_IP_NAME, iPAddressAsString));
            }
        }

        private static int ParseNumericalEnvVariable(string nestLevel)
        {
            return string.IsNullOrEmpty(nestLevel) ? 0 : int.Parse(nestLevel);
        }
    }
}
