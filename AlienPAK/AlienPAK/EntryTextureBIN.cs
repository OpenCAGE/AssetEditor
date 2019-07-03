using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AlienPAK
{
    public enum TextureFormats : int
    {
        DXGI_FORMAT_B8G8R8A8_UNORM = 0x2,
        DXGI_FORMAT_B8G8R8_UNORM = 0x5,
        DXGI_FORMAT_BC1_UNORM = 0x6,
        DXGI_FORMAT_BC3_UNORM = 0x9,
        DXGI_FORMAT_BC5_UNORM = 0x8,
        DXGI_FORMAT_BC7_UNORM = 0xD
    }

    class EntryTextureBIN
    {
        public TextureFormats TextureFormat;
        public int Width = -1;
        public int Height = -1;
    }
}
