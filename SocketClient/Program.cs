using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;
using System.Threading;

namespace SocketClient
{
    public class ClientSocket
    {
        private byte[] m_ReadBuffer = new byte[1024];
        private Socket m_Socket;
        private List<byte[]> m_DataList = new List<byte[]>();

        public ClientSocket()
        {
            m_Socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        }

        public void Connect(string ip, int port)
        {
            m_Socket.BeginConnect(ip, port, ConnectCallback, m_Socket);
        }

        public void Tick() { }

        public void Send(byte[] data)
        {
            m_Socket.BeginSend(data, 0, data.Length, 0, SendCallback, m_Socket);
        }

        public void Close()
        {
            m_Socket.Close();
        }

        public byte[] PopData()
        {
            if(m_DataList.Count <= 0)
            {
                return null;
            }

            byte[] data = null;
            lock (m_DataList)
            {
                data = m_DataList[0];
                m_DataList.RemoveAt(0);
            }
            return data;
        }

        private void ConnectCallback(IAsyncResult ar)
        {
            try
            {
                Socket socket = (Socket)ar.AsyncState;
                socket.EndConnect(ar);
                Console.WriteLine("connect successfully.");
                socket.BeginReceive(m_ReadBuffer, 0, 1024, 0, RecieveCallback, socket);
            }
            catch (SocketException ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }

        private void RecieveCallback(IAsyncResult ar)
        {
            try
            {
                Socket socket = (Socket)ar.AsyncState;
                int count = socket.EndReceive(ar);

                byte[] data = new byte[count];
                Array.Copy(m_ReadBuffer, 0, data, 0, count);
                lock (m_DataList)
                {
                    m_DataList.Add(data);
                }

                socket.BeginReceive(m_ReadBuffer, 0, 1024, 0, RecieveCallback, socket);
            }
            catch (SocketException ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }

        private void SendCallback(IAsyncResult ar)
        {
            try
            {
                Socket socket = (Socket)ar.AsyncState;
                int count = socket.EndSend(ar); // 只是把数据成功放到 send buffer。
            }
            catch (SocketException ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }
    }

    class Program
    {
        private static List<string> m_CmdList = new List<string>();
        private static int m_CmdCount;
        private static bool m_IsShutdown = false;

        static void IOMain()
        {
            while (!m_IsShutdown)
            {
                string cmd = Console.ReadLine();
                lock (m_CmdList)
                {
                    m_CmdList.Add(cmd);
                    m_CmdCount++;
                }
            }
        }

        static void Main(string[] args)
        {
            ThreadStart ioStart = new ThreadStart(IOMain);
            Thread ioThread = new Thread(ioStart);
            ioThread.Start();
            Console.WriteLine("client is running...");

            ClientSocket mgr = new ClientSocket();

            while(!m_IsShutdown)
            {
                while(m_CmdCount > 0)
                {
                    string cmd = null;
                    lock (m_CmdList)
                    {
                        cmd = m_CmdList[0];
                        m_CmdList.RemoveAt(0);
                        m_CmdCount--;
                    }

                    if(cmd == "connect")
                    {
                        Console.WriteLine("begin connect server.");
                        mgr.Connect("127.0.0.1", 8888);
                    }
                    else if (cmd == "exit")
                    {
                        mgr.Close();
                        Thread.Sleep(2000);
                        m_IsShutdown = true;
                    }
                    else if (cmd == "send")
                    {
                        string str = "say hello.";
                        mgr.Send(Encoding.Default.GetBytes(str));
                    }
                }

                byte[] data = mgr.PopData();
                while (data != null)
                {
                    string str = Encoding.Default.GetString(data);
                    Console.WriteLine(str);
                    data = mgr.PopData();
                }
            }
        }
    }
}
