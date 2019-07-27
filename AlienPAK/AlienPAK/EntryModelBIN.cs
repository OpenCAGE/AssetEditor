using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AlienPAK
{
    class EntryModelBIN
    {
        public string Filename = "";
        public string ModelPartName = "";

        public int FilenameOffset = 0;
        public int ModelPartNameOffset = 0;
        public int MaterialLibaryIndex = 0;
        public int BlockSize = 0;
        public int ScaleFactor = 0;
        public int VertCount = 0;
        public int FaceCount = 0;
        public int BoneCount = 0;
    }
}
