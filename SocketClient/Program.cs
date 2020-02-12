using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;

namespace SocketClient
{
    public class ClientSocketManager
    {
        private byte[] m_RecieveBuffer = new byte[1024];

        private Socket m_Socket;
        public ClientSocketManager()
        {
            m_Socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        }
        public void Avoke()
        {
            m_Socket.BeginConnect("127.0.0.0", 8888, ConnectCallback, m_Socket);
        }
        public void Tick() { }

        public void Send(byte[] data)
        {
            m_Socket.BeginSend(data, 0, data.Length, 0, SendCallback, m_Socket);
        }

        private void ConnectCallback(IAsyncResult ar)
        {
            try
            {
                Console.WriteLine("call back connect.");
                Socket socket = (Socket)ar.AsyncState;
                socket.EndConnect(ar);
                socket.BeginReceive(m_RecieveBuffer, 0, 1024, 0, RecieveCallback, socket);
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

                string str = System.Text.Encoding.Default.GetString(m_RecieveBuffer, 0, count);
                Console.WriteLine(str);
                socket.BeginReceive(m_RecieveBuffer, 0, 1024, 0, RecieveCallback, socket);
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

            }
        }
    }

    class Program
    {

        static void Main(string[] args)
        {
            ClientSocketManager mgr = new ClientSocketManager();
            Console.WriteLine("begin connect server.");
            mgr.Avoke();
            Console.Read();
        }
    }
}
