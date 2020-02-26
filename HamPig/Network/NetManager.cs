using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Google.Protobuf;


namespace HamPig.Network
{
    public class NetManager
    {
        public interface IProtocListener
        {
            void Invoke(byte[] data, int offset, int size);
        }

        private static Dictionary<Int16, IProtocListener> m_ProtocMap = new Dictionary<Int16, IProtocListener>();

        private static ClientSocket m_ClientSocket;

        public static void Connect(String ip, Int32 port)
        {
            m_ClientSocket = new ClientSocket();
            m_ClientSocket.onReceive.AddListener(delegate (byte[] data)
            {
                Int16 type = LittleEndianByte.GetInt16(data, 0);
                if (m_ProtocMap.ContainsKey(type))
                {
                    m_ProtocMap[type].Invoke(data, 2, data.Length - 2);
                }
            });
            m_ClientSocket.Connect("127.0.0.1", 8888);
        }

        public static void Close()
        {
            m_ClientSocket.Close();
        }

        public static void Register(Int16 key, IProtocListener protoc)
        {
            if (m_ProtocMap.ContainsKey(key)) return;

            m_ProtocMap.Add(key, protoc);
        }

        public static void Send(Int16 key, IMessage msg)
        {
            var typeBytes = LittleEndianByte.GetBytes(key);
            var dataBytes = msg.ToByteArray();
            var sendBytes = typeBytes.Concat(dataBytes).ToArray();
            m_ClientSocket.Send(sendBytes);
        }

        public static void Update()
        {
            if (m_ClientSocket != null)
                m_ClientSocket.Tick();
        }
    }
}
