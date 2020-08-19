using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AlienPAK
{
    class CS2
    {
        /* BIN */
        public string Filename = "";
        public string ModelPartName = "";
        public string MaterialName = ""; //Pulled from MTL with MateralLibraryIndex

        public int FilenameOffset = 0;
        public int ModelPartNameOffset = 0;
        public int MaterialLibaryIndex = 0;
        public int BlockSize = 0;
        public int ScaleFactor = 0;
        public int VertCount = 0;
        public int FaceCount = 0;
        public int BoneCount = 0;

        /* PAK */
        public int PakOffset = 0;
        public int PakSize = 0;
    }

    class Vector3
    {
        public Vector3() { }
        public Vector3(float _x, float _y, float _z)
        {
            x = _x; y = _y; z = _z;
        }

        public float x;
        public float y;
        public float z;
    }
    class Vector2
    {
        public Vector2() { }
        public Vector2(float _x, float _y)
        {
            x = _x; y = _y;
        }

        public float x;
        public float y;
    }

    class Vertex
    {
        public Vector3 position = new Vector3();
        public Vector2 coordinate = new Vector2();
        public Vector3 normal = new Vector3();
    }

    class Face
    {
        public List<Vertex> vertices = new List<Vertex>();
    }

    class Submesh
    {
        public List<Face> faces = new List<Face>();
        public string name = "";
    }
}
