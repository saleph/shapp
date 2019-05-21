﻿using System;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading;

namespace Shapp
{

    internal class AsynchronousCommunicationUtils
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
                workSocket = handler
            };
            state.processingDone.Reset();
            handler.BeginReceive(state.buffer, 0, sizeof(int), SocketFlags.None,
                new AsyncCallback(ReadPayloadSizeCallback), state);
            state.processingDone.WaitOne(ShappSettins.Default.EventWaitTime);
        }

        private void ReadPayloadSizeCallback(IAsyncResult ar)
        {
            StateObject state = (StateObject)ar.AsyncState;
            // continue the listener thread
            state.processingDone.Set();
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
            Console.WriteLine("bytes read: {0}, state.bytesRead: {1}", bytesRead, state.bytesRead);
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
