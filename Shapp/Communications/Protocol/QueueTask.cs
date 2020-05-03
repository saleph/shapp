using Shapp.Utils.WorkQueue;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;


namespace Shapp.Communications.Protocol {
    [Serializable]
    public class QueueTask : ISystemMessage {
        public delegate void Callback(Socket client, QueueTask queueTask);
        public static event Callback OnReceive;
        internal static readonly string DEFAULT_NAME = "Default task";

        public delegate IData TaskFunction(IData input);

        /// <summary>
        /// Task name. The return message will have the same name.
        /// </summary>
        public string Name = DEFAULT_NAME;
        /// <summary>
        /// Input data structures to work with.
        /// </summary>
        public IData InputData;
        /// <summary>
        /// Function to run.
        /// </summary>
        public TaskFunction functionToRun;


        public void Dispatch(Socket sender) {
            OnReceive?.Invoke(sender, this);
        }
    }
}
