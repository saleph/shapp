using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Shapp.Communications {
    public static class CommunicatorWithChildren {

        private static AsynchronousServer server = null;
        private static bool IsInitialized = false;
        private static readonly Dictionary<JobId, Socket> jobIdToSocket = new Dictionary<JobId, Socket>();

        public static void InitializeServer(AsynchronousServer.NewMessageReceived[] additionalCallbacks = null) {
            if (IsInitialized)
                return;
            server = new AsynchronousServer(JobEnvVariables.GetMyDestinationPortForChildren());
            server.NewMessageReceivedEvent += (objectRecv, sock) => {
                if (objectRecv is ISystemMessage hello)
                    hello.Dispatch(sock);
            };
            if (additionalCallbacks != null) {
                Array.ForEach(additionalCallbacks, callback => server.NewMessageReceivedEvent += callback);
            }
            server.Start();
            IsInitialized = true;
        }

        public static void SendToChild(JobId jobId) {
            if (!IsInitialized)
                throw new ShappException("before sending a message you have to initialize the CommunicatorWithChildren");

        }


    }
}
