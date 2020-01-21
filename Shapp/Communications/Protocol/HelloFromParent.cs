using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;

namespace Shapp.Communications.Protocol {
    [Serializable]
    public class HelloFromParent : ISystemMessage {
        public delegate void Callback(Socket client, HelloFromParent helloFromChild);
        public static event Callback OnReceive;
        public void Dispatch(Socket sender) {
            OnReceive?.Invoke(sender, this);
        }
    }
}
