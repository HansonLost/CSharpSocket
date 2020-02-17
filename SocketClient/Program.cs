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
                    else if (cmd == "mulSend")
                    {
                        for (int i = 0; i < 300; i++)
                        {
                            string str = String.Format("{0} - I am message!", i + 1);
                            mgr.Send(Encoding.Default.GetBytes(str));
                        }
                    }
                    else if (cmd == "longSend")
                    {
                        string str = @"
其中，Vertex Compression的实现是将顶点channel的数据格式format设置为16bit，因此可以节约运行时的内存使用（float->half)。
Optimize Mesh Data则主要用来剔除不需要的channel，即剔除额外的数据。因为与压缩无关，本文先暂时不讨论这个选项。
但是，Mesh Compression是使用压缩算法，将mesh数据进行压缩，结果是会减少占用硬盘的空间，但是在runtime的时候会被解压为原始精度的数据。
和Runtime时内存关系较大的是Vertex Compression的实现。
而是否可以进行Vertex Compression，则和模型的导入设置以及是否可以进行dynamic batching（在build 阶段，主要判断mesh的顶点数是否符合条件）有关。
简单可以归纳为以下几点：
1. 是否适合进行dynamic batching
2. 在Model Importer中是否开启了Read/Write Enabled
3. 在Model Importer中是否开启了Mesh Compression（是的，很吃惊是吧）
其中第一点，就是该mesh是否适合进行dynamic batching。这里不仅和player setting中是否勾选了dynamic batching有关，还和mesh的顶点数是否超过300有关。当然，dynamic batching是否可行主要和顶点属性的数量有关，但是为了简单，build阶段就按照常见的一个顶点带3个顶点属性，也就是300个顶点来做限制了。
所以如果开启了dynamic batching，则300个顶点以下的mesh不会被执行顶点压缩。
其次，Read/Write Enabled这个选项也会使Vertex Compression失效。所以一般情况下，为了节约内存最好不好勾选这个选项，除了无法进行顶点压缩之外，它还会额外在内存中保留一份mesh数据。
再次，也是大家常常忽略的一点，即如果开启了Mesh Compression，则会override掉Vertex Compression以及Optimize Mesh Data的设置 。Mesh Compression会将mesh在硬盘上的存储空间进行压缩，但是不会在runtime时节省内存。";
                        mgr.Send(Encoding.Default.GetBytes(str));
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
