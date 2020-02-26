using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;
using System.Threading;
using HamPig;
using HamPig.Network;
using Google.Protobuf;
using GameProto;

namespace SocketClient
{
    public enum ProtocType : Int16
    {
        None,
        Login,
        LoginRes,
    }

    

    public sealed class LoginListener : NetManager.BaseListener<LoginListener, GameProto.Login>
    {
        public override short GetProtocType() => (Int16)ProtocType.Login;
        protected override Login ParseData(byte[] data, int offset, int size) => Login.Parser.ParseFrom(data, offset, size);
    }

    public sealed class LoginResListener : NetManager.BaseListener<LoginResListener, GameProto.LoginRes>
    {
        public override short GetProtocType() => (Int16)ProtocType.LoginRes;
        protected override LoginRes ParseData(byte[] data, int offset, int size) => LoginRes.Parser.ParseFrom(data, offset, size);
    }

    class Program
    {
        static bool isShutdown = false;

        static void Main(string[] args)
        {
            LoginListener.instance.AddListener(delegate (GameProto.Login msg)
            {
                Console.WriteLine(String.Format("account = {0}", msg.Account));
                Console.WriteLine(String.Format("password = {0}", msg.Password));
            });

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
                NetManager.Send((Int16)ProtocType.Login, new GameProto.Login
                {
                    Account = "HansonLost",
                    Password = "321321",
                });
            }
            else
            {
                Console.WriteLine("未识别指令");
            }
        }
    }
}
