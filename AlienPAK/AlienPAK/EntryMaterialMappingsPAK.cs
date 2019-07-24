using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AlienPAK
{
    class EntryMaterialMappingsPAK
    {
        public byte[] header = new byte[4];
        public string filename = "";
        public int entry_number = 0; //materials will be 2* this number
        public List<string> materials = new List<string>();
    }
}
