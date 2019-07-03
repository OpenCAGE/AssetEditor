using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AlienPAK
{
    class BigEndianUtils
    {
        /* Tools for reading big endians */
        public int ReadInt32(BinaryReader Reader)
        {
            byte[] data = Reader.ReadBytes(4);
            Array.Reverse(data);
            return BitConverter.ToInt32(data, 0);
        }
        public Int16 ReadInt16(BinaryReader Reader)
        {
            var data = Reader.ReadBytes(2);
            Array.Reverse(data);
            return BitConverter.ToInt16(data, 0);
        }
        public Int64 ReadInt64(BinaryReader Reader)
        {
            var data = Reader.ReadBytes(8);
            Array.Reverse(data);
            return BitConverter.ToInt64(data, 0);
        }
        public UInt32 ReadUInt32(BinaryReader Reader)
        {
            var data = Reader.ReadBytes(4);
            Array.Reverse(data);
            return BitConverter.ToUInt32(data, 0);
        }
    }
}
