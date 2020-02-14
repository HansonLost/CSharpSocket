using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HamPig.Network
{
    public class ClientSocket
    {
        private byte[] m_ReadBuffer = new byte[1024];
        private Socket m_Socket;
        private List<byte[]> m_DataList = new List<byte[]>();
        private int m_DataCount = 0;
        
        public Listener<byte[]> onReceive { get; private set; }

        public ClientSocket()
        {
            onReceive = new Listener<byte[]>();
            m_Socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        }

        public void Connect(string ip, int port)
        {
            m_Socket.BeginConnect(ip, port, ConnectCallback, m_Socket);
        }

        public void Tick()
        {
            while(m_DataCount > 0)
            {
                byte[] data = null;
                lock (m_DataList)
                {
                    data = m_DataList[0];
                    m_DataList.RemoveAt(0);
                    m_DataCount--;
                }
                onReceive.Invoke(data);
            }
        }

        public void Send(byte[] data)
        {
            Int16 len = (Int16)data.Length;
            byte[] lenBytes = BitConverter.GetBytes(len);
            byte[] sendBytes = lenBytes.Concat(data).ToArray();
            m_Socket.BeginSend(sendBytes, 0, sendBytes.Length, 0, SendCallback, m_Socket);
        }

        public void Close()
        {
            m_Socket.Close();
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
}
