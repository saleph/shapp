// Asynchronous Server Socket Example
// http://msdn.microsoft.com/en-us/library/fx6588te.aspx

using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace Shapp {
    public class AsynchronousServer {
        private readonly ManualResetEvent connectionEstablished = new ManualResetEvent(false);
        private readonly object isListeningLock = new object();
        private bool isListening;
        private Socket listenerSocket;
        private readonly int port;

        public bool IsListening { get { lock (isListeningLock) { return isListening; } } set { lock (isListeningLock) { isListening = value; } } }
        private readonly Thread listener;
        private readonly AsynchronousCommunicationUtils asynchronousCommunicationUtils = new AsynchronousCommunicationUtils();

        /// <summary>
        /// Delegate for new messages received by the socket.
        /// </summary>
        /// <param name="classInstance">received object; cast it for your favourite type</param>
        public delegate void NewMessageReceived(object classInstance, Socket client);
        public event NewMessageReceived NewMessageReceivedEvent;

        /// <summary>
        /// Delegate for new clients connected.
        /// </summary>
        /// <param name="client">just connected client</param>
        public delegate void NewClientConnected(Socket client);
        public event NewClientConnected NewClientConnectedEvent;


        public AsynchronousServer(int port) {
            this.port = port;
            asynchronousCommunicationUtils.NewMessageReceivedEvent += OnMessageReceive;
            listener = new Thread(new ThreadStart(ListenForNewConnections));
        }

        private void OnMessageReceive(object classInstance, Socket client) {
            NewMessageReceivedEvent?.Invoke(classInstance, client);
        }

        public void Start() {
            if (!listener.IsAlive) {
                IsListening = true;
                listener.Start();
                C.log.Debug("Server started on port: " + port);
            }
        }

        public void Stop() {
            if (listener.IsAlive) {
                IsListening = false;
                listener.Join();
            }
        }

        private void ListenForNewConnections() {
            IPAddress ipAddress = GetLocalIPAddress();
            IPEndPoint localEndPoint = new IPEndPoint(ipAddress, port);
            C.log.Debug(string.Format("Listening on {0}", localEndPoint.ToString()));

            listenerSocket = new Socket(AddressFamily.InterNetwork,
                SocketType.Stream, ProtocolType.Tcp);

            listenerSocket.Bind(localEndPoint);
            listenerSocket.Listen(C.SERVER_BACKLOG_SIZE);
            connectionEstablished.Reset();
            listenerSocket.BeginAccept(new AsyncCallback(AcceptCallback), listenerSocket);
            while (IsListening) {
                if (connectionEstablished.WaitOne()) {
                    // it was not a timeout - prepare new callback
                    
                }
                connectionEstablished.Reset();
            }
        }

        private static IPAddress GetLocalIPAddress() {
            var host = Dns.GetHostEntry(Dns.GetHostName());
            foreach (var ip in host.AddressList) {
                if (ip.AddressFamily == AddressFamily.InterNetwork) {
                    return ip;
                }
            }
            throw new Exception("No network adapters with an IPv4 address in the system!");
        }

        private void AcceptCallback(IAsyncResult ar) {
            listenerSocket.BeginAccept(new AsyncCallback(AcceptCallback), listenerSocket);
            connectionEstablished.Set();
            Socket listener = (Socket)ar.AsyncState;
            Socket handler = listener.EndAccept(ar);
            C.log.Info("Connection established with a client " + handler.RemoteEndPoint.ToString());
            NewClientConnectedEvent?.Invoke(handler);
            while (IsListening) {
                try {
                    asynchronousCommunicationUtils.ListenForMessages(handler);
                } catch (SocketException) {
                    C.log.Info("Connection lost towards " + handler.RemoteEndPoint.ToString());
                    return;
                }
            }
        }

        public void Send(Socket client, object objectToSend) {
            AsynchronousCommunicationUtils.Send(client, objectToSend);
        }
    }
}