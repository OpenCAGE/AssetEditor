using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AlienPAK
{
    class EntryTexturePAK
    {
        public string FileName = "";
        public int[] Dimensions = { 0, 0 };
        public EntryTextureBIN BinHeader = null;
        public int BinIndex = -1; //BinHeader should link to BinIndex
        public int Size1 = -1;
        public int Size2 = -1;
        public byte[] Content;
        public bool Error = false;
    }
}
