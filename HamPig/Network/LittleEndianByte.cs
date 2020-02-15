using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HamPig.Network
{
    /// <summary>
    /// 小端编码转换器
    /// </summary>
    public class LittleEndianByte
    {
        public static byte[] GetBytes(Int16 value)
        {
            byte[] res = BitConverter.GetBytes(value);
            if (!BitConverter.IsLittleEndian)
            {
                res.Reverse();
            }
            return res;

        }

        public static Int16 GetInt16(byte[] value, int startIndex)
        {
            return (Int16)(value[startIndex + 1] << 8 | value[startIndex]);
        }
        public static Int16 GetInt16(byte[] value)
        {
            return GetInt16(value, 0);
        }
        
    }
}
