using System;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading;

namespace Shapp
{
    public class AsynchronousCommunicationUtils
    {
        public class StateObject
        {
            public Socket workSocket = null;
            public int bytesRead = 0;
            // Receive buffer. At first it will receive 4 bytes with size of the payload
            public byte[] buffer = new byte[sizeof(int)];
            public ManualResetEvent processingDone = new ManualResetEvent(false);
        }

        /// <summary>
        /// Delegate for new messages received by the socket.
        /// </summary>
        /// <param name="classInstance">received object; cast it for your favourite type</param>
        public delegate void NewMessageReceived(object classInstance, Socket client);
        public event NewMessageReceived NewMessageReceivedEvent;

        public void ListenForMessages(Socket handler)
        {
            StateObject state = new StateObject
            {
                bytesRead = 0,
                workSocket = handler
            };
            state.processingDone.Reset();
            handler.BeginReceive(state.buffer, state.bytesRead, sizeof(int), SocketFlags.None,
                new AsyncCallback(ReadPayloadSizeCallback), state);
            state.processingDone.WaitOne(ShappSettins.Default.EventWaitTime);
        }

        private void ReadPayloadSizeCallback(IAsyncResult ar)
        {
            StateObject state = (StateObject)ar.AsyncState;
            // continue the listener thread
            Socket handler = state.workSocket;
            int bytesRead = handler.EndReceive(ar);

            if (bytesRead == 0)
            {
                return;
            }
            state.bytesRead += bytesRead;
            if (state.bytesRead < sizeof(int))
            {
                handler.BeginReceive(state.buffer, state.bytesRead, sizeof(int) - state.bytesRead, 0,
                new AsyncCallback(ReadPayloadSizeCallback), state);
            }
            else
            {
                int payloadSize = BitConverter.ToInt32(state.buffer, 0);
                state.workSocket = handler;
                state.bytesRead = 0;
                state.buffer = new byte[payloadSize];
                handler.BeginReceive(state.buffer, state.bytesRead, state.buffer.Length, 0,
                    new AsyncCallback(ReadPayloadCallback), state);
            }
        }

        private void ReadPayloadCallback(IAsyncResult ar)
        {
            StateObject state = (StateObject)ar.AsyncState;
            state.processingDone.Set();
            Socket handler = state.workSocket;
            int bytesRead = handler.EndReceive(ar);

            if (bytesRead == 0)
            {
                return;
            }
            state.bytesRead += bytesRead;
            if (state.bytesRead < state.buffer.Length)
            {
                handler.BeginReceive(state.buffer, state.bytesRead, state.buffer.Length - state.bytesRead, 0,
                    new AsyncCallback(ReadPayloadCallback), state);
            }
            else
            {
                using (var stream = new MemoryStream(state.buffer))
                {
                    var formatter = new BinaryFormatter();
                    stream.Seek(0, SeekOrigin.Begin);
                    object receivedObject = formatter.Deserialize(stream);
                    Interlocked.Increment(ref reception);
                    NewMessageReceivedEvent?.Invoke(receivedObject, handler);
                }
            }
        }

        public static long reception = 0;

        public static void Send(Socket handler, object objectToSend)
        {
            var stream = new MemoryStream();
            stream.Seek(0, SeekOrigin.Begin);
            var formatter = new BinaryFormatter();
            formatter.Serialize(stream, objectToSend);
            byte[] serializedObject = stream.GetBuffer();
            byte[] messageHeader = BitConverter.GetBytes(serializedObject.Length);

            byte[] byteData = messageHeader.Concat(serializedObject).ToArray();
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
