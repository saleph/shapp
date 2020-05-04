using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Shapp.Communications.Protocol {
    [Serializable]
    public class StopWorker : ISystemMessage {
        public delegate void Callback(Socket client, StopWorker stopWorker);
        public static event Callback OnReceive;
        public void Dispatch(Socket sender) {
            OnReceive?.Invoke(sender, this);
        }
    }
}
