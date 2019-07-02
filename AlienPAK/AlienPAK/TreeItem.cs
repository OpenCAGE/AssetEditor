using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AlienPAK
{
    public enum TreeItemType { EXPORTABLE_FILE, LOADED_STRING, DIRECTORY };

    public struct TreeItem
    {
        public string String_Value;
        public TreeItemType Item_Type;
    }
}
