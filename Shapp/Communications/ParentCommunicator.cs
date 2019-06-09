using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using Shapp.Communications.Protocol;

namespace Shapp
{
    public static class ParentCommunicator
    {
        private static AsynchronousClient client;

        public static void Initialize()
        {
            client = new AsynchronousClient();
            client.NewMessageReceivedEvent += (classInstance, server) =>
            {
                if (classInstance is ISystemMessage systemMessage)
                    systemMessage.Dispatch(server);
            };
            client.Connect(JobEnvVariables.GetParentSubmitterIp(), JobEnvVariables.GetParentSubmitterDestinationPort());
        }

        public static void Stop()
        {
            client.Stop();
        }

        public static void Send(object message)
        {
            client.Send(message);
        }
    }
}
