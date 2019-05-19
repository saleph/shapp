// Asynchronous Server Socket Example
// http://msdn.microsoft.com/en-us/library/fx6588te.aspx

using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading;
using System.Linq;

namespace Shapp
{
    public class AsynchronousSocketListener
    {
        public class StateObject
        {
            public Socket workSocket = null;
            public int bytesRead = 0;
            // Receive buffer. At first it will receive 4 bytes with size of the payload
            public byte[] buffer = new byte[sizeof(int)];
        }
        private const int PORT = 11000;
        private const int SERVER_BACKLOG_SIZE = 100;
        private ManualResetEvent connectionEstablished = new ManualResetEvent(false);
        private readonly Thread listener;

        /// <summary>
        /// Delegate for new messages received by the server.
        /// </summary>
        /// <param name="classInstance">received object; cast it for your favourite type</param>
        public delegate void NewMessageReceived(object classInstance, Socket client);
        public event NewMessageReceived NewMessageReceivedEvent;

        public AsynchronousSocketListener()
        {
            listener = new Thread(new ThreadStart(StartListening));
            listener.Start();
        }

        public void StartListening()
        {
            IPHostEntry ipHostInfo = Dns.GetHostEntry(Dns.GetHostName());
            IPAddress ipAddress = ipHostInfo.AddressList[0];
            IPEndPoint localEndPoint = new IPEndPoint(ipAddress, PORT);

            Socket listener = new Socket(AddressFamily.InterNetwork,
                SocketType.Stream, ProtocolType.Tcp);
            
            listener.Bind(localEndPoint);
            listener.Listen(SERVER_BACKLOG_SIZE);
            while (true)
            {
                connectionEstablished.Reset();
                listener.BeginAccept(new AsyncCallback(AcceptCallback), listener);
                connectionEstablished.WaitOne();
            }
        }

        public void AcceptCallback(IAsyncResult ar)
        {
            connectionEstablished.Set();
            Socket listener = (Socket)ar.AsyncState;
            Socket handler = listener.EndAccept(ar);
            StateObject state = new StateObject
            {
                workSocket = handler
            };
            handler.BeginReceive(state.buffer, 0, sizeof(int), SocketFlags.None,
                new AsyncCallback(ReadPayloadSizeCallback), state);
        }

        public void ReadPayloadSizeCallback(IAsyncResult ar)
        {
            StateObject state = (StateObject)ar.AsyncState;
            Socket handler = state.workSocket;
            int bytesRead = handler.EndReceive(ar);

            if (bytesRead == 0)
            {
                return;
            }
            state.bytesRead += bytesRead;
            if (state.bytesRead < sizeof(int))
            {
                handler.BeginReceive(state.buffer, 0, sizeof(int) - state.bytesRead, 0,
                new AsyncCallback(ReadPayloadSizeCallback), state);
            }
            else
            { 
                int payloadSize = BitConverter.ToInt32(state.buffer, 0);
                StateObject newState = new StateObject
                {
                    workSocket = handler,
                    bytesRead = 0,
                    buffer = new byte[payloadSize]
                };
                handler.BeginReceive(newState.buffer, 0, newState.buffer.Length, 0,
                 new AsyncCallback(ReadPayloadCallback), newState);
            }
        }

        private void ReadPayloadCallback(IAsyncResult ar)
        {
            StateObject state = (StateObject)ar.AsyncState;
            Socket handler = state.workSocket;
            int bytesRead = handler.EndReceive(ar);

            if (bytesRead == 0)
            {
                return;
            }
            state.bytesRead += bytesRead;
            if (state.bytesRead < state.buffer.Length)
            {
                handler.BeginReceive(state.buffer, 0, sizeof(int) - state.bytesRead, 0,
                    new AsyncCallback(ReadPayloadSizeCallback), state);
            }
            else
            {
                using (var stream = new MemoryStream(state.buffer))
                {
                    var formatter = new BinaryFormatter();
                    stream.Seek(0, SeekOrigin.Begin);
                    object receivedObject = formatter.Deserialize(stream);
                    NewMessageReceivedEvent?.Invoke(receivedObject, handler);
                }
            }
        }

        public static void Send(Socket handler, object objectToSend)
        {
            var stream = new MemoryStream();
            stream.Seek(0, SeekOrigin.Begin);
            var formatter = new BinaryFormatter();
            formatter.Serialize(stream, objectToSend);
            byte[] serializedObject = stream.GetBuffer();
            byte[] messageHeader = BitConverter.GetBytes(serializedObject.Length);

            // Convert the string data to byte data using ASCII encoding.
            byte[] byteData = messageHeader.Concat(serializedObject).ToArray();

            // Begin sending the data to the remote device.
            handler.BeginSend(byteData, 0, byteData.Length, 0,
                new AsyncCallback(SendCallback), handler);
        }

        private static void SendCallback(IAsyncResult ar)
        {
            Socket handler = (Socket)ar.AsyncState;
            handler.EndSend(ar);
        }
    }
}