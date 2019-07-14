using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shapp.Communications.Protocol {
    public class ProtocolSerializer {
        protected string Serialize() {
            return string.Format("{0}: {1}", C.PROTOCOL_LOG_HEADER, this.SerializeToXml());
        }
    }
}
