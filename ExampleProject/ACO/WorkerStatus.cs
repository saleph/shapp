using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace ExampleProject.ACO {
    class WorkerStatus : Shapp.Communications.Protocol.ProtocolSerializer, Shapp.ISystemMessage {
        public delegate void Callback(Socket client, WorkerStatus workerStatus);
        public static event Callback OnReceive;

        public int[] bestTrail;
        public int bestPathLength;
        public double[][] pheromones;

        public void Dispatch(Socket sender) {
            Shapp.C.log.Debug("WorkerStatus");
            OnReceive?.Invoke(sender, this);
        }
    }
}
