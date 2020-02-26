using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;
using System.Threading;
using HamPig;
using HamPig.Network;

namespace SocketClient
{
    public enum ProtocType : Int16
    {
        None,
        Login,
        LoginRes,
    }

    public class LoginListener : Singleton<LoginListener>, NetManager.IProtocListener
    {
        private Action<GameProto.Login> m_Action;

        public LoginListener()
        {
            NetManager.Register((Int16)ProtocType.Login, this);
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

    public class LoginResListener : Singleton<LoginResListener>, NetManager.IProtocListener
    {
        private Action<GameProto.LoginRes> m_Action;

        public LoginResListener()
        {
            NetManager.Register((Int16)ProtocType.LoginRes, this);
        }

        public void AddListener(Action<GameProto.LoginRes> action)
        {
            m_Action += action;
        }

        public void Invoke(byte[] data, int offset, int size)
        {
            var msg = GameProto.LoginRes.Parser.ParseFrom(data, offset, size);
            m_Action.Invoke(msg);
        }
    }

    class Program
    {
        static bool isShutdown = false;

        static void Main(string[] args)
        {
            LoginResListener.instance.AddListener(delegate (GameProto.LoginRes loginRes)
            {
                if (loginRes.IsMatch)
                    Console.WriteLine("Login successfully.");
                else
                    Console.WriteLine("Login failed");
            });

            Console.WriteLine("client is running...");
            ConsoleAsync console = new ConsoleAsync();

            while(!isShutdown)
            {
                string cmd = console.TryReadLine();
                if(cmd != null)
                {
                    ParseCommand(cmd);
                }

                NetManager.Update();
            }
        }
        static void ParseCommand(String cmd)
        {
            if (cmd == "connect")
            {
                NetManager.Connect("127.0.0.1", 8888);
            }
            else if (cmd == "close")
            {
                NetManager.Close();
            }
            else if (cmd == "exit")
            {
                isShutdown = true;
            }
            else if (cmd == "login")
            {
                GameProto.Login hanson = new GameProto.Login
                {
                    Account = "HansonLost",
                    Password = "321321",
                };

                NetManager.Send((Int16)ProtocType.Login, hanson);
            }
            else
            {
                Console.WriteLine("未识别指令");
            }
        }
    }
}
