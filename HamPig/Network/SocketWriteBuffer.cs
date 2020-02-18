using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HamPig.Network
{
    public class SocketWriteBuffer
    {
        private Queue<ByteArray> m_ExDataQueue;
        public SocketWriteBuffer()
        {
            m_ExDataQueue = new Queue<ByteArray>();
        }

        // 若放入数据前队列是空时，会返回打包后的数据
        public ByteArray Add(byte[] data)
        {
            Int16 len = (Int16)data.Length;
            byte[] lenBytes = LittleEndianByte.GetBytes(len);
            byte[] sendBytes = lenBytes.Concat(data).ToArray();

            ByteArray res = null;
            lock (m_ExDataQueue)
            {
                m_ExDataQueue.Enqueue(new ByteArray(sendBytes));
                if (m_ExDataQueue.Count == 1)
                {
                    res = m_ExDataQueue.First();
                }
            }
            return res;
        }

        public ByteArray Update(Int32 count)
        {
            ByteArray sendingData = null;
            lock (m_ExDataQueue)
            {
                sendingData = m_ExDataQueue.First();
            }
            sendingData.Remove(count);
            if(sendingData.size <= 0)
            {
                // 已全放入 system send buffer
                lock (m_ExDataQueue)
                {
                    m_ExDataQueue.Dequeue();
                    sendingData = (m_ExDataQueue.Count > 0 ? m_ExDataQueue.First() : null);
                }
            }
            return sendingData;
        }
    }
}
