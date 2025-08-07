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
            Int32Collection indices = new Int32Collection();
            Point3DCollection vertices = new Point3DCollection();
            Vector3DCollection normals = new Vector3DCollection();
            List<Vector4> tangents = new List<Vector4>();
            PointCollection uv0 = new PointCollection();
            PointCollection uv1 = new PointCollection();
            PointCollection uv2 = new PointCollection();
            PointCollection uv3 = new PointCollection();
            PointCollection uv7 = new PointCollection();

            //TODO: implement skeleton lookup for the indexes
            List<Vector4> boneIndex = new List<Vector4>(); //The indexes of 4 bones that affect each vertex
            List<Vector4> boneWeight = new List<Vector4>(); //The weights for each bone

            if (submesh.content.Length == 0)
                return new GeometryModel3D();

            using (BinaryReader reader = new BinaryReader(new MemoryStream(submesh.content)))
            {
                for (int i = 0; i < submesh.VertexFormat.Elements.Count; ++i)
                {
                    if (i == submesh.VertexFormat.Elements.Count - 1)
                    {
                        //TODO: should probably properly verify VariableType here 
                        // if (submesh.VertexFormat.Elements[i].Count != 1 || submesh.VertexFormat.Elements[i][0].VariableType != VBFE_InputType.INDICIES_U16)
                        //     throw new Exception("unexpected format");

                        for (int x = 0; x < submesh.IndexCount; x++)
                            indices.Add(reader.ReadUInt16());

                        continue;
                    }

                    for (int x = 0; x < submesh.VertexCount; ++x)
                    {
                        for (int y = 0; y < submesh.VertexFormat.Elements[i].Count; ++y)
                        {
                            AlienVBF.Element format = submesh.VertexFormat.Elements[i][y];
                            switch (format.VariableType)
                            {
                                case VBFE_InputType.VECTOR3:
                                    { 
                                        Vector3D v = new Vector3D(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());
                                        switch (format.ShaderSlot)
                                        {
                                            case VBFE_InputSlot.NORMAL:
                                                normals.Add(v);
                                                break;
                                            case VBFE_InputSlot.TANGENT:
                                                tangents.Add(new Vector4((float)v.X, (float)v.Y, (float)v.Z, 0));
                                                break;
                                            case VBFE_InputSlot.UV:
                                                //TODO: 3D UVW
                                                break;
                                        };
                                        break;
                                    }
                                case VBFE_InputType.INT32:
                                    {
                                        int v = reader.ReadInt32();
                                        switch (format.ShaderSlot)
                                        {
                                            case VBFE_InputSlot.COLOUR:
                                                //??
                                                break;
                                        }
                                        break;
                                    }
                                case VBFE_InputType.VECTOR4_BYTE:
                                    {
                                        Vector4 v = new Vector4(reader.ReadByte(), reader.ReadByte(), reader.ReadByte(), reader.ReadByte());
                                        switch (format.ShaderSlot)
                                        {
                                            case VBFE_InputSlot.BONE_INDICES:
                                                boneIndex.Add(v);
                                                break;
                                        }
                                        break;
                                    }
                                case VBFE_InputType.VECTOR4_BYTE_DIV255:
                                    {
                                        Vector4 v = new Vector4(reader.ReadByte(), reader.ReadByte(), reader.ReadByte(), reader.ReadByte());
                                        v /= 255.0f;
                                        switch (format.ShaderSlot)
                                        {
                                            case VBFE_InputSlot.BONE_WEIGHTS:
                                                boneWeight.Add(v / (v.X + v.Y + v.Z + v.W));
                                                break;
                                            case VBFE_InputSlot.UV:
                                                uv2.Add(new System.Windows.Point(v.X, v.Y));
                                                uv3.Add(new System.Windows.Point(v.Z, v.W));
                                                break;
                                        }
                                        break;
                                    }
                                case VBFE_InputType.VECTOR2_INT16_DIV2048:
                                    {
                                        System.Windows.Point v = new System.Windows.Point(reader.ReadInt16() / 2048.0f, reader.ReadInt16() / 2048.0f);
                                        switch (format.ShaderSlot)
                                        {
                                            case VBFE_InputSlot.UV:
                                                if (format.VariantIndex == 0) uv0.Add(v);
                                                else if (format.VariantIndex == 1)
                                                {
                                                    // TODO: We can figure this out based on AlienVBFE.
                                                    //Material->Material.Flags |= Material_HasTexCoord1;
                                                    uv1.Add(v);
                                                }
                                                else if (format.VariantIndex == 2) uv2.Add(v);
                                                else if (format.VariantIndex == 3) uv3.Add(v);
                                                else if (format.VariantIndex == 7) uv7.Add(v);
                                                break;
                                        }
                                        break;
                                    }
                                case VBFE_InputType.VECTOR4_INT16_DIVMAX:
                                    {
                                        Vector4 v = new Vector4(reader.ReadInt16(), reader.ReadInt16(), reader.ReadInt16(), reader.ReadInt16());
                                        v /= (float)Int16.MaxValue;
                                        if (v.W != 0 && v.W != -1 && v.W != 1) throw new Exception("Unexpected vert W");
                                        v *= submesh.ScaleFactor; //Account for scale
                                        switch (format.ShaderSlot)
                                        {
                                            case VBFE_InputSlot.VERTEX:
                                                vertices.Add(new Point3D(v.X, v.Y, v.Z));
                                                break;
                                        }
                                        break;
                                    }
                                case VBFE_InputType.VECTOR4_BYTE_NORM:
                                    {
                                        Vector4 v = new Vector4(reader.ReadByte(), reader.ReadByte(), reader.ReadByte(), reader.ReadByte());
                                        v /= (float)byte.MaxValue - 0.5f;
                                        v = Vector4.Normalize(v);
                                        switch (format.ShaderSlot)
                                        {
                                            case VBFE_InputSlot.NORMAL:
                                                normals.Add(new Vector3D(v.X, v.Y, v.Z));
                                                break;
                                            case VBFE_InputSlot.TANGENT:
                                                break;
                                            case VBFE_InputSlot.BITANGENT:
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

            if (vertices.Count == 0) return new GeometryModel3D();

            return new GeometryModel3D
            {
                Geometry = new MeshGeometry3D
                {
                    Positions = vertices,
                    TriangleIndices = indices,
                    Normals = normals,
                    TextureCoordinates = uv0,
                },
                Material = new DiffuseMaterial(new SolidColorBrush(Color.FromRgb(255, 255, 0))),
                BackMaterial = new DiffuseMaterial(new SolidColorBrush(Color.FromRgb(255, 255, 0)))
            };
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
            submesh.ScaleFactor = customScaleFactor == null ? mesh.CalculateScaleFactor() : (ushort)customScaleFactor;

            submesh.AABBMax = new Vector3(mesh.BoundingBox.Max.X, mesh.BoundingBox.Max.Y, mesh.BoundingBox.Max.Z);
            submesh.AABBMin = new Vector3(mesh.BoundingBox.Min.X, mesh.BoundingBox.Min.Y, mesh.BoundingBox.Min.Z);

            //Example vertex format with vertices, UVs, normals, and a colour
            //submesh.VertexFormat.Elements.Add(new List<AlienVBF.Element>() { new AlienVBF.Element(VBFE_InputType.VECTOR4_INT16_DIVMAX, VBFE_InputSlot.VERTEX), new AlienVBF.Element(VBFE_InputType.VECTOR2_INT16_DIV2048, VBFE_InputSlot.UV), new AlienVBF.Element(VBFE_InputType.INT32, VBFE_InputSlot.COLOUR) });
            //submesh.VertexFormat.Elements.Add(new List<AlienVBF.Element>() { new AlienVBF.Element(VBFE_InputType.VECTOR3, VBFE_InputSlot.NORMAL) });
            //submesh.VertexFormat.Elements.Add(new List<AlienVBF.Element>() { new AlienVBF.Element(VBFE_InputType.AlienVertexInputType_u16) });

            //Example vertex format with just vertices
            //submesh.VertexFormat.Elements.Add(new List<AlienVBF.Element>() { new AlienVBF.Element(VBFE_InputType.VECTOR4_INT16_DIVMAX, VBFE_InputSlot.VERTEX) });
            //submesh.VertexFormat.Elements.Add(new List<AlienVBF.Element>() { new AlienVBF.Element(VBFE_InputType.AlienVertexInputType_u16) });

            submesh.VertexFormat = new AlienVBF();
            submesh.VertexFormat.Elements.Add(new List<AlienVBF.Element>() { new AlienVBF.Element(VBFE_InputType.VECTOR4_INT16_DIVMAX, VBFE_InputSlot.VERTEX), new AlienVBF.Element(VBFE_InputType.VECTOR2_INT16_DIV2048, VBFE_InputSlot.UV) { Offset = 8 } });
            submesh.VertexFormat.Elements.Add(new List<AlienVBF.Element>() { new AlienVBF.Element(VBFE_InputType.VECTOR3, VBFE_InputSlot.NORMAL) });
            submesh.VertexFormat.Elements.Add(new List<AlienVBF.Element>() { new AlienVBF.Element(VBFE_InputType.INDICIES_U16) }); 

            submesh.VertexFormatLowDetail = new AlienVBF();
            submesh.VertexFormatLowDetail.Elements.Add(new List<AlienVBF.Element>() { new AlienVBF.Element(VBFE_InputType.VECTOR4_INT16_DIVMAX, VBFE_InputSlot.VERTEX), new AlienVBF.Element(VBFE_InputType.VECTOR2_INT16_DIV2048, VBFE_InputSlot.UV) { Offset = 8 } });
            submesh.VertexFormatLowDetail.Elements.Add(new List<AlienVBF.Element>() { new AlienVBF.Element(VBFE_InputType.INDICIES_U16) }); 

            MemoryStream ms = new MemoryStream();
            using (BinaryWriter reader = new BinaryWriter(ms))
            {
                for (int i = 0; i < submesh.VertexFormat.Elements.Count; ++i)
                {
                    if (i == submesh.VertexFormat.Elements.Count - 1)
                    {
                        for (int x = 0; x < indices.Length; x++)
                            reader.Write((UInt16)indices[x]);

                        Utilities.Align(reader, 16);
                        continue;
                    }

                    for (int x = 0; x < submesh.VertexCount; ++x)
                    {
                        for (int y = 0; y < submesh.VertexFormat.Elements[i].Count; ++y)
                        {
                            AlienVBF.Element format = submesh.VertexFormat.Elements[i][y];
                            switch (format.VariableType)
                            {
                                case VBFE_InputType.VECTOR3:
                                    {
                                        switch (format.ShaderSlot)
                                        {
                                            case VBFE_InputSlot.NORMAL:
                                                reader.Write((float)mesh.Normals[x].X);
                                                reader.Write((float)mesh.Normals[x].Y);
                                                reader.Write((float)mesh.Normals[x].Z);
                                                break;
                                            case VBFE_InputSlot.TANGENT:
                                                //tangents.Add(new Vector4((float)v.X, (float)v.Y, (float)v.Z, 0));
                                                break;
                                            case VBFE_InputSlot.UV:
                                                //TODO: 3D UVW
                                                break;
                                        };
                                        break;
                                    }/*
                                case VBFE_InputType.INT32:
                                    {
                                        int v = reader.ReadInt32();
                                        switch (format.ShaderSlot)
                                        {
                                            case VBFE_InputSlot.COLOUR:
                                                //??
                                                break;
                                        }
                                        break;
                                    }
                                case VBFE_InputType.VECTOR4_BYTE:
                                    {
                                        Vector4 v = new Vector4(reader.ReadByte(), reader.ReadByte(), reader.ReadByte(), reader.ReadByte());
                                        switch (format.ShaderSlot)
                                        {
                                            case VBFE_InputSlot.BONE_INDICES:
                                                boneIndex.Add(v);
                                                break;
                                        }
                                        break;
                                    }
                                case VBFE_InputType.VECTOR4_BYTE_DIV255:
                                    {
                                        Vector4 v = new Vector4(reader.ReadByte(), reader.ReadByte(), reader.ReadByte(), reader.ReadByte());
                                        v /= 255.0f;
                                        switch (format.ShaderSlot)
                                        {
                                            case VBFE_InputSlot.BONE_WEIGHTS:
                                                boneWeight.Add(v / (v.X + v.Y + v.Z + v.W));
                                                break;
                                            case VBFE_InputSlot.UV:
                                                uv2.Add(new System.Windows.Point(v.X, v.Y));
                                                uv3.Add(new System.Windows.Point(v.Z, v.W));
                                                break;
                                        }
                                        break;
                                    }
                                */
                                case VBFE_InputType.VECTOR2_INT16_DIV2048:
                                    {
                                        switch (format.ShaderSlot)
                                        {
                                            case VBFE_InputSlot.UV:
                                                Vector2 v = new Vector2(mesh.TextureCoordinateChannels[format.VariantIndex][x].X, mesh.TextureCoordinateChannels[format.VariantIndex][x].Y);
                                                v *= 2048.0f;

                                                if (v.X > Int16.MaxValue || v.Y > Int16.MaxValue)
                                                {
                                                    string sdfdf = "";
                                                }

                                                reader.Write((Int16)v.X);
                                                reader.Write((Int16)v.Y);
                                                break;
                                        }
                                        break;
                                    }
                                case VBFE_InputType.VECTOR4_INT16_DIVMAX:
                                    {
                                        switch (format.ShaderSlot)
                                        {
                                            case VBFE_InputSlot.VERTEX:
                                                Vector4 v = new Vector4(mesh.Vertices[x].X, mesh.Vertices[x].Y, mesh.Vertices[x].Z, 0);
                                                v /= submesh.ScaleFactor;
                                                v *= (float)Int16.MaxValue;
                                                reader.Write((Int16)v.X);
                                                reader.Write((Int16)v.Y);
                                                reader.Write((Int16)v.Z);
                                                reader.Write((Int16)v.W); //-1,0,1
                                                break;
                                        }
                                        break;
                                    }
                                    /*
                                case VBFE_InputType.VECTOR4_BYTE_NORM:
                                    {
                                        Vector4 v = new Vector4(reader.ReadByte(), reader.ReadByte(), reader.ReadByte(), reader.ReadByte());
                                        v /= (float)byte.MaxValue - 0.5f;
                                        v = Vector4.Normalize(v);
                                        switch (format.ShaderSlot)
                                        {
                                            case VBFE_InputSlot.NORMAL:
                                                normals.Add(new Vector3D(v.X, v.Y, v.Z));
                                                break;
                                            case VBFE_InputSlot.TANGENT:
                                                break;
                                            case VBFE_InputSlot.BITANGENT:
                                                break;
                                        }
                                        break;
                                    }*/
                            }
                        }
                    }
                    Utilities.Align(reader, 16);
                }
            }
            submesh.content = ms.ToArray();
            //submesh.

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
