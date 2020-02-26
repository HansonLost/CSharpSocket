using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;
using Google.Protobuf;

namespace HamPig.Network
{
    public class ServerNetManager
    {
        public interface IProtocListener
        {
            void Invoke(Socket cfd, byte[] data, int offset, int size);
        }

        public abstract class BaseListener<T, P> : Singleton<T>, IProtocListener
            where T : class, new()
            where P : IMessage
        {
            private Action<Socket, P> m_Action;
            public BaseListener()
            {
                Register(GetProtocType(), this);
            }
            public abstract Int16 GetProtocType();
            protected abstract P ParseData(byte[] data, int offset, int size);
            public void AddListener(Action<Socket, P> action)
            {
                m_Action += action;
            }

            public void Invoke(Socket cfd, byte[] data, int offset, int size)
            {
                m_Action.Invoke(cfd, ParseData(data, offset, size));
            }
        }

        private static ServerSocket m_ServerSocket;
        private static Dictionary<Int16, IProtocListener> m_ProtocMap = new Dictionary<Int16, IProtocListener>();
        public static void Register(Int16 id, IProtocListener protoc)
        {
            if (m_ProtocMap.ContainsKey(id)) return;
            m_ProtocMap.Add(id, protoc);
        }

        public static void Bind(String ip, Int32 port)
        {
            if (m_ServerSocket != null) return;
            m_ServerSocket = new ServerSocket();
            m_ServerSocket.Bind(ip, port);
            m_ServerSocket.onReceive.AddListener(delegate (Socket cfd, byte[] data)
            {
                Int16 id = LittleEndianByte.GetInt16(data, 0);
                if (m_ProtocMap.ContainsKey(id))
                {
                    m_ProtocMap[id].Invoke(cfd, data, 2, data.Length - 2);
                }
            });
        }

        public static void Send(Socket cfd, Int16 key, IMessage msg)
        {
            if (m_ServerSocket == null) return;

            var typeBytes = LittleEndianByte.GetBytes(key);
            var dataBytes = msg.ToByteArray();
            var sendBytes = typeBytes.Concat(dataBytes).ToArray();
            m_ServerSocket.Send(cfd, sendBytes);
        }

        public static void Update()
        {
            if (m_ServerSocket != null)
                m_ServerSocket.Tick();
        }
    }
}
