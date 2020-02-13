using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;
using System.Threading;
using HamPig.Network;

namespace SocketClient
{
    class Program
    {
        private static List<string> m_CmdList = new List<string>();
        private static int m_CmdCount;
        private static bool m_IsShutdown = false;

        static void IOMain()
        {
            while (!m_IsShutdown)
            {
                string cmd = Console.ReadLine();
                lock (m_CmdList)
                {
                    m_CmdList.Add(cmd);
                    m_CmdCount++;
                }
            }
        }

        static void Main(string[] args)
        {
            ThreadStart ioStart = new ThreadStart(IOMain);
            Thread ioThread = new Thread(ioStart);
            ioThread.Start();
            Console.WriteLine("client is running...");

            ClientSocket mgr = new ClientSocket();

            while(!m_IsShutdown)
            {
                while(m_CmdCount > 0)
                {
                    string cmd = null;
                    lock (m_CmdList)
                    {
                        cmd = m_CmdList[0];
                        m_CmdList.RemoveAt(0);
                        m_CmdCount--;
                    }

                    if(cmd == "connect")
                    {
                        Console.WriteLine("begin connect server.");
                        mgr.Connect("127.0.0.1", 8888);
                    }
                    else if (cmd == "exit")
                    {
                        mgr.Close();
                        m_IsShutdown = true;
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
