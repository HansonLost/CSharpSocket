using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HamPig.Network
{
    public class ByteArray
    {
        public byte[] buffer { get; private set; }
        public Int32 offset { get; private set; }
        public Int32 size { get; private set; }

        public ByteArray(Int32 capacity = 1024)
        {
            buffer = new byte[capacity];
            offset = 0;
            size = 0;
        }

        public Int32 Remove(Int32 count)
        {
            count = (count <= size ? count : size);
            offset += count;
            size -= count;
            if(GetFreeLength() <= 0)
            {
                RefreshSpace();
            }
            return count;
        }

        public Int32 Add(byte[] data, Int32 offset, Int32 size)
        {
            Int32 freeLen = GetFreeLength();
            if(size >= freeLen)
            {
                Array.Copy(buffer, this.offset, buffer, 0, this.size);
                this.offset = 0;
                freeLen = GetFreeLength();
            }
            Int32 copyLen = (size <= freeLen ? size : freeLen);
            Array.Copy(data, offset, buffer, this.offset + this.size, copyLen);
            this.size += size;
            return copyLen;
        }

        public Int32 Add(byte[] data)
        {
            return Add(data, 0, data.Length);
        }

        public Int32 Add(ByteArray data, Int32 offset, Int32 size)
        {
            return Add(data.buffer, data.offset + offset, size);
        }

        public Int32 Add(ByteArray data)
        {
            return Add(data.buffer, data.offset, data.size);
        }

        /// <summary>
        /// 慎用!新添加的数据为跟随在已利用数据后、暂未利用的数据。
        /// </summary>
        public Int32 Add(Int32 size)
        {
            Int32 freeLen = GetFreeLength();
            Int32 newLen = (size <= freeLen ? size : freeLen);
            this.size += newLen;
            size -= newLen;

            if(freeLen == newLen)
            {
                RefreshSpace();
                freeLen = GetFreeLength();
            }

            if(size > 0)
            {
                Int32 addtLen = (size <= freeLen ? size : freeLen);
                this.size += addtLen;
                newLen += addtLen;
            }
            return newLen;
        }

        public byte[] ToBytes()
        {
            if (size <= 0) return null;
            byte[] res = new byte[size];
            Array.Copy(buffer, offset, res, 0, size);
            return res;
        }

        public Int32 GetFreeLength()
        {
            return buffer.Length - (offset + size);
        }

        private void RefreshSpace()
        {
            Array.Copy(buffer, offset, buffer, 0, size);
            offset = 0;
        }
    }
}
