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

namespace AlienPAK
{
    public static class CathodeLibExtensions
    {
        public static byte[] ToDDS(this Textures.TEX4 texture, bool forceLowRes = false)
        {
            Textures.TEX4.Part part = texture?.tex_HighRes?.Content != null && !forceLowRes ? texture.tex_HighRes : texture?.tex_LowRes?.Content != null ? texture.tex_LowRes : null;
            if (part == null) return null;
            DirectXTexUtility.DXGIFormat format;
            switch (texture.Format)
            {
                case Textures.TextureFormat.DXGI_FORMAT_BC5_UNORM:
                    format = DirectXTexUtility.DXGIFormat.BC5UNORM;
                    break;
                case Textures.TextureFormat.DXGI_FORMAT_BC1_UNORM:
                    format = DirectXTexUtility.DXGIFormat.BC1UNORM;
                    break;
                case Textures.TextureFormat.DXGI_FORMAT_BC3_UNORM:
                    format = DirectXTexUtility.DXGIFormat.BC3UNORM;
                    break;
                case Textures.TextureFormat.DXGI_FORMAT_B8G8R8A8_UNORM:
                    format = DirectXTexUtility.DXGIFormat.B8G8R8A8UNORM;
                    break;
                default:
                    format = DirectXTexUtility.DXGIFormat.BC7UNORM;
                    break;
            }
            DirectXTexUtility.GenerateDDSHeader(
                DirectXTexUtility.GenerateMataData(part.Width, part.Height, part.MipLevels, format, texture.Type == Textures.AlienTextureType.ENVIRONMENT_MAP),
                DirectXTexUtility.DDSFlags.FORCEDX10EXT, out DirectXTexUtility.DDSHeader ddsHeader, out DirectXTexUtility.DX10Header dx10Header);
            MemoryStream ms = new MemoryStream();
            using (BinaryWriter bw = new BinaryWriter(ms))
            {
                bw.Write(DirectXTexUtility.EncodeDDSHeader(ddsHeader, dx10Header));
                bw.Write(part.Content);
            }
            return ms.ToArray();
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
                        //TODO: should probably verify VariableType here -----> maybe use the new case i've put in below?
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
                                //case VBFE_InputType.AlienVertexInputType_u16:
                                //    {
                                //        for (int z = 0; z < submesh.IndexCount; z++)
                                //            indices.Add(reader.ReadUInt16());
                                //    }
                                //    break;

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
            MeshGeometry3D model = (MeshGeometry3D)submesh.ToGeometryModel3D().Geometry;
            Mesh mesh = new Mesh();
            mesh.MaterialIndex = 0; //todo
            for (int i = 0; i < model.Positions.Count; i++)
                mesh.Vertices.Add(new Assimp.Vector3D((float)model.Positions[i].X, (float)model.Positions[i].Y, (float)model.Positions[i].Z));
            mesh.HasTextureCoords(0);
            for (int i = 0; i < model.TextureCoordinates.Count; i++)
                mesh.TextureCoordinateChannels[0].Add(new Assimp.Vector3D((float)model.TextureCoordinates[i].X, (float)model.Positions[i].Y, 0));
            for (int i = 0; i < model.Normals.Count; i++)
                mesh.Normals.Add(new Assimp.Vector3D((float)model.Normals[i].X, (float)model.Normals[i].Y, (float)model.Normals[i].Z));
            bool worked = mesh.SetIndices(model.TriangleIndices.ToArray(), 3);
            if (!worked) throw new Exception("oops");
            return mesh;
        }

        public static CS2.Component.LOD.Submesh ToSubmesh(this Scene model_)
        {
            AssimpContext importer = new AssimpContext();
            Scene model = importer.ImportFile("C:\\Users\\mattf\\Documents\\CUBE.fbx", PostProcessPreset.TargetRealTimeMaximumQuality);
            importer.Dispose();


            return null;
        }
    }
}
