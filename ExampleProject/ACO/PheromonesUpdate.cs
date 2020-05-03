using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace ExampleProject.ACO {
    [Serializable]
internal class PheromonesUpdate : Shapp.ISystemMessage {
    public delegate void Callback(Socket client, PheromonesUpdate pheromonesUpdate);
    public static event Callback OnReceive;

    public double[][] pheromones;

    public void Dispatch(Socket sender) {
        Shapp.C.log.Debug("PheromonesUpdate");
        OnReceive?.Invoke(sender, this);
    }
    
}
}
