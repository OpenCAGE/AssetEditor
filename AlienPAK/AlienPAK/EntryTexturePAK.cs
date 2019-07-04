using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AlienPAK
{
    class EntryTexturePAK
    {
        public enum ThisEntryType { ENTRY_MIN, ENTRY_MAX };

        public Int16[] Dimensions = { 0, 0 };
        public EntryTextureBIN BinHeader = null;
        public int BinIndex = -1; //BinHeader should link to BinIndex
        public int Size = -1;
        public byte[] Content;
        public ThisEntryType Type;
    }
}
