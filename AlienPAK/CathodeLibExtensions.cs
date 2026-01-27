using Assimp.Configs;
using Assimp;
using CATHODE;
using CathodeLib;
using DirectXTex;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using static CATHODE.Materials.Material;
using static CATHODE.Models;
using System.Reflection;
using Vector3D = System.Windows.Media.Media3D.Vector3D;
using System.Windows;
using System.ComponentModel;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Drawing.Imaging;
using PixelFormat = System.Drawing.Imaging.PixelFormat;
using Color = System.Windows.Media.Color;
using System.Windows.Interop;
using static CATHODE.Models.CS2.Component.LOD;

namespace AlienPAK
{
    public static class CathodeLibExtensions
    {
        /* Convert a TEX4 to DDS */
        public static byte[] ToDDS(this Textures.TEX4 texture, bool forceLowRes = false)
        {
            Textures.TEX4.Texture part = texture?.TextureStreamed?.Content != null && !forceLowRes ? texture.TextureStreamed : texture?.TexturePersistent?.Content != null ? texture.TexturePersistent : null;
            if (part == null) return null;
            DirectXTexUtility.DXGIFormat format;
            switch (texture.Format)
            {
                case Textures.TextureFormat.A32R32G32B32F:
                    format = DirectXTexUtility.DXGIFormat.R32G32B32A32FLOAT;
                    break;
                case Textures.TextureFormat.A16R16G16B16:
                    format = DirectXTexUtility.DXGIFormat.R16G16B16A16UNORM;
                    break;
                case Textures.TextureFormat.A8R8G8B8:
                    format = DirectXTexUtility.DXGIFormat.R8G8B8A8UNORM;
                    break;
                case Textures.TextureFormat.X8R8G8B8:
                    format = DirectXTexUtility.DXGIFormat.B8G8R8X8UNORM;
                    break;
                case Textures.TextureFormat.A8:
                    format = DirectXTexUtility.DXGIFormat.A8UNORM;
                    break;
                case Textures.TextureFormat.L8:
                    format = DirectXTexUtility.DXGIFormat.R8UNORM;
                    break;
                case Textures.TextureFormat.DXT1:
                    format = DirectXTexUtility.DXGIFormat.BC1UNORM;
                    break;
                case Textures.TextureFormat.DXT3:
                    format = DirectXTexUtility.DXGIFormat.BC2UNORM;
                    break;
                case Textures.TextureFormat.DXT5:
                    format = DirectXTexUtility.DXGIFormat.BC3UNORM;
                    break;
                case Textures.TextureFormat.DXN:
                    format = DirectXTexUtility.DXGIFormat.BC5UNORM;
                    break;
                case Textures.TextureFormat.A4R4G4B4:
                    format = DirectXTexUtility.DXGIFormat.B4G4R4A4UNORM;
                    break;
                case Textures.TextureFormat.BC6H:
                    format = DirectXTexUtility.DXGIFormat.BC6HUF16;
                    break;
                case Textures.TextureFormat.BC7:
                    format = DirectXTexUtility.DXGIFormat.BC7UNORM;
                    break;
                case Textures.TextureFormat.R16F:
                    format = DirectXTexUtility.DXGIFormat.R16FLOAT;
                    break;
                default:
                    format = DirectXTexUtility.DXGIFormat.UNKNOWN;
                    break;
            }
            DirectXTexUtility.GenerateDDSHeader(
                DirectXTexUtility.GenerateMataData(part.Width, part.Height, part.MipLevels, format, texture.StateFlags.HasFlag(Textures.TextureStateFlag.CUBE)),
                DirectXTexUtility.DDSFlags.FORCEDX10EXT, out DirectXTexUtility.DDSHeader ddsHeader, out DirectXTexUtility.DX10Header dx10Header);
            MemoryStream ms = new MemoryStream();
            using (BinaryWriter bw = new BinaryWriter(ms))
            {
                bw.Write(DirectXTexUtility.EncodeDDSHeader(ddsHeader, dx10Header));
                bw.Write(part.Content);
            }
            return ms.ToArray();
        }

        /* Convert DDS to a TEX4 Part */
        public static Textures.TEX4.Texture ToTEX4Part(this byte[] content, out Textures.TextureFormat format, Textures.TEX4.Texture part = null) //Optionally pass a part to start from
        {
            if (part == null) part = new Textures.TEX4.Texture();
            using (MemoryStream imageStream = new MemoryStream(content))
            using (Pfim.IImage image = Pfim.Pfim.FromStream(imageStream))
            {
                part.Depth = (short)image.BitsPerPixel; // is this right
                part.MipLevels = (short)image.MipMaps.Length;
                part.Width = (short)image.Width;
                part.Height = (short)image.Height;
                byte[] contentTrimmed;
                using (MemoryStream readerStream = new MemoryStream(content))
                using (BinaryReader reader = new BinaryReader(readerStream))
                {
                    reader.BaseStream.Position = 84;
                    if (reader.ReadChar() == 'D' && reader.ReadChar() == 'X' && reader.ReadChar() == '1' && reader.ReadChar() == '0')
                        reader.BaseStream.Position = 148;
                    else
                        reader.BaseStream.Position = 128;
                    contentTrimmed = reader.ReadBytes((int)(reader.BaseStream.Length - reader.BaseStream.Position));
                }
                part.Content = contentTrimmed;
                Pfim.DdsHeaderDxt10 header = ((Pfim.Dds)image).Header10;
                if (header == null)
                {
                    format = (Textures.TextureFormat)0;
                    return null;
                }
                switch (header.DxgiFormat)
                {
                    case Pfim.DxgiFormat.R32G32B32A32_FLOAT:
                        format = Textures.TextureFormat.A32R32G32B32F;
                        break;
                    case Pfim.DxgiFormat.R16G16B16A16_UNORM:
                        format = Textures.TextureFormat.A16R16G16B16;
                        break;
                    case Pfim.DxgiFormat.R8G8B8A8_UNORM:
                        format = Textures.TextureFormat.A8R8G8B8;
                        break;
                    case Pfim.DxgiFormat.B8G8R8X8_UNORM:
                        format = Textures.TextureFormat.X8R8G8B8;
                        break;
                    case Pfim.DxgiFormat.A8_UNORM:
                        format = Textures.TextureFormat.A8;
                        break;
                    case Pfim.DxgiFormat.R8_UNORM:
                        format = Textures.TextureFormat.L8;
                        break;
                    case Pfim.DxgiFormat.BC1_UNORM:
                        format = Textures.TextureFormat.DXT1;
                        break;
                    case Pfim.DxgiFormat.BC2_UNORM:
                        format = Textures.TextureFormat.DXT3;
                        break;
                    case Pfim.DxgiFormat.BC3_UNORM:
                        format = Textures.TextureFormat.DXT5;
                        break;
                    case Pfim.DxgiFormat.BC5_UNORM:
                        format = Textures.TextureFormat.DXN;
                        break;
                    case Pfim.DxgiFormat.B4G4R4A4_UNORM:
                        format = Textures.TextureFormat.A4R4G4B4;
                        break;
                    case Pfim.DxgiFormat.BC6H_UF16:
                        format = Textures.TextureFormat.BC6H;
                        break;
                    case Pfim.DxgiFormat.BC7_UNORM:
                        format = Textures.TextureFormat.BC7;
                        break;
                    case Pfim.DxgiFormat.R16_FLOAT:
                        format = Textures.TextureFormat.R16F;
                        break;
                    default:
                        format = Textures.TextureFormat.BC7;
                        break;
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

        public static Mesh ToMesh(this CS2.Component.LOD.Submesh submesh)
        {
            //TODO: we should just make a generic "to usable data" extension that spits out the verts etc, rather than relying on this - but doing it for testing for now
            GeometryModel3D modelGeo = submesh.ToGeometryModel3D();
            MeshGeometry3D model = (MeshGeometry3D)modelGeo?.Geometry;
            Mesh mesh = new Mesh();
            mesh.MaterialIndex = 0; //todo
            if (model != null)
            {
                for (int i = 0; i < model.Positions.Count; i++)
                    mesh.Vertices.Add(new Assimp.Vector3D((float)model.Positions[i].X, (float)model.Positions[i].Y, (float)model.Positions[i].Z));
                mesh.HasTextureCoords(0);
                for (int i = 0; i < model.TextureCoordinates.Count; i++)
                    mesh.TextureCoordinateChannels[0].Add(new Assimp.Vector3D((float)model.TextureCoordinates[i].X, (float)model.TextureCoordinates[i].Y, 0));
                for (int i = 0; i < model.Normals.Count; i++)
                    mesh.Normals.Add(new Assimp.Vector3D((float)model.Normals[i].X, (float)model.Normals[i].Y, (float)model.Normals[i].Z));
                bool worked = mesh.SetIndices(model.TriangleIndices.ToArray(), 3);
                if (!worked) throw new Exception("oops");
            }
            return mesh;
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
