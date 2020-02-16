using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HamPig.Network
{
    public class SocketReadBuffer
    {
        public ByteArray recvBuffer { get; private set; }

        private ByteArray m_BuildBuffer;
        private Queue<ByteArray> m_DataQueue;

        public SocketReadBuffer(Int32 capacity = 1024)
        {
            recvBuffer = new ByteArray(capacity);
            m_DataQueue = new Queue<ByteArray>();
        }

        public void Update(Int32 count)
        {
            if (count <= 0) return;

            lock (recvBuffer)
            {
                recvBuffer.Add(count);
                while (recvBuffer.size > 0)
                {
                    Int32 totalLen = recvBuffer.size;
                    if (m_BuildBuffer != null)
                    {
                        Int32 remainLen = m_BuildBuffer.GetFreeLength();
                        Int32 receiveLen = (remainLen <= totalLen ? remainLen : totalLen);
                        Int32 size = m_BuildBuffer.Add(recvBuffer, 0, receiveLen);
                        recvBuffer.Remove(size);

                        if (m_BuildBuffer.GetFreeLength() <= 0)
                        {
                            // 已完全接收一条数据
                            lock (m_DataQueue)
                            {
                                m_DataQueue.Enqueue(m_BuildBuffer);
                            }
                            m_BuildBuffer = null;
                        }
                    }
                    else if (recvBuffer.size >= 2)
                    {
                        Int16 len = LittleEndianByte.GetInt16(recvBuffer);
                        recvBuffer.Remove(2);
                        m_BuildBuffer = new ByteArray(len);
                    }
                    else
                    {
                        // 数据接收完毕或不足凑一个 Int16
                        break;
                    }
                }
            }
        }

        public ByteArray GetData()
        {
            return (m_DataQueue.Count > 0 ? m_DataQueue.Dequeue() : null);
        }
    }
}
