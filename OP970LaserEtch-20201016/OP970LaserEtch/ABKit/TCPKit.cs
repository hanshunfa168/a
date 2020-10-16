using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using System.Threading;
using System.Net;
using System.Diagnostics;

namespace TopaKit
{
    public class StateObject
    {
        public const int BufferSize = 4096;
        public const int RecvSize = 1024;
        public Socket workSocket = null;
        public byte[] buffer = new byte[BufferSize];
        public int bufferLen = 0;
        public int oprFlag = -1;
        public int idxFlag = -1;
        //public StringBuilder sb = new StringBuilder();
    }
    /// <summary>
    /// 
    /// </summary>
    
    public static class TCPServer
    {
        public delegate void OnReceiveCallback(StateObject state);
        public static OnReceiveCallback ReceiveCallbackEvent { get; set;}
        private static Thread threadAccept = null;
        private static  Socket server = null;
        public static ManualResetEvent acceptDone = new ManualResetEvent(false);

        public static void Start()
        {
            IPAddress ipAddress = IPAddress.Any;
            IPEndPoint localEndPoint = new IPEndPoint(ipAddress, 2000);
            server = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            try
            {
                server.Bind(localEndPoint);
                server.Listen(10);
                //ReceiveCallbackEvent += ReceiveCallback;
                threadAccept = new Thread(new ThreadStart(AcceptLoop));
                threadAccept.Start();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }
        public static bool Close()
        {
            try
            {
                if (threadAccept != null)
                {
                    threadAccept.Abort();
                }
                if (server != null)
                {
                    server.Shutdown(SocketShutdown.Both);
                    server.Close();
                }
                return true;
            }
            catch
            {
                return false;
            }
        }
        public static void AcceptLoop()
        {
            while (true)
            {
                acceptDone.Reset();
                server.BeginAccept(new AsyncCallback(AcceptCallback), server);
                acceptDone.WaitOne();
            }
        }
        public static void AcceptCallback(IAsyncResult ar)
        {
            Socket listener = (Socket)ar.AsyncState;
            Socket socket = listener.EndAccept(ar);
            StateObject state = new StateObject();
            state.workSocket = socket;
            state.bufferLen = -1;  // 客户端连接
            ReceiveCallbackEvent(state);
            socket.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0, new AsyncCallback(ReadCallback), state);
            acceptDone.Set();
        }
        public static void ReadCallback(IAsyncResult ar)
        {
            String content = String.Empty;
            StateObject state = (StateObject)ar.AsyncState;
            Socket socket = state.workSocket;
            int bytesRead = socket.EndReceive(ar);
            if (bytesRead > 0)
            {
                state.bufferLen = bytesRead;
                ReceiveCallbackEvent(state);
                socket.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0, new AsyncCallback(ReadCallback), state);
            }
            else
            {
                state.bufferLen = -2; // 客户端断开连接
                ReceiveCallbackEvent(state);
            }
        }
        public static void Send(StateObject state)
        {
            state.workSocket.BeginSend(state.buffer, 0, state.bufferLen, 0, new AsyncCallback(SendCallback), state.workSocket);
        }

        private static void SendCallback(IAsyncResult ar)
        {
            try
            {
                Socket socket = (Socket)ar.AsyncState;
                int bytesSent = socket.EndSend(ar);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }
        //private static void ReceiveCallback(StateObject state)
        //{ 
        //    //
        //}
    }
    /// <summary>
    /// 
    /// </summary>
    public class TCPClient
    {
        public delegate void OnReceiveCallback(StateObject state);
        public OnReceiveCallback ReceiveCallbackEvent { get; set; }

        private Socket client = null;

        private ManualResetEvent sendDone = new ManualResetEvent(true);
        private ManualResetEvent receiveDone = new ManualResetEvent(false);
        //private ManualResetEvent NotifyEvent = new ManualResetEvent(false);

        public bool Open(string ip, int port)
        {
            IPAddress ipAddress = IPAddress.Parse(ip);
            IPEndPoint remoteEP = new IPEndPoint(ipAddress, port);
            client = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            try
            {
                client.Connect(remoteEP);
                StateObject state = new StateObject();
                state.workSocket = client;
                state.bufferLen = -1; // 客户端连接成功
                ReceiveCallbackEvent(state); // 
                client.BeginReceive(state.buffer, 0, StateObject.RecvSize, 0, new AsyncCallback(ReceiveCallback), state);
                return true;
            }
            catch
            {
                return false;
            }
        }
        public bool Close()
        {
            try
            {
                if (client != null)
                {
                    client.Shutdown(SocketShutdown.Both);
                    //client.Disconnect(true);
                    client.Close();
                }
                return true;
            }
            catch
            {
                return false;
            }
        }
        public bool SendWaitOne(StateObject sendIO, int ms)
        {
            receiveDone.Reset();
            Send(sendIO, ms);
            return receiveDone.WaitOne(ms);
        }
        public bool Send(StateObject state, int ms)
        {
            try
            {
                sendDone.Reset();
                state.workSocket = client;
                client.BeginSend(state.buffer, 0, state.bufferLen, 0, new AsyncCallback(SendCallback), state);
                return sendDone.WaitOne(ms);
            }
            catch 
            {
                return false;
            }
        }

        private void ReceiveCallback(IAsyncResult ar)
        {
            try
            {
                StateObject state = (StateObject)ar.AsyncState;
                Socket client = state.workSocket;
                int bytesRead = client.EndReceive(ar);
                if (bytesRead > 0)
                {
                    state.bufferLen = bytesRead;
                    ReceiveCallbackEvent(state);
                    //Array.Clear(state.buffer, 0, bytesRead);
                    receiveDone.Set();
                    client.BeginReceive(state.buffer, 0, StateObject.RecvSize, 0, new AsyncCallback(ReceiveCallback), state);        
                }
                else if (bytesRead == 0)
                {
                    state.bufferLen = -2; // 服务器关闭
                    ReceiveCallbackEvent(state);
                }
            }
            catch 
            {
            }
        }

        private void SendCallback(IAsyncResult ar)
        {
            try
            {
                //Socket client = (Socket)ar.AsyncState;
                StateObject state = (StateObject)ar.AsyncState;
                int bytesSent = state.workSocket.EndSend(ar);
                state.bufferLen -= bytesSent;
                if (state.bufferLen <= 0)
                    sendDone.Set();
            }
            catch 
            {
            }
        }
    }
}