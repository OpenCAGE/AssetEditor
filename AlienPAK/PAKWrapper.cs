using CATHODE;
using CathodeLib;
using DirectXTex;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Linq;
using static CATHODE.Textures;
using static DirectXTex.DirectXTexUtility;

namespace AlienPAK
{
    public class PAKWrapper
    {
        /* Get a file from the loaded PAK as a byte array */
        /*
        public byte[] GetFileContent(string FileName)
        {
            switch (_type)
            {
                case PAKType.ANIMATIONS:
                case PAKType.UI:
                    return ((PAK2)_file).Entries.FirstOrDefault(o => o.Filename.Replace('\\', '/') == FileName.Replace('\\', '/'))?.Content;
                case PAKType.TEXTURES:
                    Textures.TEX4 texture = ((Textures)_file).Entries.FirstOrDefault(o => o.Name.Replace('\\', '/') == FileName.Replace('\\', '/'));
                    Textures.TEX4.Texture part = texture?.TextureStreamed?.Content != null ? texture.TextureStreamed : texture?.TexturePersistent?.Content != null ? texture.TexturePersistent : null;
                    if (part == null) return null;
                    DDSHeader theDDSHeader = new DDSHeader();
                    DX10Header theDX10Header = new DX10Header();
                    MemoryStream ms = new MemoryStream();
                    switch (texture.Format)
                    {
                        case Textures.TextureFormat.A32R32G32B32F:
                        case Textures.TextureFormat.A16R16G16B16:
                        case Textures.TextureFormat.A8R8G8B8:
                        case Textures.TextureFormat.X8R8G8B8:
                        case Textures.TextureFormat.A8:
                        case Textures.TextureFormat.L8:
                        case Textures.TextureFormat.A4R4G4B4:
                        case Textures.TextureFormat.DXT1:
                        case Textures.TextureFormat.DXT3:
                        case Textures.TextureFormat.DXN:
                        case Textures.TextureFormat.DXT5:
                        case Textures.TextureFormat.BC6H:
                        case Textures.TextureFormat.BC7:
                        case Textures.TextureFormat.R16F:
                            theDDSHeader.mHeight = (uint)part.Height;
                            theDDSHeader.mWidth = (uint)part.Width;
                            theDDSHeader.mDepth = (uint)part.Depth;
                            theDDSHeader.mMipMapCount = (uint)part.MipLevels;

                            theDDSHeader.mCaps1 = DDSCaps.DDSCAPS_TEXTURE;
                            if (theDDSHeader.mDepth > 1) { theDDSHeader.mFlags |= DDSFlags.DDSD_DEPTH; theDDSHeader.mCaps1 |= DDSCaps.DDSCAPS_COMPLEX; theDDSHeader.mCaps2 |= DDSCaps2.DDSCAPS2_VOLUME; }
                            if (theDDSHeader.mMipMapCount > 0) { theDDSHeader.mFlags |= DDSFlags.DDSD_MIPMAPCOUNT; theDDSHeader.mCaps1 |= DDSCaps.DDSCAPS_COMPLEX; }
                            if (texture.StateFlags.HasFlag(TextureStateFlag.CUBE)) { theDDSHeader.mCaps2 |= DDSCaps2.DDSCAPS2_FULLCUBEMAP; theDX10Header.mMiscFlags |= DDSMiscFlag.DDS_RESOURCE_MISC_TEXTURECUBE; }
                            theDX10Header.mResourceDimension = part.Depth > 1 ? D3D10_RESOURCE_DIMENSION.D3D10_RESOURCE_DIMENSION_TEXTURE3D : D3D10_RESOURCE_DIMENSION.D3D10_RESOURCE_DIMENSION_TEXTURE2D;
                            theDX10Header.mArraySize = 1;

                            //TODO: does this support cubemaps?

                            switch (texture.Format)
                            {
                                case Textures.TextureFormat.A32R32G32B32F: theDX10Header.mDXGIFormat = DXGI_FORMAT.DXGI_FORMAT_R32G32B32A32_FLOAT; break;
                                case Textures.TextureFormat.A16R16G16B16: theDX10Header.mDXGIFormat = DXGI_FORMAT.DXGI_FORMAT_R16G16B16A16_UNORM; break;
                                case Textures.TextureFormat.A8R8G8B8: theDX10Header.mDXGIFormat = DXGI_FORMAT.DXGI_FORMAT_B8G8R8A8_UNORM; break;
                                case Textures.TextureFormat.X8R8G8B8: theDX10Header.mDXGIFormat = DXGI_FORMAT.DXGI_FORMAT_B8G8R8X8_UNORM; break;
                                case Textures.TextureFormat.A8:
                                case Textures.TextureFormat.L8: theDX10Header.mDXGIFormat = DXGI_FORMAT.DXGI_FORMAT_A8_UNORM; break;
                                case Textures.TextureFormat.A4R4G4B4: theDX10Header.mDXGIFormat = DXGI_FORMAT.DXGI_FORMAT_B4G4R4A4_UNORM; break;
                                case Textures.TextureFormat.DXT1: theDX10Header.mDXGIFormat = DXGI_FORMAT.DXGI_FORMAT_BC1_UNORM; break;
                                case Textures.TextureFormat.DXT3: theDX10Header.mDXGIFormat = DXGI_FORMAT.DXGI_FORMAT_BC2_UNORM; break;
                                case Textures.TextureFormat.DXN: theDX10Header.mDXGIFormat = DXGI_FORMAT.DXGI_FORMAT_BC5_UNORM; break;
                                case Textures.TextureFormat.DXT5: theDX10Header.mDXGIFormat = DXGI_FORMAT.DXGI_FORMAT_BC3_UNORM; break;
                                case Textures.TextureFormat.BC6H: theDX10Header.mDXGIFormat = DXGI_FORMAT.DXGI_FORMAT_BC6H_UF16; break;
                                case Textures.TextureFormat.BC7: theDX10Header.mDXGIFormat = DXGI_FORMAT.DXGI_FORMAT_BC7_UNORM; break;
                                case Textures.TextureFormat.R16F: theDX10Header.mDXGIFormat = DXGI_FORMAT.DXGI_FORMAT_R16_FLOAT; break;
                            }
                            using (BinaryWriter bw = new BinaryWriter(ms))
                            {
                                bw.Write(new char[4] { 'D', 'D', 'S', ' ' });
                                Utilities.Write(bw, theDDSHeader);
                                Utilities.Write(bw, theDX10Header);
                                bw.Write(part.Content);
                            }
                            break;
                    }
                    return ms.ToArray();
                case PAKType.MODELS:
                    return null;
            }
        }
        */
    }
}
