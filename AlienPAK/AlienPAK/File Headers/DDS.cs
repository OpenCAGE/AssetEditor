using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AlienPAK
{
    public enum TextureCapability : uint
    {
        NONE = 0x0, // not part of DX, added for convenience
        COMPLEX = 0x8, // should be set for any DDS file with more than one main surface
        TEXTURE = 0x1000, // should always be set
        MIPMAP = 0x400000 // only for files with MipMaps
    }

    class DDS
    {
        public uint DDS_MAGIC = 0x44445320; // "DDS "
        public uint dwSize = 0x7C000000; // 124 (little endian)
        public uint dwFlags; // (comes in 2 parts) first short is 4013 2nd short is 8
        public uint dwHeight; // 720
        public uint dwWidth; // 1280
        public uint dwPitchOrLinearSize; // For compressed formats, this is the total number of bytes for the main image.
        public uint dwDepth = 0x0; // For volume textures, this is the depth of the volume.
        public uint dwMipMapCount; // total number of levels in the mipmap chain of the main image.
        
        public UInt32[] dwReserved1 = new UInt32[11]; // 11 UInt32s 11- 1 = 10 because the 0th element is also counted

        //Pixelformat sub-struct, 32 bytes
        public UInt32 pfSize = 32; // Size of Pixelformat structure. This member must be set to 32.
        public UInt32 pfFlags; // Flags to indicate valid fields.
        public UInt32 pfFourCC; // This is the four-character code for compressed formats.

        public UInt32 pfRGBBitCount = 0x0; // For RGB formats, this is the total number of bits in the format. dwFlags should include DDpf_RGB in this case. This value is usually 16, 24, or 32. For A8R8G8B8, this value would be 32.
        public UInt32 pfRBitMask = 0x0; // For RGB formats, these three fields contain the masks for the red, green, and blue channels. For A8R8G8B8, these values would be 0x00ff0000, 0x0000ff00, and 0x000000ff respectively.
        public UInt32 pfGBitMask = 0x0; // ..
        public UInt32 pfBBitMask = 0x0; // ..
        public UInt32 pfABitMask = 0x0; // For RGB formats, this contains the mask for the alpha channel, if any. dwFlags should include DDpf_ALPHAPIXELS in this case. For A8R8G8B8, this value would be 0xff000000.

        //Capabilities sub-struct, 16 bytes
        public UInt32 dwCaps1; // always includes DDSCAPS_TEXTURE. with more than one main surface DDSCAPS_COMPLEX should also be set.
        public UInt32 dwCaps2; // For cubic environment maps, DDSCAPS2_CUBEMAP should be included as well as one or more faces of the map
        public UInt32 dwCaps3;
        public UInt32 dwCaps4;
        public UInt32 dwReserved2; // reserverd
    }
}
