using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HamPig.Network
{
    public interface IProtocListener
    {
        ProtocType GetProtocType();
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
        Person,
    }

    // client net
    public class NetManager
    {
        private static Dictionary<ProtocType, IProtocListener> m_ProtocMap = new Dictionary<ProtocType, IProtocListener>();

        private static ClientSocket m_ClientSocket;

        public static void Connect()
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

        public static void Register(ProtocType type, IProtocListener protoc)
        {
            m_ProtocMap.Add(type, protoc);
        }

        public static void Send(IProtocData protoc)
        {
            m_ClientSocket.Send(protoc.ToBytes());
        }
    }

    public class BaseProtocListener<T> : Singleton<T>, IProtocListener 
        where T : class, new()
    {
        public BaseProtocListener()
        {
            ProtocType type = GetProtocType();
            NetManager.Register(type, this);
        }
        public virtual ProtocType GetProtocType() => ProtocType.None;
        public virtual void Invoke(byte[] data, int offset, int size) { return; }
    }

    /* 外部代码 */

    public class Person : BaseProtocListener<Person>
    {
        private Action<String> m_Action;

        public override ProtocType GetProtocType() => ProtocType.Person;
        public void AddListener(Action<String> action) => m_Action += action;
        public override void Invoke(byte[] data, int offset, int size)
        {
            byte[] msg = new byte[size];
            Array.Copy(data, offset, msg, 0, size);
            String name = Encoding.Default.GetString(msg);
            m_Action.Invoke(name);
        }
    }

    public class PersonData
    {
        public string name;
    }

    public class Run
    {
        public void Main()
        {
            Person.instance.AddListener(delegate (String name)
            {
                Console.WriteLine(name);
            });
        }
    }
}
