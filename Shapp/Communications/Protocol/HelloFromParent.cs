using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;

namespace Shapp.Communications.Protocol
{
    [Serializable]
    public class HelloFromParent : ISystemMessage
    {
        public void Dispatch(Socket sender)
        {
            Console.Out.WriteLine("HelloFromParent received from " + sender.LocalEndPoint);
            Console.Out.WriteLine("Sending HelloFromChild...");
            AsynchronousCommunicationUtils.Send(sender, new HelloFromChild());
            //System.Threading.Thread.Sleep(100);
            //ParentCommunicator.Stop();
        }
    }
}
