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
        static int count = 0;

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
                        count++;
                        string str = String.Format(@"{0}-C#是微软公司发布的一种面向对象的、运行于.NET Framework和.NET Core(完全开源，跨平台)之上的高级程序设计语言。并定于在微软职业开发者论坛(PDC)上登台亮相。C#是微软公司研究员Anders Hejlsberg的最新成果。C#看起来与Java有着惊人的相似；它包括了诸如单一继承、接口、与Java几乎同样的语法和编译成中间代码再运行的过程。但是C#与Java有着明显的不同，它借鉴了Delphi的一个特点，与COM（组件对象模型）是直接集成的，而且它是微软公司 .NET windows网络框架的主角。C#是一种安全的、稳定的、简单的、优雅的，由C和C++衍生出来的面向对象的编程语言。它在继承C和C++强大功能的同时去掉了一些它们的复杂特性（例如没有宏以及不允许多重继承）。C#综合了VB简单的可视化操作和C++的高运行效率，以其强大的操作能力、优雅的语法风格、创新的语言特性和便捷的面向组件编程的支持成为.NET开发的首选语言。 [1] C#是面向对象的编程语言。它使得程序员可以快速地编写各种基于MICROSOFT .NET平台的应用程序，MICROSOFT .NET提供了一系列的工具和服务来最大程度地开发利用计算与通讯领域。C#使得C++程序员可以高效的开发程序，且因可调用由 C/C++ 编写的本机原生函数，而绝不损失C/C++原有的强大的功能。因为这种继承关系，C#与C/C++具有极大的相似性，熟悉类似语言的开发者可以很快的转向C#。 [2]", count);
                        mgr.Send(Encoding.Default.GetBytes(str));
                        //mgr.Send(Encoding.Default.GetBytes("how old are you."));
                        //mgr.Send(Encoding.Default.GetBytes("see you."));
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
