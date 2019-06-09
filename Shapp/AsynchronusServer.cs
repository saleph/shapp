// Asynchronous Server Socket Example
// http://msdn.microsoft.com/en-us/library/fx6588te.aspx

using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace Shapp
{
    public class AsynchronousServer
    {
        private static readonly int PORT = ShappSettins.Default.CommunicationPort;
        private const int SERVER_BACKLOG_SIZE = 100;
        private ManualResetEvent connectionEstablished = new ManualResetEvent(false);
        private readonly object isListeningLock = new object();
        private bool isListening;
        public bool IsListening { get { lock (isListeningLock) { return isListening; } } set { lock (isListeningLock) { isListening = value; } } }
        private readonly Thread listener;
        private readonly AsynchronousCommunicationUtils asynchronousCommunicationUtils = new AsynchronousCommunicationUtils();

        /// <summary>
        /// Delegate for new messages received by the socket.
        /// </summary>
        /// <param name="classInstance">received object; cast it for your favourite type</param>
        public delegate void NewMessageReceived(object classInstance, Socket client);
        public event NewMessageReceived NewMessageReceivedEvent;

        public AsynchronousServer()
        {
            asynchronousCommunicationUtils.NewMessageReceivedEvent += OnMessageReceive;
            listener = new Thread(new ThreadStart(ListenForNewConnections));
        }

        private void OnMessageReceive(object classInstance, Socket client)
        {
            NewMessageReceivedEvent?.Invoke(classInstance, client);
        }

        public void Start()
        {
            if (!listener.IsAlive)
            {
                IsListening = true;
                listener.Start();
            }
        }

        public void Stop()
        {
            if (listener.IsAlive)
            {
                IsListening = false;
                listener.Join();
            }
        }

        private void ListenForNewConnections()
        {
            IPAddress ipAddress = GetLocalIPAddress();
            IPEndPoint localEndPoint = new IPEndPoint(ipAddress, PORT);

            Socket listener = new Socket(AddressFamily.InterNetwork,
                SocketType.Stream, ProtocolType.Tcp);

            listener.Bind(localEndPoint);
            listener.Listen(SERVER_BACKLOG_SIZE);
            while (IsListening)
            {
                connectionEstablished.Reset();
                listener.BeginAccept(new AsyncCallback(AcceptCallback), listener);
                connectionEstablished.WaitOne(ShappSettins.Default.EventWaitTime);
            }
        }

        private static IPAddress GetLocalIPAddress()
        {
            var host = Dns.GetHostEntry(Dns.GetHostName());
            foreach (var ip in host.AddressList)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                {
                    return ip;
                }
            }
            throw new Exception("No network adapters with an IPv4 address in the system!");
        }

        private void AcceptCallback(IAsyncResult ar)
        {
            connectionEstablished.Set();
            Socket listener = (Socket)ar.AsyncState;
            Socket handler = listener.EndAccept(ar);
            while (IsListening)
            {
                asynchronousCommunicationUtils.ListenForMessages(handler);
            }
        }

        public void Send(Socket client, object objectToSend)
        {
            AsynchronousCommunicationUtils.Send(client, objectToSend);
        }
    }
}