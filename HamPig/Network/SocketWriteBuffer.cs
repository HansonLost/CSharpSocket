using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HamPig.Network
{
    public class SocketWriteBuffer
    {
        private Queue<byte[]> m_DataQueue;
        public Int32 offset { get; private set; }
        public Int32 size { get; private set; }


        public SocketWriteBuffer()
        {
            m_DataQueue = new Queue<byte[]>();
        }

        // 主线程调用
        public byte[] Add(byte[] data)
        {
            Int16 len = (Int16)data.Length;
            byte[] lenBytes = LittleEndianByte.GetBytes(len);
            byte[] sendBytes = lenBytes.Concat(data).ToArray();

            byte[] buffer = null;
            lock (m_DataQueue)
            {
                m_DataQueue.Enqueue(sendBytes);
                if(m_DataQueue.Count == 1)
                {
                    buffer = m_DataQueue.First();
                }
            }
            if(buffer != null)
            {
                offset = 0;
                size = buffer.Length;
            }
            return buffer;
        }

        // socket 线程调用
        public byte[] Update(Int32 count)
        {
            byte[] buffer = null;
            lock (m_DataQueue)
            {
                buffer = m_DataQueue.First();
            }
            offset += count;
            size -= count;

            if(size <= 0)
            {
                offset = 0;
                size = 0;

                lock (m_DataQueue)
                {
                    m_DataQueue.Dequeue();
                    buffer = (m_DataQueue.Count > 0 ? m_DataQueue.First() : null);
                }
                if(buffer != null)
                {
                    offset = 0;
                    size = buffer.Length;
                }
            }
            return buffer;
        }
    }
}
