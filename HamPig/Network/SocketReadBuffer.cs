using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HamPig.Network
{
    public class SocketReadBuffer
    {
        public byte[] buffer { get; private set; }
        public Int32 offset { get; private set; }
        public Int32 size { get; private set; }
        public Int32 capacity { get { return buffer.Length; } }

        public SocketReadBuffer(Int32 capacity = 1024)
        {
            buffer = new byte[capacity];
            offset = 0;
            size = 0;
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
