using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;

namespace Shapp {
    /// <summary>
    /// Workaround for a bug in HTCondor python API. ENV field does not recognize different
    /// variables, so I've prepared a list of such variables to be parsed as a XML and put as
    /// a whole into one env variable.
    /// </summary>
    [DataContract]
    class EnvVarsList {
        [XmlIgnore]
        public IPAddress IPAddress;
        [XmlElement("IPAddress")]
#pragma warning disable IDE0051 // Remove unused private members
#pragma warning disable IDE1006 // Naming Styles
        // workaround for serialization of the IPAddress class
        public string ____zzzxxxxxdfhfjddhfuIPAddressForXml
#pragma warning restore IDE1006 // Naming Styles
#pragma warning restore IDE0051 // Remove unused private members
        {
            get { return IPAddress.ToString(); }
            set {
                IPAddress = string.IsNullOrEmpty(value) ? null :
            IPAddress.Parse(value);
            }
        }
        [DataMember]
        public int NestLevel;
        [DataMember]
        public int CommunicationPort;
        [XmlIgnore]
        public JobId MyJobId {
            get {
                if (___xmlJobId == null) {
                    return null;
                }
                return new JobId(___xmlJobId);
            }
        }
        [XmlElement("MyJobId")]
        public string ___xmlJobId;

        public override string ToString() => Serialize();

        public string Serialize() {
            return this.SerializeToXml();
        }

        public static EnvVarsList Deserialize(string xml) {
            if (xml == null || xml.Length == 0) {
                return new EnvVarsList() {
                    IPAddress = IPAddress.Parse(C.DEFAULT_IP_ADDRESS),
                    NestLevel = 0,
                    CommunicationPort = C.DEFAULT_PORT
                };
            }
            using (var stream = new StringReader(xml)) {
                var serializer = new XmlSerializer(typeof(EnvVarsList));
                return serializer.Deserialize(stream) as EnvVarsList;
            }
        }
    }
}
