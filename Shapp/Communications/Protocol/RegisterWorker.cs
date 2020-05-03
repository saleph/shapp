using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Shapp.Communications.Protocol {
    [Serializable]
    public class RegisterWorker : ISystemMessage {
        public delegate void Callback(Socket client, RegisterWorker registerWorker);
        public static event Callback OnReceive;
        public JobId JobId;
        public void Dispatch(Socket sender) {
            OnReceive?.Invoke(sender, this);
        }
    }
}
