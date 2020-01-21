// Asynchronous Client Socket Example
// http://msdn.microsoft.com/en-us/library/bew39x2a.aspx

using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Text;
using static Shapp.AsynchronousServer;
using static Shapp.AsynchronousCommunicationUtils;

namespace Shapp {
    public class AsynchronousClient {
        // ManualResetEvent instances signal completion.
        private static readonly ManualResetEvent connectDone =
            new ManualResetEvent(false);

        private readonly AsynchronousCommunicationUtils asynchronousCommunicationUtils = new AsynchronousCommunicationUtils();

        private readonly object isListeningLock = new object();
        private bool isListening;
        private bool IsListening { get { lock (isListeningLock) { return isListening; } } set { lock (isListeningLock) { isListening = value; } } }

        /// <summary>
        /// Delegate for new messages received by the socket.
        /// </summary>
        /// <param name="classInstance">received object; cast it for your favourite type</param>
        public delegate void NewMessageReceived(object classInstance, Socket client);
        public event NewMessageReceived NewMessageReceivedEvent;

        private Socket client;

        public AsynchronousClient() {
            asynchronousCommunicationUtils.NewMessageReceivedEvent += OnMessageReceive;
        }

        private void OnMessageReceive(object classInstance, Socket client) {
            NewMessageReceivedEvent?.Invoke(classInstance, client);
        }

        public void Connect(IPAddress ipAddress, int port = C.DEFAULT_PORT) {
            IPEndPoint remoteEP = new IPEndPoint(ipAddress, port);
            client = new Socket(AddressFamily.InterNetwork,
                SocketType.Stream, ProtocolType.Tcp);
            IsListening = true;
            client.BeginConnect(remoteEP,
                new AsyncCallback(ConnectCallback), client);
            C.log.Debug("Connection established towards " + remoteEP.ToString());
        }

        public void Stop() {
            IsListening = false;
        }

        private void ConnectCallback(IAsyncResult ar) {
            Socket client = (Socket)ar.AsyncState;
            client.EndConnect(ar);
            connectDone.Set();
            while (IsListening) {
                try {
                    asynchronousCommunicationUtils.ListenForMessages(client);
                } catch (SocketException) {
                    C.log.Info("Connection lost towards " + client.ToString());
                    return;
                }
            }
        }

        public void Send(object objectToSend) {
            connectDone.WaitOne();
            AsynchronousCommunicationUtils.Send(client, objectToSend);
        }
    }
}