using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace User1
{
    class AsyncUser1
    {
        private const int port = 8081;
        private static ManualResetEvent allDone = new ManualResetEvent(false);
        private static ManualResetEvent sendDone = new ManualResetEvent(false);
        private static Socket socket;
        private static Socket listener;
        private static StateObject state = new StateObject();
        private static void Send(Socket client, String data)
        {
            // Convert the string data to byte data using ASCII encoding.  
            byte[] byteData = Encoding.ASCII.GetBytes(data);

            // Begin sending the data to the remote device.  
            client.BeginSend(byteData, 0, byteData.Length, SocketFlags.None,
                new AsyncCallback(SendCallback), client);

            sendDone.WaitOne();
        }

        private static void SendCallback(IAsyncResult ar)
        {
            // Извлекаем сокет из объекта состояния
            Socket client = (Socket)ar.AsyncState;

            // Завершаем отправление даных 
            int bytesSent = client.EndSend(ar);   // lock
            sendDone.Set();
            while (true)
            {
                allDone.Reset();
                socket.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0, ReadCallBack, state);
                allDone.WaitOne();

            }
        }

        //server methods


        public void StartListenning()
        {
            string ip = "127.0.0.1";

            IPAddress iPAddress = IPAddress.Parse(ip);
            IPEndPoint endPoint = new IPEndPoint(iPAddress, port);
            listener = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            listener.Bind(endPoint);
            listener.Listen(10);
            allDone.Reset();
            while (true)
            {
                allDone.Reset();
                listener.BeginAccept(AcceptCallBack, listener);
                allDone.WaitOne();
            }
        }
        static void ReadCallBack(IAsyncResult ar)
        {
            StateObject state = (StateObject)ar.AsyncState;
            Socket handler = state.socket;
            int bytesRead = handler.EndReceive(ar);
            string text = Encoding.ASCII.GetString(state.buffer, 0, bytesRead);
            Console.WriteLine("You received");
            Console.WriteLine(text);
            Console.WriteLine("Write a message");
            string message = Console.ReadLine();
            Send(state.socket, message);

        }
        static void AcceptCallBack(IAsyncResult ar)
        {
            Console.WriteLine("Start");

            Socket listener = (Socket)ar.AsyncState;
            socket = listener.EndAccept(ar);

            state.socket = socket;
            allDone.Set();
            socket.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0, ReadCallBack, state);

        }
    }
}
