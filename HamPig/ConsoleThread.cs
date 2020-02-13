using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace HamPig
{
    public class ConsoleThread
    {
        public bool isRunning { get; private set; }
        private string m_Cmd;
        
        public void Receive()
        {
            if (isRunning)
            {
                return;
            }
            isRunning = true;

            ThreadStart ts = new ThreadStart(Main);
            Thread thread = new Thread(ts);
            thread.Start();
        }

        private void Main()
        {
            m_Cmd = System.Console.ReadLine();
            isRunning = false;
        }

        public string GetCommand()
        {
            return m_Cmd;
        }
    }
}
