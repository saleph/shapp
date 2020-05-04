using Shapp.Utils.WorkQueue;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;


namespace Shapp.Utils.WorkQueue {
    [Serializable]
    public class QueueTask : ISystemMessage {
        public delegate void Callback(Socket client, QueueTask queueTask);
        public static event Callback OnReceive;
        internal static readonly string DEFAULT_NAME = "Default task";
        private static int COUNTER = 0;

        public delegate IData TaskFunction(IData input);

        /// <summary>
        /// Task counter, not meant to be changed.
        /// </summary>
        public readonly int Id = Interlocked.Increment(ref COUNTER);

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
