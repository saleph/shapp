﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;

namespace Shapp
{
    /// <summary>
    /// Workaround for a bug in HTCondor python API. ENV field does not recognize different
    /// variables, so I've prepared a list of such variables to be parsed as a XML and put as
    /// a whole into one env variable.
    /// </summary>
    [DataContract]
    public class EnvVarsList
    {
        [XmlIgnore]
        public IPAddress IPAddress;
        [XmlElement("IPAddress")]
        private string IPAddressForXml
        {
            get { return IPAddress.ToString(); }
            set
            {
                IPAddress = string.IsNullOrEmpty(value) ? null :
            IPAddress.Parse(value);
            }
        }
        [DataMember]
        public int NestLevel;

        public override string ToString() => Serialize();

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
            if (xml == null || xml.Length == 0)
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
}
