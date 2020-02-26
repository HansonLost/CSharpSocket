using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Google.Protobuf;


namespace HamPig.Network
{
    
    public interface IProtocListener
    {
        void Invoke(byte[] data, int offset, int size);
        /* AddListener 由派生类自行定义 */
    }

    public interface IProtocData
    {
        byte[] ToBytes();
    }

    public enum ProtocType : Int16
    {
        None,
        Login,
    }

    // client net
    public class NetManager
    {
        private static Dictionary<ProtocType, IProtocListener> m_ProtocMap = new Dictionary<ProtocType, IProtocListener>();

        private static ClientSocket m_ClientSocket;

        public static void Connect(String ip, Int32 port)
        {
            m_ClientSocket = new ClientSocket();
            m_ClientSocket.onReceive.AddListener(delegate (byte[] data)
            {
                ProtocType type = (ProtocType)LittleEndianByte.GetInt16(data, 0);
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

        public static void Register(ProtocType type, IProtocListener protoc)
        {
            m_ProtocMap.Add(type, protoc);
        }

        public static void Send(ProtocType type, IMessage msg)
        {
            var typeBytes = LittleEndianByte.GetBytes((Int16)type);
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

    //public class BaseProtocListener<T> : Singleton<T>, IProtocListener 
    //    where T : class, new()
    //{
    //    public BaseProtocListener()
    //    {
    //        ProtocType type = GetProtocType();
    //        NetManager.Register(type, this);
    //    }
    //    public abstract virtual ProtocType GetProtocType();
    //    public virtual void Invoke(byte[] data, int offset, int size) { return; }
    //}

    /* 外部代码 */


    public class LoginListener : Singleton<LoginListener>, IProtocListener
    {
        private Action<GameProto.Login> m_Action;

        public LoginListener()
        {
            NetManager.Register(ProtocType.Login, this);
        }

        public void AddListener(Action<GameProto.Login> action)
        {
            m_Action += action;
        }

        public void Invoke(byte[] data, int offset, int size)
        {
            var msg = GameProto.Login.Parser.ParseFrom(data, offset, size);
            m_Action.Invoke(msg);
        }
    }
}
