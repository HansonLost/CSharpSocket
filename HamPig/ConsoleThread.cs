using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace HamPig
{
    public class ConsoleAsync
    {
        public bool isRunning { get; private set; }
        private string m_Cmd;
        
        public string TryReadLine()
        {
            string res = null;
            if(isRunning)
            {
                res = m_Cmd;
                m_Cmd = null;
            }
            else
            {
                isRunning = true;
                ThreadStart ts = new ThreadStart(Main);
                Thread thread = new Thread(ts);
                thread.Start();
            }
            return res;
        }

        private void Main()
        {
            m_Cmd = System.Console.ReadLine();
            isRunning = false;
        }
    }
}
