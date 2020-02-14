using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;

namespace HamPig.Network
{
    public class ServerSocket
    {
        private class ClientState
        {
            public Socket socket;
            public byte[] readBuffer;
        }

        private class Data
        {
            public Socket clientfd;
            public byte[] byteData;
        }

        private Dictionary<Socket, ClientState> m_Clients = new Dictionary<Socket, ClientState>();
        private Socket m_Listenfd;

        private List<Data> m_DataList = new List<Data>();
        private int m_DataCount = 0;

        public Listener<Socket, byte[]> onReceive { get; private set; }

        public ServerSocket()
        {
            onReceive = new Listener<Socket, byte[]>();
            m_Listenfd = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        }

        public void Run()
        {
            Console.WriteLine("Server running...");
            IPAddress ip = IPAddress.Parse("127.0.0.1");
            IPEndPoint ipEp = new IPEndPoint(ip, 8888);
            m_Listenfd.Bind(ipEp);
            m_Listenfd.Listen(0);   // 不限制待连接数
            m_Listenfd.BeginAccept(AcceptCallback, m_Listenfd);
        }

        public void Tick()
        {
            while (m_DataCount > 0)
            {
                Data msg = null;
                lock (m_DataList)
                {
                    msg = m_DataList[0];
                    m_DataList.RemoveAt(0);
                    m_DataCount--;
                }
                onReceive.Invoke(msg.clientfd, msg.byteData);
            }
        }

        private void AcceptCallback(IAsyncResult ar)
        {
            try
            {
                Console.WriteLine("accept client.");
                Socket listenfd = (Socket)ar.AsyncState;
                Socket clientfd = listenfd.EndAccept(ar);
                ClientState state = new ClientState
                {
                    socket = clientfd,
                    readBuffer = new byte[1024],
                };
                m_Clients.Add(clientfd, state);
                clientfd.BeginReceive(state.readBuffer, 0, 1024, 0, ReceiveCallback, state);
                listenfd.BeginAccept(AcceptCallback, listenfd);
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
                ClientState state = (ClientState)ar.AsyncState;
                Socket clientfd = state.socket;
                int count = clientfd.EndReceive(ar);
                if (count == 0)
                {
                    // client 请求关闭 socket
                    Console.WriteLine("client close.");
                    clientfd.Close();
                }
                else
                {
                    lock (m_DataList)
                    {
                        byte[] byteData = new byte[count];
                        Array.Copy(state.readBuffer, 0, byteData, 0, count);
                        m_DataList.Add(new Data
                        {
                            clientfd = clientfd,
                            byteData = byteData,
                        });
                        m_DataCount++;
                    }
                    clientfd.BeginReceive(state.readBuffer, 0, 1024, 0, ReceiveCallback, state);
                }
            }
            catch (SocketException ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }
    }
}
