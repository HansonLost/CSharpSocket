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
            return count;
        }

        public Int32 Add(byte[] data)
        {
            Int32 freeLen = GetFreeLength();
            if(data.Length >= freeLen)
            {
                Array.Copy(buffer, offset, buffer, 0, size);
                offset = 0;
                freeLen = GetFreeLength();
            }
            Int32 copyLen = (data.Length <= freeLen ? data.Length : freeLen);
            Array.Copy(data, 0, buffer, offset + size, copyLen);
            return copyLen;
        }

        public byte[] ToBytes()
        {
            if (size <= 0) return null;
            byte[] res = new byte[size];
            Array.Copy(buffer, offset, res, 0, size);
            return res;
        }

        private Int32 GetFreeLength()
        {
            return buffer.Length - (offset + size);
        }
    }
}
