﻿using System;
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
            public SocketReadBuffer readBuffer;
        }

        private class Data
        {
            public Socket clientfd;
            public byte[] byteData;
        }

        private Dictionary<Socket, ClientState> m_OnlineClients = new Dictionary<Socket, ClientState>();
        private Socket m_Listenfd;

        private List<Data> m_DataList = new List<Data>();
        private int m_DataCount = 0;

        public Listener<Socket, byte[]> onReceive { get; private set; }

        public ServerSocket()
        {
            onReceive = new Listener<Socket, byte[]>();
            m_Listenfd = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            //m_Listenfd.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true); 先不弄端口复用
        }

        public void Bind(String ip, Int32 port)
        {
            Console.WriteLine("Server running...");
            IPAddress addr = IPAddress.Parse(ip);
            IPEndPoint ipEp = new IPEndPoint(addr, port);
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

        public void Send(Socket cfd, byte[] data)
        {
            if (!m_OnlineClients.ContainsKey(cfd)) return;
            Int16 len = (Int16)data.Length;
            byte[] lenBytes = LittleEndianByte.GetBytes(len);/* BitConverter.GetBytes(len);*/
            byte[] sendBytes = lenBytes.Concat(data).ToArray();
            cfd.BeginSend(sendBytes, 0, sendBytes.Length, 0, SendCallback, cfd);
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
                    readBuffer = new SocketReadBuffer(),
                };
                m_OnlineClients.Add(clientfd, state);
                clientfd.BeginReceive(state.readBuffer.recvBuffer, ReceiveCallback, state);
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
                Socket cfd = state.socket;
                int count = cfd.EndReceive(ar);
                if (count == 0)
                {
                    // client 请求关闭 socket
                    Console.WriteLine("client close.");
                    m_OnlineClients.Remove(cfd);
                    cfd.Close();
                }
                else
                {
                    state.readBuffer.Update(count);

                    ByteArray data = state.readBuffer.GetData();
                    while (data != null)
                    {
                        lock (m_DataList)
                        {
                            
                            m_DataList.Add(new Data
                            {
                                clientfd = cfd,
                                byteData = data.ToBytes(),
                            });
                            m_DataCount++;
                        }
                        data = state.readBuffer.GetData();
                    }

                    cfd.BeginReceive(state.readBuffer.recvBuffer, ReceiveCallback, state);
                }
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
