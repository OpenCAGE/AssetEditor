using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AlienPAK
{
    class CathodeShaderHeader
    {
        public string ShaderType = "";
    }

    //In a manner similar to that used by COMMANDS.PAK, shaders split up their strings to four byte blocks with their own use info to save on storage space (?)
    class CathodeShaderString
    {
        public byte[] HeaderMagic1; //4
        public byte[] HeaderMagic2; //4
        public byte[] StringPart1; //4
        public byte[] StringPart2; //4
        public int Number1 = 0; //preceeds string1
        public int Number2 = 0; //proceeds string1
    }
}
