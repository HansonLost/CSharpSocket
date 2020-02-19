using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HamPig.Network
{
    public class SocketWriteBuffer
    {
        private Queue<ByteArray> m_DataQueue;
        public SocketWriteBuffer()
        {
            m_DataQueue = new Queue<ByteArray>();
        }

        // 若放入数据前队列是空时，会返回打包后的数据
        public ByteArray Add(byte[] data)
        {
            Int16 len = (Int16)data.Length;
            byte[] lenBytes = LittleEndianByte.GetBytes(len);
            byte[] sendBytes = lenBytes.Concat(data).ToArray();

            ByteArray res = null;
            lock (m_DataQueue)
            {
                m_DataQueue.Enqueue(new ByteArray(sendBytes));
                if (m_DataQueue.Count == 1)
                {
                    res = m_DataQueue.First();
                }
            }
            return res;
        }

        public ByteArray Update(Int32 count)
        {
            ByteArray sendingData = null;
            lock (m_DataQueue)
            {
                sendingData = m_DataQueue.First();
            }
            sendingData.Remove(count);
            if(sendingData.size <= 0)
            {
                // 已全放入 system send buffer
                lock (m_DataQueue)
                {
                    m_DataQueue.Dequeue();
                    sendingData = (m_DataQueue.Count > 0 ? m_DataQueue.First() : null);
                }
            }
            return sendingData;
        }

        public bool IsEmpty()
        {
            lock (m_DataQueue)
            {
                return (m_DataQueue.Count <= 0);
            }
        }
    }
}
