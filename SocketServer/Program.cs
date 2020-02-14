using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
            }
        }
    }
}
