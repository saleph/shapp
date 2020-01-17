using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using Shapp.Communications.Protocol;

namespace Shapp {
    public static class CommunicatorToParent {
        private static AsynchronousClient client;
        private static bool isInitialized = false;

        public static void Initialize() {
            if (isInitialized)
                return;
            client = new AsynchronousClient();
            client.NewMessageReceivedEvent += (classInstance, server) => {
                if (classInstance is ISystemMessage systemMessage)
                    systemMessage.Dispatch(server);
            };
            client.Connect(JobEnvVariables.GetParentSubmitterIp(), JobEnvVariables.GetParentSubmitterDestinationPort());
            isInitialized = true;
        }

        public static void Stop() {
            if (!isInitialized)
                return;
            client.Stop();
            isInitialized = false;
        }

        public static void Send(object message) {
            if (!isInitialized)
                throw new ShappException("before sending a message you have to initialize the CommunicatorToParent");
            client.Send(message);
        }
    }
}
