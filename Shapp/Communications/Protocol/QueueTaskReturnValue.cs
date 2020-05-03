using Shapp.Utils.WorkQueue;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;


namespace Shapp.Communications.Protocol {
    [Serializable]
    public class QueueTaskReturnValue : ISystemMessage {
        public delegate void Callback(Socket client, QueueTaskReturnValue queueTask);
        public static event Callback OnReceive;

        /// <summary>
        /// Task name. The return message will have the same name.
        /// </summary>
        public string Name = QueueTask.DEFAULT_NAME;
        /// <summary>
        /// Input data structures to work with.
        /// </summary>
        public IData OutputData;

        public void Dispatch(Socket sender) {
            OnReceive?.Invoke(sender, this);
        }
    }
}
