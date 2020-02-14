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
        //private static List<string> m_CmdList = new List<string>();
        //private static int m_CmdCount;
        //private static bool m_IsShutdown = false;

        //static void IOMain()
        //{
        //    while (!m_IsShutdown)
        //    {
        //        string cmd = Console.ReadLine();
        //        lock (m_CmdList)
        //        {
        //            m_CmdList.Add(cmd);
        //            m_CmdCount++;
        //        }
        //    }
        //}

        static void Main(string[] args)
        {
            Console.WriteLine("client is running...");
            ConsoleAsync console = new ConsoleAsync();
            //ThreadStart ioStart = new ThreadStart(IOMain);
            //Thread ioThread = new Thread(ioStart);
            //ioThread.Start();
            ClientSocket mgr = new ClientSocket();
            bool isShutdown = false;

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
                    else if (cmd == "exit")
                    {
                        mgr.Close();
                        isShutdown = true;
                    }
                    else if (cmd == "send")
                    {
                        string str = "say hello.";
                        mgr.Send(Encoding.Default.GetBytes(str));
                    }
                }

                byte[] data = mgr.PopData();
                while (data != null)
                {
                    string str = Encoding.Default.GetString(data);
                    Console.WriteLine(str);
                    data = mgr.PopData();
                }
            }
        }
    }
}
