using Assimp;
using Assimp.Configs;
using CATHODE;
using CathodeLib;
using DirectXTex;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using static CATHODE.Materials.Material;
using static CATHODE.Models;
using static CATHODE.Models.CS2.Component.LOD;
using static CATHODE.Textures;
using static DirectXTex.DirectXTexUtility;
using Color = System.Windows.Media.Color;
using PixelFormat = System.Drawing.Imaging.PixelFormat;
using Vector3D = System.Windows.Media.Media3D.Vector3D;

namespace AlienPAK
{
    public static class CathodeLibExtensions
    {
        /* Convert a TEX4 to DDS */
        public static byte[] ToDDS(this Textures.TEX4 texture, bool forceLowRes = false)
        {
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

                        default:
                            throw new Exception("Unsupported");
                    }
                    using (BinaryWriter bw = new BinaryWriter(ms))
                    {
                        bw.Write(new char[4] { 'D', 'D', 'S', ' ' });
                        Utilities.Write(bw, theDDSHeader);
                        Utilities.Write(bw, theDX10Header);
                        bw.Write(part.Content);
                    }
                    break;

                default:
                    throw new Exception("Unsupported");
            }
            return ms.ToArray();
        }

        /* Convert DDS to a TEX4 Part */
        public static Textures.TEX4.Texture ToTEX4Part(this byte[] content, out Textures.TextureFormat format)
        {
            Textures.TEX4.Texture part = new TEX4.Texture();
            format = TextureFormat.AUTO;

            using (MemoryStream stream = new MemoryStream(content))
            using (BinaryReader reader = new BinaryReader(stream))
            {
                reader.BaseStream.Position += 4;
                DDSHeader ddsHeader = Utilities.Consume<DDSHeader>(reader);
                DX10Header dx10Header = null;
                if (ddsHeader.mPixelFormat.mFlags == DDSPixelFormat.DDPF_FOURCC &&
                    ddsHeader.mPixelFormat.mFourCC[0] == 'D' && ddsHeader.mPixelFormat.mFourCC[1] == 'X' && ddsHeader.mPixelFormat.mFourCC[2] == '1' && ddsHeader.mPixelFormat.mFourCC[3] == '0')
                    dx10Header = Utilities.Consume<DX10Header>(reader);

                if (dx10Header == null)
                    return null;

                part.Depth = (short)ddsHeader.mDepth;
                part.MipLevels = (short)ddsHeader.mMipMapCount;
                part.Width = (short)ddsHeader.mWidth;
                part.Height = (short)ddsHeader.mHeight;
                part.Content = reader.ReadBytes((int)(reader.BaseStream.Length - reader.BaseStream.Position));

                switch (dx10Header.mDXGIFormat)
                {
                    case DXGI_FORMAT.DXGI_FORMAT_R32G32B32A32_FLOAT:
                        format = Textures.TextureFormat.A32R32G32B32F;
                        break;
                    case DXGI_FORMAT.DXGI_FORMAT_R16G16B16A16_UNORM:
                        format = Textures.TextureFormat.A16R16G16B16;
                        break;
                    case DXGI_FORMAT.DXGI_FORMAT_B8G8R8A8_UNORM:
                        format = Textures.TextureFormat.A8R8G8B8;
                        break;
                    case DXGI_FORMAT.DXGI_FORMAT_B8G8R8X8_UNORM:
                        format = Textures.TextureFormat.X8R8G8B8;
                        break;
                    case DXGI_FORMAT.DXGI_FORMAT_A8_UNORM:
                        format = Textures.TextureFormat.A8; //A8 and L8 both map to A8_UNORM
                        break;
                    case DXGI_FORMAT.DXGI_FORMAT_B4G4R4A4_UNORM:
                        format = Textures.TextureFormat.A4R4G4B4;
                        break;
                    case DXGI_FORMAT.DXGI_FORMAT_BC1_UNORM:
                        format = Textures.TextureFormat.DXT1;
                        break;
                    case DXGI_FORMAT.DXGI_FORMAT_BC2_UNORM:
                        format = Textures.TextureFormat.DXT3;
                        break;
                    case DXGI_FORMAT.DXGI_FORMAT_BC5_UNORM:
                        format = Textures.TextureFormat.DXN;
                        break;
                    case DXGI_FORMAT.DXGI_FORMAT_BC3_UNORM:
                        format = Textures.TextureFormat.DXT5;
                        break;
                    case DXGI_FORMAT.DXGI_FORMAT_BC6H_UF16:
                        format = Textures.TextureFormat.BC6H;
                        break;
                    case DXGI_FORMAT.DXGI_FORMAT_BC7_UNORM:
                        format = Textures.TextureFormat.BC7;
                        break;
                    case DXGI_FORMAT.DXGI_FORMAT_R16_FLOAT:
                        format = Textures.TextureFormat.R16F;
                        break;
                    default:
                        return null;
                }
            }
            return part;
        }

        /* Convert a TEX4 to Bitmap */
        public static Bitmap ToBitmap(this Textures.TEX4 texture, bool forceLowRes = false)
        {
            byte[] content = texture?.ToDDS(forceLowRes);
            return content?.ToBitmap();
        }
        public static Bitmap ToBitmap(this byte[] content, bool forceLowRes = false)
        {
            Bitmap toReturn = null;
            if (content == null) return null;
            try
            {
                MemoryStream imageStream = new MemoryStream(content);
                using (var image = Pfim.Pfim.FromStream(imageStream))
                {
                    PixelFormat format = PixelFormat.DontCare;
                    switch (image.Format)
                    {
                        case Pfim.ImageFormat.Rgba32:
                            format = PixelFormat.Format32bppArgb;
                            break;
                        case Pfim.ImageFormat.Rgb24:
                            format = PixelFormat.Format24bppRgb;
                            break;
                        case Pfim.ImageFormat.Rgb8:
                            format = PixelFormat.Format8bppIndexed;
                            break;
                        default:
                            Console.WriteLine("Unsupported DDS: " + image.Format);
                            break;
                    }
                    if (format != PixelFormat.DontCare)
                    {
                        var handle = GCHandle.Alloc(image.Data, GCHandleType.Pinned);
                        try
                        {
                            var data = Marshal.UnsafeAddrOfPinnedArrayElement(image.Data, 0);
                            toReturn = new Bitmap(image.Width, image.Height, image.Stride, format, data);
                        }
                        finally
                        {
                            handle.Free();
                        }
                    }
                }
            }
            catch
            {
                return null;
            }
            return toReturn;
        }

        [DllImport("gdi32.dll", EntryPoint = "DeleteObject")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool DeleteObject([In] IntPtr hObject);

        /* Convert a Bitmap to ImageSource */
        public static ImageSource ToImageSource(this Bitmap bmp)
        {
            var handle = bmp.GetHbitmap();
            try
            {
                return Imaging.CreateBitmapSourceFromHBitmap(handle, IntPtr.Zero, Int32Rect.Empty, System.Windows.Media.Imaging.BitmapSizeOptions.FromEmptyOptions());
            }
            finally { DeleteObject(handle); }
        }

        public static GeometryModel3D ToGeometryModel3D(this CS2.Component.LOD.Submesh submesh)
        {
            if (submesh.Data.Length == 0)
                return new GeometryModel3D();

            Int32Collection indices = new Int32Collection();
            Point3DCollection vertices = new Point3DCollection();
            PointCollection[] uvs = new PointCollection[0];

            using (BinaryReader reader = new BinaryReader(new MemoryStream(submesh.Data)))
            {
                for (int i = 0; i < submesh.VertexFormatFull.Attributes.Count; ++i)
                {
                    if (i == submesh.VertexFormatFull.Attributes.Count - 1)
                    {
                        for (int x = 0; x < submesh.IndexCount; x++)
                            indices.Add(reader.ReadUInt16());
                        continue;
                    }

                    for (int x = 0; x < submesh.VertexCount; ++x)
                    {
                        for (int y = 0; y < submesh.VertexFormatFull.Attributes[i].Count; ++y)
                        {
                            VertexFormat.Attribute attr = submesh.VertexFormatFull.Attributes[i][y];
                            Vector4 v = ReadVertexData(reader, attr.Type);

                            switch (attr.Usage)
                            {
                                case VertexFormat.Usage.Position:
                                    vertices.Add(new Point3D(v.X * submesh.VertexScale, v.Y * submesh.VertexScale, -v.Z * submesh.VertexScale));
                                    break;
                                case VertexFormat.Usage.TexCoord:
                                    if (attr.Index >= uvs.Length)
                                        Array.Resize(ref uvs, attr.Index + 1);
                                    if (uvs[attr.Index] == null)
                                        uvs[attr.Index] = new PointCollection();
                                    uvs[attr.Index].Add(new System.Windows.Point(v.X * 16.0f, v.Y * 16.0f));
                                    break;
                                //TODO: support more data
                            }
                        }
                    }
                    Utilities.Align(reader, 16);
                }
            }

            if (vertices.Count == 0) return new GeometryModel3D();

            Int32Collection reversedIndices = new Int32Collection();
            for (int i = 0; i < indices.Count; i += 3)
            {
                if (i + 2 < indices.Count)
                {
                    reversedIndices.Add(indices[i]);
                    reversedIndices.Add(indices[i + 2]);
                    reversedIndices.Add(indices[i + 1]);
                }
            }

            PointCollection uv = new PointCollection();
            for (int i = 0; i < uvs.Length; i++)
            {
                if (uvs[i] != null)
                {
                    uv = uvs[i];
                    break;
                }
            }

            MeshGeometry3D geometry = new MeshGeometry3D
            {
                Positions = vertices,
                TriangleIndices = reversedIndices,
                TextureCoordinates = uv,
            };
            return new GeometryModel3D
            {
                Geometry = geometry,
                Material = new DiffuseMaterial(new SolidColorBrush(Color.FromRgb(255, 255, 0))),
                BackMaterial = new DiffuseMaterial(new SolidColorBrush(Color.FromRgb(255, 255, 0)))
            };
        }

        private static Vector4 ReadVertexData(BinaryReader reader, VertexFormat.Type type)
        {
            switch (type)
            {
                case VertexFormat.Type.FP32_1:
                    return new Vector4(reader.ReadSingle(), 0, 0, 0);
                case VertexFormat.Type.FP32_2:
                    return new Vector4(reader.ReadSingle(), reader.ReadSingle(), 0, 0);
                case VertexFormat.Type.FP32_3:
                    return new Vector4(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle(), 0);
                case VertexFormat.Type.FP32_4:
                    return new Vector4(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());
                case VertexFormat.Type.Color:
                    uint data = reader.ReadUInt32();
                    return new Vector4((float)((data & 0xFF000000) >> 24) / 255.0f, (float)((data & 0x00FF0000) >> 16) / 255.0f, (float)((data & 0x0000FF00) >> 8) / 255.0f, (float)((data & 0x000000FF) >> 0) / 255.0f);
                case VertexFormat.Type.U8_4:
                    return new Vector4(reader.ReadByte(), reader.ReadByte(), reader.ReadByte(), reader.ReadByte());
                case VertexFormat.Type.S16_2:
                    return new Vector4(reader.ReadInt16(), reader.ReadInt16(), 0, 0);
                case VertexFormat.Type.S16_4:
                    return new Vector4(reader.ReadInt16(), reader.ReadInt16(), reader.ReadInt16(), reader.ReadInt16());
                case VertexFormat.Type.U8_4N:
                    return new Vector4((float)reader.ReadByte() / 255.0f, (float)reader.ReadByte() / 255.0f, (float)reader.ReadByte() / 255.0f, (float)reader.ReadByte() / 255.0f);
                case VertexFormat.Type.S16_2N:
                    return new Vector4((float)reader.ReadInt16() / (float)Int16.MaxValue, (float)reader.ReadInt16() / (float)Int16.MaxValue, 0, 0);
                case VertexFormat.Type.S16_4N:
                    return new Vector4((float)reader.ReadInt16() / (float)Int16.MaxValue, (float)reader.ReadInt16() / (float)Int16.MaxValue, (float)reader.ReadInt16() / (float)Int16.MaxValue, (float)reader.ReadInt16() / (float)Int16.MaxValue);
                case VertexFormat.Type.U16_2N:
                    return new Vector4((float)reader.ReadUInt16() / (float)UInt16.MaxValue, (float)reader.ReadUInt16() / (float)UInt16.MaxValue, 0, 0);
                case VertexFormat.Type.U16_4N:
                    return new Vector4((float)reader.ReadUInt16() / (float)UInt16.MaxValue, (float)reader.ReadUInt16() / (float)UInt16.MaxValue, (float)reader.ReadUInt16() / (float)UInt16.MaxValue, (float)reader.ReadUInt16() / (float)UInt16.MaxValue);
                case VertexFormat.Type.Dec3N:
                    uint val = reader.ReadUInt32();
                    short sx = (short)((val >> 20) & 0x3ff);
                    short sy = (short)((val >> 10) & 0x3ff);
                    short sz = (short)((val) & 0x3ff);
                    return new Vector4(((sx < 512) ? sx : (sx - 1024)) / 511.0f, ((sy < 512) ? sy : (sy - 1024)) / 511.0f, ((sz < 512) ? sz : (sz - 1024)) / 511.0f, 0);
            }
            throw new Exception("Unsupported VertexFormatType");
        }

        public static Assimp.Material ToAssimpMaterial(this Materials.Material cathodeMaterial, int materialIndex, string diffuseTextureFileName = null)
        {
            Assimp.Material mat = new Assimp.Material();
            if (cathodeMaterial == null) return mat;

            mat.Name = cathodeMaterial.Name;
            float r, g, b;
            MaterialApplier.GetDiffuseTintForExport(cathodeMaterial, out r, out g, out b);
            mat.ColorDiffuse = new Assimp.Color4D(r, g, b, 1.0f);
            if (!string.IsNullOrEmpty(diffuseTextureFileName))
            {
                Assimp.TextureSlot slot = new Assimp.TextureSlot();
                slot.FilePath = diffuseTextureFileName;
                slot.TextureType = Assimp.TextureType.Diffuse;
                slot.TextureIndex = 0;
                mat.AddMaterialTexture(slot);
            }
            return mat;
        }

        public static Mesh ToMesh(this CS2.Component.LOD.Submesh submesh, int materialIndex = 0)
        {
            cMesh cathodeMesh = ModelUtility.ToMesh(submesh);
            Mesh assimpMesh = new Mesh();
            assimpMesh.MaterialIndex = materialIndex;

            if (!assimpMesh.SetIndices(cathodeMesh.Indices.Select(x => (int)x).ToArray(), 3))
            {
                return assimpMesh;
            }

            for (int i = 0; i < cathodeMesh.Vertices.Count; i++)
            {
                assimpMesh.Vertices.Add(new Assimp.Vector3D((float)cathodeMesh.Vertices[i].X, (float)cathodeMesh.Vertices[i].Y, (float)cathodeMesh.Vertices[i].Z));
            }
            for (int i = 0; i < cathodeMesh.Normals.Count; i++)
            {
                assimpMesh.Normals.Add(new Assimp.Vector3D((float)cathodeMesh.Normals[i].X, (float)cathodeMesh.Normals[i].Y, (float)cathodeMesh.Normals[i].Z));
            }
            //binormals?
            for (int i = 0; i < cathodeMesh.Tangents.Count; i++)
            {
                assimpMesh.Tangents.Add(new Assimp.Vector3D((float)cathodeMesh.Tangents[i].X, (float)cathodeMesh.Tangents[i].Y, (float)cathodeMesh.Tangents[i].Z));
            }
            int exportedUVs = 0;
            for (int i = 0; i < cathodeMesh.UVs.Length; i++)
            {
                if (cathodeMesh.UVs[i] == null) continue;

                for (int x = 0; x < cathodeMesh.UVs[i].Count; x++)
                {
                    assimpMesh.TextureCoordinateChannels[exportedUVs].Add(new Assimp.Vector3D((float)cathodeMesh.UVs[i][x].X, (float)cathodeMesh.UVs[i][x].Y, 0));
                }
                assimpMesh.HasTextureCoords(exportedUVs);
                assimpMesh.UVComponentCount[exportedUVs] = 2;
                exportedUVs++;
            }
            return assimpMesh;
        }

        public static void ExportMesh(this Models.CS2 cs2, string filename)
        {
            string modelDir = Path.GetDirectoryName(filename);
            string modelBase = Path.GetFileNameWithoutExtension(filename);
            if (string.IsNullOrEmpty(modelBase)) modelBase = cs2.Name ?? "model";

            List<Materials.Material> materials = new List<Materials.Material>();
            Dictionary<Materials.Material, int> materialIndexes = new Dictionary<Materials.Material, int>();
            foreach (var component in cs2.Components)
            {
                foreach (var lod in component.LODs)
                {
                    foreach (var submesh in lod.Submeshes)
                    {
                        if (submesh.Material != null && !materialIndexes.ContainsKey(submesh.Material))
                        {
                            materialIndexes[submesh.Material] = materials.Count;
                            materials.Add(submesh.Material);
                        }
                    }
                }
            }

            Directory.CreateDirectory(Path.Combine(modelDir, modelBase + " Textures"));
            string[] diffuseFileNames = new string[materials.Count];
            for (int i = 0; i < materials.Count; i++)
            {
                Materials.Material mat = materials[i];
                Textures.TEX4 diffuseTex = MaterialApplier.GetDiffuseTexture(mat);
                if (diffuseTex != null)
                {
                    byte[] dds = diffuseTex.ToDDS();
                    if (dds != null && dds.Length > 0)
                    {
                        string ddsFileName = modelBase + " Textures/" + i + "_" + Path.GetFileNameWithoutExtension(diffuseTex.Name) + ".dds";
                        diffuseFileNames[i] = ddsFileName;
                        File.WriteAllBytes(Path.Combine(modelDir, ddsFileName), dds);
                    }
                }
            }

            Scene scene = new Scene();
            for (int matIdx = 0; matIdx < materials.Count; matIdx++)
                scene.Materials.Add(materials[matIdx].ToAssimpMaterial(matIdx, diffuseFileNames[matIdx]));
            if (scene.Materials.Count == 0)
                scene.Materials.Add(new Assimp.Material());

            scene.RootNode = new Node(cs2.Name);
            for (int i = 0; i < cs2.Components.Count; i++)
            {
                Node componentNode = new Node(i.ToString());
                scene.RootNode.Children.Add(componentNode);
                for (int x = 0; x < cs2.Components[i].LODs.Count; x++)
                {
                    Node lodNode = new Node(cs2.Components[i].LODs[x].Name);
                    componentNode.Children.Add(lodNode);
                    for (int y = 0; y < cs2.Components[i].LODs[x].Submeshes.Count; y++)
                    {
                        Node submeshNode = new Node(y.ToString());
                        lodNode.Children.Add(submeshNode);
                        Materials.Material submeshMat = cs2.Components[i].LODs[x].Submeshes[y].Material;
                        int meshMatIndex = (submeshMat != null && materialIndexes.ContainsKey(submeshMat)) ? materialIndexes[submeshMat] : 0;
                        Mesh mesh = cs2.Components[i].LODs[x].Submeshes[y].ToMesh(meshMatIndex);
                        mesh.Name = cs2.Name + " [" + x + "] -> " + lodNode.Name + " [" + i + "]";
                        scene.Meshes.Add(mesh);
                        submeshNode.MeshIndices.Add(scene.Meshes.Count - 1);
                    }
                }
            }

            using (AssimpContext exp = new AssimpContext())
            {
                exp.ExportFile(scene, filename, Path.GetExtension(filename).TrimStart('.').ToLowerInvariant());
            }
        }

        public static CS2.Component.LOD.Submesh ToSubmesh(this Mesh mesh, ushort? customScaleFactor = null)
        {
            //We can't have more vertices than Int16.MaxValue as we won't be able to point to them
            if (mesh.VertexCount > Int16.MaxValue) return null;

            //All faces must be triangulated
            foreach (Assimp.Face face in mesh.Faces) if (face.Indices.Count != 3) return null;

            //Mesh must have some content
            if (mesh.BoundingBox.Max == new Assimp.Vector3D(0,0,0)) return null;

            CS2.Component.LOD.Submesh submesh = new CS2.Component.LOD.Submesh();
            submesh.VertexCount = mesh.VertexCount;
            int[] indices = mesh.GetIndices();
            submesh.IndexCount = indices.Length;

            //meshes must not exceed 1 unit in any direction -> TODO: we should validate customScaleFactor here...
            submesh.VertexScale = customScaleFactor == null ? mesh.CalculateScaleFactor() : (ushort)customScaleFactor;

            submesh.MaxBounds = new Vector3(mesh.BoundingBox.Max.X, mesh.BoundingBox.Max.Y, mesh.BoundingBox.Max.Z);
            submesh.MinBounds = new Vector3(mesh.BoundingBox.Min.X, mesh.BoundingBox.Min.Y, mesh.BoundingBox.Min.Z);

            //Example vertex format with vertices, UVs, normals, and a colour
            //submesh.VertexFormat.Elements.Add(new List<AlienVBF.Element>() { new AlienVBF.Element(VBFE_InputType.VECTOR4_INT16_DIVMAX, VBFE_InputSlot.VERTEX), new AlienVBF.Element(VBFE_InputType.VECTOR2_INT16_DIV2048, VBFE_InputSlot.UV), new AlienVBF.Element(VBFE_InputType.INT32, VBFE_InputSlot.COLOUR) });
            //submesh.VertexFormat.Elements.Add(new List<AlienVBF.Element>() { new AlienVBF.Element(VBFE_InputType.VECTOR3, VBFE_InputSlot.NORMAL) });
            //submesh.VertexFormat.Elements.Add(new List<AlienVBF.Element>() { new AlienVBF.Element(VBFE_InputType.AlienVertexInputType_u16) });

            //Example vertex format with just vertices
            //submesh.VertexFormat.Elements.Add(new List<AlienVBF.Element>() { new AlienVBF.Element(VBFE_InputType.VECTOR4_INT16_DIVMAX, VBFE_InputSlot.VERTEX) });
            //submesh.VertexFormat.Elements.Add(new List<AlienVBF.Element>() { new AlienVBF.Element(VBFE_InputType.AlienVertexInputType_u16) });

            submesh.VertexFormatFull = new VertexFormat();
            submesh.VertexFormatFull.Attributes.Add(new List<VertexFormat.Attribute>() { new VertexFormat.Attribute(VertexFormat.Type.S16_4N, VertexFormat.Usage.Position), new VertexFormat.Attribute(VertexFormat.Type.S16_2N, VertexFormat.Usage.TexCoord) });
            submesh.VertexFormatFull.Attributes.Add(new List<VertexFormat.Attribute>() { new VertexFormat.Attribute(VertexFormat.Type.FP32_3, VertexFormat.Usage.Normal) });
            submesh.VertexFormatFull.Attributes.Add(new List<VertexFormat.Attribute>() { new VertexFormat.Attribute(VertexFormat.Type.Unused) }); 

            submesh.VertexFormatPartial = new VertexFormat();
            submesh.VertexFormatPartial.Attributes.Add(new List<VertexFormat.Attribute>() { new VertexFormat.Attribute(VertexFormat.Type.S16_4N, VertexFormat.Usage.Position), new VertexFormat.Attribute(VertexFormat.Type.S16_2N, VertexFormat.Usage.TexCoord) });
            submesh.VertexFormatPartial.Attributes.Add(new List<VertexFormat.Attribute>() { new VertexFormat.Attribute(VertexFormat.Type.Unused) }); 

            MemoryStream ms = new MemoryStream();
            using (BinaryWriter reader = new BinaryWriter(ms))
            {
                for (int i = 0; i < submesh.VertexFormatFull.Attributes.Count; ++i)
                {
                    if (i == submesh.VertexFormatFull.Attributes.Count - 1)
                    {
                        for (int x = 0; x < indices.Length; x++)
                            reader.Write((UInt16)indices[x]);

                        Utilities.Align(reader, 16);
                        continue;
                    }

                    //TEMP!! This should be reworked to the new logic

                    for (int x = 0; x < submesh.VertexCount; ++x)
                    {
                        for (int y = 0; y < submesh.VertexFormatFull.Attributes[i].Count; ++y)
                        {
                            VertexFormat.Attribute format = submesh.VertexFormatFull.Attributes[i][y];
                            switch (format.Type)
                            {
                                case VertexFormat.Type.FP32_3:
                                    {
                                        switch (format.Usage)
                                        {
                                            case VertexFormat.Usage.Normal:
                                                reader.Write((float)mesh.Normals[x].X);
                                                reader.Write((float)mesh.Normals[x].Y);
                                                reader.Write((float)mesh.Normals[x].Z);
                                                break;
                                        };
                                        break;
                                    }
                                case VertexFormat.Type.S16_2N:
                                    {
                                        switch (format.Usage)
                                        {
                                            case VertexFormat.Usage.TexCoord:
                                                Vector2 v = new Vector2(mesh.TextureCoordinateChannels[format.Index][x].X, mesh.TextureCoordinateChannels[format.Index][x].Y);
                                                v *= 2048.0f;
                                                reader.Write((Int16)v.X);
                                                reader.Write((Int16)v.Y);
                                                break;
                                        }
                                        break;
                                    }
                                case VertexFormat.Type.S16_4N:
                                    {
                                        switch (format.Usage)
                                        {
                                            case VertexFormat.Usage.Position:
                                                Vector4 v = new Vector4(mesh.Vertices[x].X, mesh.Vertices[x].Y, mesh.Vertices[x].Z, 0);
                                                v /= submesh.VertexScale;
                                                v *= (float)Int16.MaxValue;
                                                reader.Write((Int16)v.X);
                                                reader.Write((Int16)v.Y);
                                                reader.Write((Int16)v.Z);
                                                reader.Write((Int16)v.W); //-1,0,1
                                                break;
                                        }
                                        break;
                                    }
                            }
                        }
                    }
                    Utilities.Align(reader, 16);
                }
            }
            submesh.Data = ms.ToArray();

            return submesh;
        }

        public static ushort CalculateScaleFactor(this Mesh mesh)
        {
            float x = Math.Max(Math.Abs(mesh.BoundingBox.Min.X), Math.Abs(mesh.BoundingBox.Max.X));
            float y = Math.Max(Math.Abs(mesh.BoundingBox.Min.Y), Math.Abs(mesh.BoundingBox.Max.Y));
            float z = Math.Max(Math.Abs(mesh.BoundingBox.Min.Z), Math.Abs(mesh.BoundingBox.Max.Z));
            ushort scaleFactor = 1;
            int i = 1;
            while (true)
            {
                if (x / (float)scaleFactor < 0.99f && y / (float)scaleFactor < 0.99f && z / (float)scaleFactor < 0.99f) break;
                if (i == 1) scaleFactor = 4;
                else scaleFactor = (ushort)(4 * i);
                i++;
            }
            return scaleFactor;
        }

        public static string ForceStringNumeric(this string str, bool allowDots = false)
        {
            string editedText = "";
            bool hasIncludedDot = false;
            bool hasIncludedMinus = false;
            for (int i = 0; i < str.Length; i++)
            {
                if (Char.IsNumber(str[i]) || (str[i] == '.' && allowDots) || (str[i] == '-'))
                {
                    if (str[i] == '-' && hasIncludedMinus) continue;
                    if (str[i] == '-' && i != 0) continue;
                    if (str[i] == '-') hasIncludedMinus = true;
                    if (str[i] == '.' && hasIncludedDot) continue;
                    if (str[i] == '.') hasIncludedDot = true;
                    editedText += str[i];
                }
            }
            if (editedText == "") editedText = "0";
            if (editedText == "-") editedText = "-0";
            if (editedText == ".") editedText = "0";
            return editedText;
        }
    }
}
