using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;

namespace Shapp.Communications.Protocol
{
    [Serializable]
    public class HelloFromChild : ProtocolSerializer, ISystemMessage {
        public delegate void Callback(Socket client, HelloFromChild helloFromChild);
        public static event Callback OnReceive;

        public JobId MyJobId;

        public void Dispatch(Socket sender) {
            C.log.Debug(Serialize());
            OnReceive?.Invoke(sender, this);
        }
    }
}
