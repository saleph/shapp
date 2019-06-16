using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;

namespace Shapp.Communications.Protocol
{
    [Serializable]
    public class HelloFromChild : ISystemMessage
    {
        public string MyJobId;

        public void Dispatch(Socket sender)
        {
            Console.Out.WriteLine("HelloFromChild received from " + MyJobId);
            Console.Out.WriteLine("Sending HelloFromParent...");
            AsynchronousCommunicationUtils.Send(sender, new HelloFromParent());
        }
    }
}
