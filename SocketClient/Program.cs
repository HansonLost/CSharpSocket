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
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("client is running...");
            ConsoleAsync console = new ConsoleAsync();
            ClientSocket mgr = new ClientSocket();
            bool isShutdown = false;

            mgr.onReceive.AddListener(delegate (byte[] data)
            {
                string str = Encoding.Default.GetString(data);
                Console.WriteLine(str);
            });

            while(!isShutdown)
            {
                string cmd = console.TryReadLine();
                if(cmd != null)
                {
                    if (cmd == "connect")
                    {
                        Console.WriteLine("begin connect server.");
                        mgr.Connect("127.0.0.1", 8888);
                    }
                    else if (cmd == "close")
                    {
                        mgr.Close();
                    }
                    else if (cmd == "exit")
                    {
                        isShutdown = true;
                    }
                    else if (cmd == "send")
                    {
                        for (int i = 0; i < 300; i++)
                        {
                            string str = String.Format("{0} - I am message!", i + 1);
                            mgr.Send(Encoding.Default.GetBytes(str));
                        }
                    }
                    else
                    {
                        Console.WriteLine("未识别指令");
                    }
                }

                mgr.Tick();
            }
        }

    }
}
