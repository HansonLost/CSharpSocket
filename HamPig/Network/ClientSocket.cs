using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HamPig.Network
{
    public static partial class SocketExtension
    {
        public static IAsyncResult BeginReceive(this Socket socket, ByteArray bytes, AsyncCallback callback, object state)
        {
            Int32 offset = bytes.offset + bytes.size;
            Int32 size = bytes.GetFreeLength();
            return socket.BeginReceive(bytes.buffer, offset, size, 0, callback, state);
        }

        public static IAsyncResult BeginSend(this Socket socket, ByteArray bytes, AsyncCallback callback, object state)
        {
            return socket.BeginSend(bytes.buffer, bytes.offset, bytes.size, 0, callback, state);
        }
    }

    public sealed class ClientSocket
    {
        private SocketReadBuffer m_ReadBuffer;
        private SocketWriteBuffer m_WriteBuffer;
        private Socket m_Socket;

        public Listener<byte[]> onReceive { get; private set; }

        public ClientSocket()
        {
            m_ReadBuffer = new SocketReadBuffer();
            m_WriteBuffer = new SocketWriteBuffer();
            onReceive = new Listener<byte[]>();
            m_Socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp)
            {
                // 2个 bufferSize 都用默认的
                NoDelay = true, // 为了游戏的实时性，关闭延时发送。
                // TTL 先不调
            };
        }

        public void Connect(string ip, int port)
        {
            m_Socket.BeginConnect(ip, port, ConnectCallback, m_Socket);
        }

        public void Tick()
        {
            ByteArray data = m_ReadBuffer.GetData();
            while(data != null)
            {
                onReceive.Invoke(data.ToBytes());
                data = m_ReadBuffer.GetData();
            }
        }

        public void Send(byte[] data)
        {
            ByteArray sendBytes = m_WriteBuffer.Add(data);
            if(sendBytes != null)
            {
                m_Socket.BeginSend(sendBytes, SendCallback, m_Socket);
            }
        }

        public void Close()
        {
            m_Socket.BeginDisconnect(false, DisconnectCallback, m_Socket);
        }

        private void ConnectCallback(IAsyncResult ar)
        {
            try
            {
                Socket socket = (Socket)ar.AsyncState;
                socket.EndConnect(ar);
                Console.WriteLine("connect successfully.");
                socket.BeginReceive(m_ReadBuffer.recvBuffer, ReceiveCallback, socket);
            }
            catch (SocketException ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }

        private void ReceiveCallback(IAsyncResult ar)
        {
            try
            {
                Socket socket = (Socket)ar.AsyncState;
                if (socket != null && !socket.Connected) return; // close 之后会中止 recv 线程，有可能会调用该 callback。
                int count = socket.EndReceive(ar);
                m_ReadBuffer.Update(count);
                socket.BeginReceive(m_ReadBuffer.recvBuffer, ReceiveCallback, socket);
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
                var sendBytes = m_WriteBuffer.Update(count);
                if(sendBytes != null)
                {
                    socket.BeginSend(sendBytes, SendCallback, socket);
                }
            }
            catch (SocketException ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }

        private void DisconnectCallback(IAsyncResult ar)
        {
            Socket socket = (Socket)ar.AsyncState;
            socket.Close();
        }
    }
}
