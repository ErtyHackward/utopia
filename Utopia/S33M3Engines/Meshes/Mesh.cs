using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using S33M3Engines.Struct.Vertex;

namespace S33M3Engines.Meshes
{
    public struct Mesh
    {
        public string Name;
        public VertexMesh[] Vertices;
        public ushort[] Indices;
    }
}
