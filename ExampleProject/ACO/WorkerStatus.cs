using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace ExampleProject.ACO {
    [Serializable]
    internal class WorkerStatus : Shapp.ISystemMessage {
        public delegate void Callback(Socket client, WorkerStatus workerStatus);
        public static event Callback OnReceive;

        public int[] bestTrail;
        public double bestPathLength;
        public double[][] pheromones;
        public Shapp.JobId MyJobId = Shapp.JobEnvVariables.GetMyJobId();
        public int iterations;

        public void Dispatch(Socket sender) {
            Shapp.C.log.Debug("WorkerStatus");
            OnReceive?.Invoke(sender, this);
        }
    }
}
