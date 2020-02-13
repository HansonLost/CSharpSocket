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

            ConsoleThread ioThread = new ConsoleThread();

            bool isShutdown = false;
            while (!isShutdown)
            {
                ioThread.Receive();
                string cmd = ioThread.GetCommand();
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
