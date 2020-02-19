using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;
using System.Threading;

namespace Shapp.Communications.Protocol {
    [Serializable]
    public class HelloFromParent : ISystemMessage {
        public delegate void Callback(Socket client, HelloFromParent helloFromChild);
        public static event Callback OnReceive;
        public void Dispatch(Socket sender) {
            Shapp.C.log.Info("HelloFromParent received");
            //Thread.Sleep(1000);
            OnReceive?.Invoke(sender, this);
        }
    }
}
