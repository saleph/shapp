using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Shapp.Communications {
    class ChildCommunicator {
        private static AsynchronousServer server;
        private static bool isInitialized = false;

        public static void Initialize() {
            if (isInitialized)
                return;
            server = new AsynchronousServer(JobEnvVariables.GetMyDestinationPortForChildren());
            server.NewMessageReceivedEvent += (classInstance, server) => {
                if (classInstance is ISystemMessage systemMessage)
                    systemMessage.Dispatch(server);
            };
            server.Start();
            isInitialized = true;
        }

        public static void Stop() {
            if (!isInitialized)
                return;
            server.Stop();
            isInitialized = false;
        }

        public static void Send(Socket target, object message) {
            if (!isInitialized)
                throw new ShappException("before sending a message you have to initialize the ChildCommunicator");
            server.Send(target, message);
        }
    }
}
