using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace User2
{
    class AsyncUser2
    {
        //client values
        private const int port = 8081;
        //server values
        private static ManualResetEvent allDone = new ManualResetEvent(false);
        //client methods
        private static ManualResetEvent connectDone = new ManualResetEvent(false);
        private static ManualResetEvent sendDone = new ManualResetEvent(false);
        private static ManualResetEvent receiveDone = new ManualResetEvent(false);
        private static Socket socket;
        private static Socket listener;
        private static StateObject state = new StateObject();
        private static string response;
        public void Start()
        {
            string ip = "127.0.0.2";

            IPAddress ipAddress = IPAddress.Parse(ip);
            IPEndPoint endPoint = new IPEndPoint(ipAddress, port);

            socket = new Socket(ipAddress.AddressFamily,
                SocketType.Stream, ProtocolType.Tcp);

            // Подключаемся к удаленному хосту
            Connect(endPoint, socket);
            sendDone.Reset();
            Console.WriteLine("Write a message");
            string message = Console.ReadLine();
            // Отправляем данные на сервер
            Send(socket, message);
            sendDone.WaitOne();
        }

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
            client.EndSend(ar);   // lock
            // Signal that all bytes have been sent.  
            allDone.Set();
            Receive(client);
            allDone.WaitOne();
        }
        private static void Receive(Socket client)
        {
            // Create the state object.  
            StateObject state = new StateObject();

            // передача ссылки на сокет нашему state object
            state.socket = client;

            // Начать получать данные с сервера
            client.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0,
                new AsyncCallback(ReceiveCallback), state);

            // блокировка до окончании асинхронного приема данных
            receiveDone.WaitOne();
        }

        private static void ReceiveCallback(IAsyncResult ar)
        {
            // Извлекаем объект состояния и клинетский сокет из объекта асинхронного состояния
            StateObject state = (StateObject)ar.AsyncState;
            Socket client = state.socket;

            // Чтение данных с сервера
            int bytesRead = client.EndReceive(ar);
            // There might be more data, so store the data received so far.  
            state.textBuilder.Append(Encoding.ASCII.GetString(state.buffer, 0, bytesRead));
            if (bytesRead >= StateObject.BufferSize)
            {
                client.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0,
                new AsyncCallback(ReceiveCallback), state);
                receiveDone.WaitOne();
            }
            else
            {
                // Все данные пришли. Записываем ответ в response
                if (state.textBuilder.Length > 1)
                {
                    response = state.textBuilder.ToString();
                }
                Console.WriteLine("You received");
                Console.WriteLine(response);
                // Переводим в сигнальное состояние
                receiveDone.Set();
                sendDone.Reset();
                Console.WriteLine("Write a message");
                string message = Console.ReadLine();
                // Отправляем данные на сервер
                Send(socket, message);
                sendDone.WaitOne();
            }
        }
        public static void Connect(EndPoint remoteEP, Socket client)
        {
            client.BeginConnect(remoteEP, new AsyncCallback(ConnectCallback), client);

            connectDone.WaitOne();
        }

        private static void ConnectCallback(IAsyncResult ar)
        {
            // Извлекаем сокет из объекта состояния
            listener = (Socket)ar.AsyncState;

            // Ждем окончания конекта  
            listener.EndConnect(ar);

            // Переключаем устройство в сигнальное состояние
            connectDone.Set();
        }
        static void AcceptCallBack(IAsyncResult ar)
        {

            Socket listener = (Socket)ar.AsyncState;
            Socket handler = listener.EndAccept(ar);
            state.socket = handler;
            handler.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0, ReadCallBack, state);
            sendDone.Set();
        }
        static void ReadCallBack(IAsyncResult ar)
        {
            StateObject state = (StateObject)ar.AsyncState;
            Socket handler = state.socket;
            int bytesRead = handler.EndReceive(ar);
            string text = Encoding.ASCII.GetString(state.buffer, 0, bytesRead);
            Console.WriteLine("You received");
            Console.WriteLine(text);
            while (true)
            {
                Console.WriteLine("Write a message");
                string message = Console.ReadLine();
                // Отправляем данные на сервер
                Send(socket, message);
                allDone.Reset();
                listener.BeginAccept(AcceptCallBack, socket);
                allDone.WaitOne();
            }
        }
    }
}
