using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;

namespace SocketServer
{
    public class ClientState
    {
        public Socket socket;
        public byte[] readBuffer;
    }

    public class ServerSocket
    {
        private Dictionary<Socket, ClientState> m_Clients = new Dictionary<Socket, ClientState>();

        private Socket m_Listenfd;

        public ServerSocket()
        {
            m_Listenfd = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        }

        public void Run()
        {
            IPAddress ip = IPAddress.Parse("127.0.0.1");
            IPEndPoint ipEp = new IPEndPoint(ip, 8888);
            m_Listenfd.Bind(ipEp);
            m_Listenfd.Listen(0);   // 不限制待连接数
            Console.WriteLine("服务器开启...");
            m_Listenfd.BeginAccept(AcceptCallback, m_Listenfd);
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
                if(count == 0)
                {
                    // client 请求关闭 socket
                    clientfd.Close();
                    Console.WriteLine("client close.");
                }
                else
                {
                    string str = Encoding.Default.GetString(state.readBuffer, 0, count);
                    Console.WriteLine(string.Format("receive:{0}", str));
                    clientfd.Send(Encoding.Default.GetBytes("Server Get."));
                    clientfd.BeginReceive(state.readBuffer, 0, 1024, 0, ReceiveCallback, state);
                }
            }
            catch (SocketException ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }
    }



    class Program
    {
        static void Main(string[] args)
        {
            ServerSocket serverSocket = new ServerSocket();
            serverSocket.Run();

            bool isShutdown = false;
            while (!isShutdown)
            {
                string cmd = Console.ReadLine();
                if (cmd == "exit")
                {
                    isShutdown = true;
                }
            }
        }
    }
}
