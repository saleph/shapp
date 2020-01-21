using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace ExampleProject.ACO {
    [Serializable]
    class StartProcessing : Shapp.ISystemMessage {
        public delegate void Callback(Socket client, StartProcessing pheromonesUpdate);
        public static event Callback OnReceive;

        public void Dispatch(Socket sender) {
            Shapp.C.log.Debug("StartProcessing");
            OnReceive?.Invoke(sender, this);
        }

    }
}
