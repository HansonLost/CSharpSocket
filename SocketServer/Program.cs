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
        static void Main(string[] args)
        {
            ServerSocket serverSocket = new ServerSocket();
            serverSocket.Run();
            serverSocket.onReceive.AddListener(delegate (Socket cfd, byte[] byteData)
            {
                //Console.WriteLine(String.Format("client : {1}", cfd.ToString(), Encoding.Default.GetString(byteData)));
                serverSocket.Send(cfd, byteData);
            });

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

                serverSocket.Tick();
            }
        }
    }
}
