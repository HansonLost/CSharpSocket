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

        public byte[] buffer { get; private set; }
        public Int32 offset { get; private set; }
        public Int32 size { get; private set; }
        public Int32 capacity { get { return buffer.Length; } }

        public SocketReadBuffer(Int32 capacity = 1024)
        {
            recvBuffer = new ByteArray(capacity);
            m_DataQueue = new Queue<ByteArray>();

            buffer = new byte[capacity];
            offset = 0;
            size = 0;
        }

        // 由 socket 线程调用
        public void Update(Int32 count)
        {
            if (count <= 0) return;

            recvBuffer.Add(count);

            while(recvBuffer.size > 0)
            {
                Int32 totalLen = recvBuffer.size;
                if (m_BuildBuffer != null)
                {
                    Int32 remainLen = m_BuildBuffer.GetFreeLength();
                    Int32 receiveLen = (remainLen <= totalLen ? remainLen : totalLen);
                    Int32 size = m_BuildBuffer.Add(recvBuffer, 0, receiveLen);
                    recvBuffer.Remove(size);

                    if(m_BuildBuffer.GetFreeLength() <= 0)
                    {
                        // 已完全接收一条数据
                        m_DataQueue.Enqueue(m_BuildBuffer);
                        m_BuildBuffer = null;

                        // 输出
                        byte[] data = m_DataQueue.First().ToBytes();
                        string str = Encoding.Default.GetString(data);
                        Console.WriteLine(String.Format("receive : {0}", str));
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

        /// <summary>
        /// 记录新存放的数据。该接口不会检验 count 的合法性。
        /// </summary>
        public void Receive(Int32 count)
        {
            size += count;
        }

        public byte[] GetData()
        {
            if (size <= 2) return null; // buffer 中没有实际 data
            Int16 len = LittleEndianByte.GetInt16(buffer, offset);
            if (size < 2 + len) return null;    // buffer 中没有完整的 data
            byte[] data = new byte[len];
            Array.Copy(buffer, offset + 2, data, 0, len);
            offset += 2 + len;
            size -= 2 + len;
            return data;
        }

        /// <summary>
        /// 整理 buffer 空间
        /// </summary>
        public void Refresh()
        {
            Array.Copy(buffer, offset, buffer, 0, size);
            offset = 0;
        }
    }
}
