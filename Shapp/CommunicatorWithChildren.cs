using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Shapp {
    public static class CommunicatorWithChildren {

        private static AsynchronousServer server = null;
        private static bool isInitialized = false;
        private static readonly Dictionary<JobId, Socket> jobIdToSocket = new Dictionary<JobId, Socket>();

        public static void InitializeServer(AsynchronousServer.NewMessageReceived[] additionalCallbacks = null) {
            if (isInitialized)
                return;
            server = new AsynchronousServer(JobEnvVariables.GetMyDestinationPortForChildren());
            AddDelegateForMessagesDispatcher();
            AddDelegateForNewChildGreeting();
            AddDelegatesForProtocolMessages();
            if (additionalCallbacks != null) {
                Array.ForEach(additionalCallbacks, callback => server.NewMessageReceivedEvent += callback);
            }
            server.Start();
            isInitialized = true;
        }

        private static void AddDelegateForNewChildGreeting() {
            server.NewClientConnectedEvent += (socket) => {
                C.log.Info("Sending HelloFromParent for greeting");
                server.Send(socket, new Communications.Protocol.HelloFromParent());
            };
        }

        private static void AddDelegateForMessagesDispatcher() {
            server.NewMessageReceivedEvent += (objectRecv, sock) => {
                if (objectRecv is ISystemMessage hello)
                    hello.Dispatch(sock);
            };
        }

        private static void AddDelegatesForProtocolMessages() {
            Communications.Protocol.HelloFromChild.OnReceive += (socket, helloFromChild) => {
                C.log.Info("Received HelloFromChild from " + helloFromChild.MyJobId + ", ip: " + socket.RemoteEndPoint.ToString());
                jobIdToSocket.Add(helloFromChild.MyJobId, socket);
            };
        }

        public static void SendToChild(JobId jobId, object objectToSend) {
            if (!isInitialized)
                throw new ShappException("before sending a message you have to initialize the CommunicatorWithChildren");
            server.Send(jobIdToSocket[jobId], objectToSend);
        }

        public static void Stop() {
            if (!isInitialized)
                throw new ShappException("server is already down");
            server.Stop();
        }
    }
}
