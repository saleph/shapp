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
        private IPEndPoint remoteEP;

        private bool IsListening { get { lock (isListeningLock) { return isListening; } } set { lock (isListeningLock) { isListening = value; } } }

        /// <summary>
        /// Delegate for new messages received by the socket.
        /// </summary>
        /// <param name="classInstance">received object; cast it for your favourite type</param>
        public delegate void NewMessageReceived(object classInstance, Socket client);
        public event NewMessageReceived NewMessageReceivedEvent;

        private Socket client;
        private Thread receiverThread;

        public AsynchronousClient() {
            asynchronousCommunicationUtils.NewMessageReceivedEvent += OnMessageReceive;
        }

        private void OnMessageReceive(object classInstance, Socket client) {
            NewMessageReceivedEvent?.Invoke(classInstance, client);
        }

        public IAsyncResult AsyncConnect(IPAddress ipAddress, int port = C.DEFAULT_PORT) {
            IPEndPoint remoteEP = new IPEndPoint(ipAddress, port);
            client = new Socket(AddressFamily.InterNetwork,
                SocketType.Stream, ProtocolType.Tcp);
            IsListening = true;
            return client.BeginConnect(remoteEP,
                new AsyncCallback(ConnectCallback), client);
        }

        public void Connect(IPAddress ipAddress, int port = C.DEFAULT_PORT) {
            var attempts = C.socketConnectAttempts;
            IPEndPoint remoteEP = new IPEndPoint(ipAddress, port);
            IsListening = true;

            while (attempts-- > 0) {
                client = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                var result = client.BeginConnect(remoteEP, new AsyncCallback(ConnectCallback), client);
                result.AsyncWaitHandle.WaitOne(C.socketConnectAttemptTimeoutMs, true);
                if (client.Connected) {
                    return;
                } else {
                    //C.log.Info("Connection not established towards " + client.RemoteEndPoint.ToString() + "... Retrying");
                    client.Close();
                    continue;
                }
            }
            throw new ShappException(string.Format("Connection towards {0} failed", remoteEP.ToString()));
        }
        public void ConnectNew(IPAddress ipAddress, int port = C.DEFAULT_PORT) {
            var attempts = C.socketConnectAttempts;
            remoteEP = new IPEndPoint(ipAddress, port);
            IsListening = true;

            client = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            while (attempts-- > 0) {
                Thread.Sleep(1000);
                try {
                    client.Connect(remoteEP);
                } catch {

                }

                if (client.Connected) {
                    receiverThread = new Thread(new ThreadStart(StartReceive));
                    Thread.Sleep(1000);
                    return;
                } else {
                    C.log.Info("Connection not established towards " + client.RemoteEndPoint.ToString() + "... Retrying");
                    continue;
                }
            }
            throw new ShappException(string.Format("Connection towards {0} failed", remoteEP.ToString()));
        }

        public void Stop() {
            IsListening = false;
        }
        private void StartReceive() {
            C.log.Info("Connection established towards " + client.RemoteEndPoint.ToString());
            while (true) {
                try {
                    asynchronousCommunicationUtils.ListenForMessages(client);
                } catch (SocketException) {
                    C.log.Info("Connection lost towards " + client.RemoteEndPoint.ToString());
                    return;
                }
            }
        }

        private void ConnectCallback(IAsyncResult ar) {
            Socket client = (Socket)ar.AsyncState;
            client.EndConnect(ar);
            connectDone.Set();
            C.log.Info("Connection established towards " + client.RemoteEndPoint.ToString());
            while (IsListening) {
                try {
                    asynchronousCommunicationUtils.ListenForMessages(client);
                } catch (SocketException) {
                    C.log.Info("Connection lost towards " + client.RemoteEndPoint.ToString());
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