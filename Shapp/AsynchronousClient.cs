// Asynchronous Client Socket Example
// http://msdn.microsoft.com/en-us/library/bew39x2a.aspx

using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Text;
using static Shapp.AsynchronousServer;
using static Shapp.AsynchronousCommunicationUtils;

namespace Shapp
{
    public class AsynchronousClient
    {
        // The port number for the remote device.
        private static readonly int port = ShappSettins.Default.CommunicationPort;

        // ManualResetEvent instances signal completion.
        private static ManualResetEvent connectDone =
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

        public AsynchronousClient()
        {
            asynchronousCommunicationUtils.NewMessageReceivedEvent += OnMessageReceive;
        }

        private void OnMessageReceive(object classInstance, Socket client)
        {
            NewMessageReceivedEvent?.Invoke(classInstance, client);
        }

        public void Connect(IPAddress ipAddress)
        {
            IPEndPoint remoteEP = new IPEndPoint(ipAddress, port);
            client = new Socket(AddressFamily.InterNetwork,
                SocketType.Stream, ProtocolType.Tcp);
            IsListening = true;
            client.BeginConnect(remoteEP,
                new AsyncCallback(ConnectCallback), client);
            connectDone.WaitOne();
        }

        public void Stop()
        {
            IsListening = false;
        }

        private void ConnectCallback(IAsyncResult ar)
        {
            Socket client = (Socket)ar.AsyncState;
            client.EndConnect(ar);
            Console.WriteLine("Socket connected to {0}", client.RemoteEndPoint.ToString());
            connectDone.Set();
            while (IsListening)
            {
                asynchronousCommunicationUtils.ListenForMessages(client);
            }
        }

        public void Send(object objectToSend)
        {
            AsynchronousCommunicationUtils.Send(client, objectToSend);
        }
    }
}