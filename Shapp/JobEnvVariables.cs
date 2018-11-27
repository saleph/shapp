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
        public const string SHAPP_ALL_ENV_VARS = SHAPP_ENV_VAR_NAMESPACE + "SHAPP_ALL_ENV_VARS";

        /// <summary>
        /// Workaround for a bug in HTCondor python API. ENV field does not recognize different
        /// variables, so I've prepared a list of such variables to be parsed as a XML and put as
        /// a whole into one env variable.
        /// </summary>
        [DataContract]
        public class EnvVarsList
        {
            [DataMember]
            public IPAddress IPAddress;
            [DataMember]
            public int NestLevel;

            public override string ToString()
            {
                return Serialize();
            }

            public string Serialize()
            {
                var xmlserializer = new XmlSerializer(typeof(EnvVarsList));
                var stringWriter = new StringWriter();
                using (var writer = XmlWriter.Create(stringWriter))
                {
                    xmlserializer.Serialize(writer, this);
                    return stringWriter.ToString();
                }
            }

            public static EnvVarsList Deserialize(string xml)
            {
                if (xml.Length == 0)
                {
                    return new EnvVarsList()
                    {
                        IPAddress = IPAddress.Parse("127.0.0.1"),
                        NestLevel = 0
                    };
                }
                using (var stream = new StringReader(xml))
                {
                    var serializer = new XmlSerializer(typeof(EnvVarsList));
                    return serializer.Deserialize(stream) as EnvVarsList;
                }
            }
        }

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
    }
}
