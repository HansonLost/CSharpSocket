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
        private SocketReadBuffer m_ReadBuffer;
        private SocketSendBuffer m_WriteBuffer;
        private Socket m_Socket;
        private List<byte[]> m_DataList = new List<byte[]>();
        private int m_DataCount = 0;
        
        public Listener<byte[]> onReceive { get; private set; }

        public ClientSocket()
        {
            m_ReadBuffer = new SocketReadBuffer();
            m_WriteBuffer = new SocketSendBuffer();
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
            byte[] sendBytes = m_WriteBuffer.Add(data);
            if(sendBytes != null)
            {
                int offset = m_WriteBuffer.offset;
                int size = m_WriteBuffer.size;
                m_Socket.BeginSend(sendBytes, offset, size, 0, SendCallback, m_Socket);
            }
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
                socket.BeginReceive(m_ReadBuffer.buffer, 0, m_ReadBuffer.capacity, 0, RecieveCallback, socket);
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

                m_ReadBuffer.Receive(count);
                byte[] data = m_ReadBuffer.GetData();
                while (data != null)
                {
                    lock (m_DataList)
                    {
                        m_DataList.Add(data);
                        m_DataCount++;
                    }
                    data = m_ReadBuffer.GetData();
                }
                m_ReadBuffer.Refresh();
                int offset = m_ReadBuffer.offset + m_ReadBuffer.size;
                int size = m_ReadBuffer.capacity - offset;
                socket.BeginReceive(m_ReadBuffer.buffer, offset, size, 0, RecieveCallback, socket);
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
                Console.WriteLine(String.Format("count={0}", count));
                byte[] sendBytes = m_WriteBuffer.Update(count);
                if(sendBytes != null)
                {
                    int offset = m_WriteBuffer.offset;
                    int size = m_WriteBuffer.size;
                    socket.BeginSend(sendBytes, offset, size, 0, SendCallback, socket);
                }
            }
            catch (SocketException ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }
    }
}
