

using System.Collections.Generic;
using BEPUphysics.MathExtensions;
using S33M3Engines.Struct.Vertex;
using Utopia.Shared.Structs;

namespace BEPUphysics.DataStructures
{
    ///<summary>
    /// Superclass of the data used to create triangle mesh bounding box trees.
    ///</summary>
    public abstract class MeshBoundingBoxTreeData
    {
        internal Vector3 _globalMove;

        internal List<ushort> uindices;
        ///<summary>
        /// Gets or sets the indices of the triangle mesh.
        ///</summary>
        public List<ushort> uIndices
        {
            get
            {
                return uindices;
            }
            set
            {
                uindices = value;
            }
        }

        internal List<VertexCubeSolid> byteVertices;
        ///<summary>
        /// Gets or sets the vertices of the triangle mesh.
        ///</summary>
        public List<VertexCubeSolid> ByteVertices
        {
            get
            {
                return byteVertices;
            }
            set
            {
                byteVertices = value;
            }
        }

        internal Vector3[] vertices;
        public Vector3[] Vertices
        {
            get { return vertices; }
            set { vertices = value; }
        }

        internal int[] _indices;
        public int[] Indices
        {
            get { return _indices; }
            set { _indices = value; }
        }

        /// <summary>
        /// Gets the bounding box of an element in the data.
        /// </summary>
        /// <param name="triangleIndex">Index of the triangle in the data.</param>
        /// <param name="boundingBox">Bounding box of the triangle.</param>
        public void GetBoundingBox(int triangleIndex, out BoundingBox boundingBox)
        {
            Vector3 v1, v2, v3;
            GetTriangle(triangleIndex, out v1, out v2, out v3);
            Vector3.Min(ref v1, ref v2, out boundingBox.Min);
            Vector3.Min(ref boundingBox.Min, ref v3, out boundingBox.Min);
            Vector3.Max(ref v1, ref v2, out boundingBox.Max);
            Vector3.Max(ref boundingBox.Max, ref v3, out boundingBox.Max);

        }
        ///<summary>
        /// Gets the triangle vertex positions at a given index.
        ///</summary>
        ///<param name="triangleIndex">First index of a triangle's vertices in the index buffer.</param>
        ///<param name="v1">First vertex of the triangle.</param>
        ///<param name="v2">Second vertex of the triangle.</param>
        ///<param name="v3">Third vertex of the triangle.</param>
        public abstract void GetTriangle(int triangleIndex, out Vector3 v1, out Vector3 v2, out Vector3 v3);
        ///<summary>
        /// Gets the position of a vertex in the data.
        ///</summary>
        ///<param name="i">Index of the vertex.</param>
        ///<param name="vertex">Position of the vertex.</param>
        public abstract void GetVertexPosition(int i, out Vector3 vertex);
    }
}
