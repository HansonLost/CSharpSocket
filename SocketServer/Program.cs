using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;
using HamPig;
using HamPig.Network;

namespace SocketServer
{
    class Program
    {
        public enum ProtocType : Int16
        {
            None,
            Login,
            LoginRes,
        }

        public class LoginListener : Singleton<LoginListener>, ServerNetManager.IProtocListener
        {
            private Action<Socket, GameProto.Login> m_Action;

            public LoginListener()
            {
                ServerNetManager.Register((Int16)ProtocType.Login, this);
            }

            public void AddListener(Action<Socket, GameProto.Login> action)
            {
                m_Action += action;
            }

            public void Invoke(Socket cfd, byte[] data, int offset, int size)
            {
                var msg = GameProto.Login.Parser.ParseFrom(data, offset, size);
                m_Action.Invoke(cfd, msg);
            }
        }


        static void Main(string[] args)
        {
            LoginListener.instance.AddListener(delegate (Socket cfd, GameProto.Login login)
            {
                Console.WriteLine(String.Format("Check Account = {0}", login.Account));
                ServerNetManager.Send(cfd, (Int16)ProtocType.Login, login);
                ServerNetManager.Send(cfd, (Int16)ProtocType.LoginRes, new GameProto.LoginRes
                {
                    IsMatch = true,
                });
            });

            ServerNetManager.Bind("127.0.0.1", 8888);

            ConsoleAsync console = new ConsoleAsync();

            bool isShutdown = false;
            while (!isShutdown)
            {
                string cmd = console.TryReadLine();
                if(cmd != null)
                {
                    if(cmd == "exit")
                    {
                        isShutdown = true;
                    }
                }

                ServerNetManager.Update();
            }
        }
    }
}
