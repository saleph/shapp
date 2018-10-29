using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Shapp
{
    class JobEnvVariables
    {
        public const string NEST_LEVEL_NAME = "CONDOR_SHAPP_NEST_LEVEL";
        public const string PARENT_SUBMITTER_IP_NAME = "PARENT_SUBMITTER_IP";

        public static int GetNestLevel()
        {
            string nestLevel = Environment.GetEnvironmentVariable(NEST_LEVEL_NAME);
            return ParseNumericalEnvVariable(nestLevel);
        }

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
