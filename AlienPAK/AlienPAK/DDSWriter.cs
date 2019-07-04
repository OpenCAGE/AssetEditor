using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AlienPAK
{
    /*
     *
     * Our DDS writer.
     * This script was originally created by Cra0kalo and Volfin, published at: https://github.com/cra0kalo/AITexExtract/blob/master/AITexExtract/DDS.cs
     * 
     * For now I've just tidied it up, but its feature set will likely be extended.
     * Some bits look a little dodgy!
     * 
    */
    public class DDSWriter
    {
        private DDS DDS_HEADER = new DDS();
        private byte[] DDS_CHUNK;
        
        private bool HasMipMaps;
        private int Pixelres;
        private TextureType File_DDSFORMAT;
        

        //Surface Description
        private enum SurfaceDescription : uint
        {
            DDSD_CAPS = 0x1, //Required in every .dds file.
            DDSD_HEIGHT = 0x2, //Required in every .dds file.
            DDSD_WIDTH = 0x4, //Required in every .dds file.
            DDSD_PITCH = 0x8, //Required when pitch is provided for an uncompressed texture.
            DDSD_PIXELFORMAT = 0x1000, //Required in every .dds file.
            DDSD_MIPMAPCOUNT = 0x20000, //Required in a mipmapped texture.
            DDSD_LINEARSIZE = 0x80000, //Required when pitch is provided for a compressed texture.
            DDSD_DEPTH = 0x800000 //Required in a depth texture.
        }

        //Pixel Format
        private enum PixelFormat : uint
        {
            NONE = 0x0, // not part of DX, added for convenience
            ALPHAPIXELS = 0x1,
            FOURCC = 0x4,
            RGB = 0x40,
            RGBA = 0x41
        }
        
        //Texture Types
        public enum TextureType : uint
        {
            UNCOMPRESSED_GENERAL,
            Unknown,
            Dxt1,
            Dxt3,
            Dxt5,
            R8G8B8,
            B8G8R8,
            Bgra5551,
            Bgra4444,
            Bgr565,
            Alpha8,
            X8R8G8B8,
            A8R8G8B8,
            A8B8G8R8,
            X8B8G8R8,
            RGB555,
            R32F,
            R16F,
            A32B32G32R32F,
            A16B16G16R16F,
            Q8W8V8U8,
            CxV8U8,
            G16R16F,
            G32R32F,
            G16R16,
            A2B10G10R10,
            A16B16G16R16,
            ATI2N,
            BC7_UNORM
        }
        
        //FourCC
        private enum FourCC : uint
        {
            D3DFMT_DXT1 = 0x31545844,
            D3DFMT_DXT2 = 0x32545844,
            D3DFMT_DXT3 = 0x33545844,
            D3DFMT_DXT4 = 0x34545844,
            D3DFMT_DXT5 = 0x35545844,
            D3DFMT_ATI2N = 0x32495441,
            DX10 = 0x30315844,
            DXGI_FORMAT_BC4_UNORM = 0x55344342,
            DXGI_FORMAT_BC4_SNORM = 0x53344342,
            DXGI_FORMAT_BC5_UNORM = 0x32495441,
            DXGI_FORMAT_BC5_SNORM = 0x53354342,

            //DXGI_FORMAT_R8G8_B8G8_UNORM
            D3DFMT_R8G8_B8G8 = 0x47424752,

            //DXGI_FORMAT_G8R8_G8B8_UNORM
            D3DFMT_G8R8_G8B8 = 0x42475247,

            //DXGI_FORMAT_R16G16B16A16_UNORM
            D3DFMT_A16B16G16R16 = 36,

            //DXGI_FORMAT_R16G16B16A16_SNORM
            D3DFMT_Q16W16V16U16 = 110,

            //DXGI_FORMAT_R16_FLOAT
            D3DFMT_R16F = 111,

            //DXGI_FORMAT_R16G16_FLOAT
            D3DFMT_G16R16F = 112,

            //DXGI_FORMAT_R16G16B16A16_FLOAT
            D3DFMT_A16B16G16R16F = 113,

            //DXGI_FORMAT_R32_FLOAT
            D3DFMT_R32F = 114,

            //DXGI_FORMAT_R32G32_FLOAT
            D3DFMT_G32R32F = 115,

            //DXGI_FORMAT_R32G32B32A32_FLOAT
            D3DFMT_A32B32G32R32F = 116,

            D3DFMT_UYVY = 0x59565955,
            D3DFMT_YUY2 = 0x32595559,
            D3DFMT_CxV8U8 = 117,

            //This is set only by the nvidia exporter, it is not set by the dx texture tool
            //,it is ignored by the dx texture tool but it returns the ability to be opened in photoshop so I decided to keep it.
            D3DFMT_Q8W8V8U8 = 63
        }
        
        /* Populate the DDS with provided info */
        public DDSWriter(byte[] DATACHUNK, int image_width, int image_height, int resolution, int mipmaps_count, TextureType format)
        {
            //Grab the contents of the file and set width/height
            DDS_CHUNK = DATACHUNK;
            DDS_HEADER.dwHeight = Convert.ToUInt32(image_height);
            DDS_HEADER.dwWidth = Convert.ToUInt32(image_width);

            //Resolution/format
            Pixelres = resolution;
            File_DDSFORMAT = format;

            //Mipmap count
            if (mipmaps_count <= 0)
            {
                DDS_HEADER.dwMipMapCount = 0x0;
                HasMipMaps = false;
            }
            else
            {
                DDS_HEADER.dwMipMapCount = Convert.ToUInt32(mipmaps_count);
                HasMipMaps = true;
            }
            
            //Create the DDS header
            CreateHeader();
        }

        /* Populate the DDS and skip header step, will export with best-guess header info */
        public DDSWriter(byte[] DATACHUNK, int image_width, int image_height)
        {
            //Grab the contents of the file and set width/height
            DDS_CHUNK = DATACHUNK;
            DDS_HEADER.dwHeight = Convert.ToUInt32(image_height);
            DDS_HEADER.dwWidth = Convert.ToUInt32(image_width);
        }
        
        /* Generate the DDS header */
        private void CreateHeader()
        {
            //PitchOrLinearSize
            DDS_HEADER.dwPitchOrLinearSize = Convert.ToUInt32(CalculatePitch(Convert.ToInt32(DDS_HEADER.dwWidth), Convert.ToInt32(DDS_HEADER.dwHeight), Pixelres, (int)File_DDSFORMAT));

            //FlagsA
            CalculateFlagA(File_DDSFORMAT);

            //FlagsB
            CalculateFlagB(File_DDSFORMAT, Pixelres);

            //FourCC if required
            switch (File_DDSFORMAT)
            {
                case TextureType.Dxt1:
                    DDS_HEADER.pfFourCC = (UInt32)FourCC.D3DFMT_DXT1;
                    break;
                case TextureType.Dxt3:
                    DDS_HEADER.pfFourCC = (UInt32)FourCC.D3DFMT_DXT3;
                    break;
                case TextureType.Dxt5:
                    DDS_HEADER.pfFourCC = (UInt32)FourCC.D3DFMT_DXT5;
                    break;
                case TextureType.ATI2N:
                    DDS_HEADER.pfFourCC = (UInt32)FourCC.D3DFMT_ATI2N;
                    break;
                default:
                    DDS_HEADER.pfFourCC = 0x0;
                    break;
            }
            
            //Set info required by uncompressed formats 
            if (!(File_DDSFORMAT == TextureType.Dxt1 || File_DDSFORMAT == TextureType.Dxt3 || File_DDSFORMAT == TextureType.Dxt5 || File_DDSFORMAT == TextureType.ATI2N))
            {
                //Set RGBBitCount
                DDS_HEADER.pfRGBBitCount = Convert.ToUInt32(Pixelres); //Must set for the resolution (24 for no alpha, 32 for alpha)

                //Bitmask as follows
                DDS_HEADER.pfRBitMask = 0xFF0000;
                DDS_HEADER.pfGBitMask = 0xFF00;
                DDS_HEADER.pfBBitMask = 0xFF;
                DDS_HEADER.pfABitMask = unchecked(0xFF000000U); //Must append UI to the end of 0xff000000
            }

            //DwCaps1 - TODO: Extend to support other stuff
            DDS_HEADER.dwCaps1 = (UInt32)TextureCapability.TEXTURE;
            DDS_HEADER.dwCaps2 = (UInt32)TextureCapability.NONE;
            DDS_HEADER.dwCaps3 = (UInt32)TextureCapability.NONE;
            DDS_HEADER.dwCaps4 = (UInt32)TextureCapability.NONE;
            DDS_HEADER.dwReserved2 = 0x0;
        }
        
        /* Calculate the image's pitch */
        public static int CalculatePitch(int width, int height, int resolution, int compressionFormat)
        {
            int pitch = ((width + 3) / 4) * ((height + 3) / 4) * ((resolution + 3) / 4);
            switch (compressionFormat)
            {
                case (int)TextureType.Dxt1:
                    pitch *= 1; // 1 for rgb
                    break;
                case (int)TextureType.Dxt3:
                case (int)TextureType.Dxt5:
                case (int)TextureType.ATI2N:
                case (int)TextureType.BC7_UNORM:
                    pitch *= 2; //True 16 block size ( 1 ALPHA 1 RGB)
                    break;
                default:
                    pitch *= 8; //shortcut for uncompressed
                    break;
            }
            return pitch;
        }
        
        /* Calculate header flag A */
        private void CalculateFlagA(TextureType format)
        {
            //temp variables
            bool DDSD_CAPS = true; ////required to be true
            bool DDSD_HEIGHT = true; ////required to be true
            bool DDSD_WIDTH = true; ////required to be true
            bool DDSD_PITCH = false;
            bool DDSD_PIXELFORMAT = true; ////required to be true
            bool DDSD_MIPMAPCOUNT = false;
            bool DDSD_LINEARSIZE = false;
            bool DDSD_DEPTH = false;
            
            //test_format
            if (format == TextureType.Dxt1 || format == TextureType.Dxt3 || format == TextureType.Dxt5 || format == TextureType.ATI2N)
            {
                //its a compressed texture with the linearsize provided
                DDSD_LINEARSIZE = true;
            }
            else
            {
                //its a uncompreesed texture with the pitch provided
                // DDSD_PITCH = True '/ disabling this because for some reason if you export a texture from photoshop that has the pitch provided in the file this flag is not set
                DDSD_LINEARSIZE = true; //// leaving this here because it works
            }
            
            //mipmaps
            if (HasMipMaps == true)
            {
                //texture uses mipmaps
                DDSD_MIPMAPCOUNT = true;
            }
            
            //Calculate the 4BYTE WORD FLAG
            uint total_owning = 0;
            if (DDSD_CAPS == true)
            {
                total_owning += (uint)SurfaceDescription.DDSD_CAPS;
            }
            if (DDSD_HEIGHT == true)
            {
                total_owning += (uint)SurfaceDescription.DDSD_HEIGHT;
            }
            if (DDSD_WIDTH == true)
            {
                total_owning += (uint)SurfaceDescription.DDSD_WIDTH;
            }
            if (DDSD_PITCH == true)
            {
                total_owning += (uint)SurfaceDescription.DDSD_PITCH;
            }
            if (DDSD_PIXELFORMAT == true)
            {
                total_owning += (uint)SurfaceDescription.DDSD_PIXELFORMAT;
            }
            if (DDSD_MIPMAPCOUNT == true)
            {
                total_owning += (uint)SurfaceDescription.DDSD_MIPMAPCOUNT;
            }
            if (DDSD_LINEARSIZE == true)
            {
                total_owning += (uint)SurfaceDescription.DDSD_LINEARSIZE;
            }
            if (DDSD_DEPTH == true)
            {
                total_owning += (uint)SurfaceDescription.DDSD_DEPTH;
            }
            DDS_HEADER.dwFlags = total_owning;
        }
        
        /* Calculate header flag B */
        private void CalculateFlagB(TextureType format, int res)
        {
            bool DDPF_FOURCC = false;
            bool DDPF_RGB = false;
            bool DDPF_ALPHAPIXELS = false;
            
            if (format == TextureType.Dxt1 || format == TextureType.Dxt3 || format == TextureType.Dxt5 || format == TextureType.ATI2N)
            {
                //compressed
                //set FourCC flag
                DDPF_FOURCC = true;

                //no need to set alpha channel its always going to only have thoe fourcc flag
            }
            else
            {
                //uncompressed
                //SET uncompressed flag
                DDPF_RGB = true;
                
                if (res == 24)
                {
                    //no alpha channel
                    DDPF_ALPHAPIXELS = false;
                }
                else if (res == 32)
                {
                    //has alpha channel
                    DDPF_ALPHAPIXELS = true;
                }
            }

            //Create the 4BYTE WORD FLAG (2nd flag)
            uint total_owning = 0;
            if (DDPF_FOURCC == true)
            {
                total_owning += (uint)PixelFormat.FOURCC;
            }
            if (DDPF_RGB == true)
            {
                total_owning += (uint)PixelFormat.RGB;
            }
            if (DDPF_ALPHAPIXELS == true)
            {
                total_owning += (uint)PixelFormat.ALPHAPIXELS;
            }
            DDS_HEADER.pfFlags = total_owning;
        }
        
        /* Save the final file */
        public void Save(string path_File2write)
        {
            //Save out the DDS file
            FileStream outputStream = new FileStream(path_File2write, FileMode.Create);
            BinaryWriter bw = new BinaryWriter(outputStream);
            BigEndianUtils EndianUtils = new BigEndianUtils();

            //Write Header first
            bw.Write(EndianUtils.FlipEndian(BitConverter.GetBytes((int)DDS_HEADER.DDS_MAGIC)));
            bw.Write(EndianUtils.FlipEndian(BitConverter.GetBytes((int)DDS_HEADER.dwSize)));
            bw.Write(DDS_HEADER.dwFlags);
            bw.Write(DDS_HEADER.dwHeight);
            bw.Write(DDS_HEADER.dwWidth);
            bw.Write(DDS_HEADER.dwPitchOrLinearSize);
            bw.Write(DDS_HEADER.dwDepth);
            bw.Write(DDS_HEADER.dwMipMapCount);

            foreach (UInt32 value in DDS_HEADER.dwReserved1)
            {
                bw.Write(value);
            }

            bw.Write(DDS_HEADER.pfSize);
            bw.Write(DDS_HEADER.pfFlags);
            bw.Write(DDS_HEADER.pfFourCC);

            bw.Write(DDS_HEADER.pfRGBBitCount);
            bw.Write(DDS_HEADER.pfRBitMask);
            bw.Write(DDS_HEADER.pfGBitMask);
            bw.Write(DDS_HEADER.pfBBitMask);
            bw.Write(DDS_HEADER.pfABitMask);

            bw.Write(DDS_HEADER.dwCaps1);
            bw.Write(DDS_HEADER.dwCaps2);
            bw.Write(DDS_HEADER.dwCaps3);
            bw.Write(DDS_HEADER.dwCaps4);

            bw.Write(DDS_HEADER.dwReserved2);

            //Write chunk
            bw.Write(DDS_CHUNK);

            //Finished!
            outputStream.Close();
            bw.Close();
        }

        /* Save the final file with a guess at the header */
        public void SaveCrude(string path_File2write)
        {
            //Save out the DDS file
            FileStream outputStream = new FileStream(path_File2write, FileMode.Create);
            BinaryWriter bw = new BinaryWriter(outputStream);
            
            //Failed to come up with a suitable header, try a best guess...
            bw.Write(542327876);
            bw.Write(124);
            bw.Write(659463);
            bw.Write(DDS_HEADER.dwHeight);
            bw.Write(DDS_HEADER.dwWidth);
            bw.Write(CalculatePitch((int)this.DDS_HEADER.dwWidth, (int)this.DDS_HEADER.dwHeight, 32, 3));
            bw.Write(1);
            bw.Write(8);
            
            foreach (UInt32 value in DDS_HEADER.dwReserved1)
            {
                bw.Write(value);
            }

            bw.Write(32);
            bw.Write(4);
            bw.Write(808540228);
            
            bw.Write(DDS_HEADER.pfRGBBitCount);
            bw.Write((uint)0xFF0000);
            bw.Write((uint)0xFF00);
            bw.Write((uint)0xFF);
            bw.Write((uint)0xFF000000);
            
            bw.Write(4198408);
            bw.Write(DDS_HEADER.dwCaps2);
            bw.Write(DDS_HEADER.dwCaps3);
            bw.Write(DDS_HEADER.dwCaps4);
            
            bw.Write(DDS_HEADER.dwReserved2);
            
            bw.Write(98);
            bw.Write(3);
            bw.Write(0);
            bw.Write(1);

            bw.Write(0);
            
            //Write chunk
            bw.Write(DDS_CHUNK);
            
            //Once we have written to the file close the stream and writter
            outputStream.Close();
            bw.Close();
        }
    }
}
