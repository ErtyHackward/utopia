

using BEPUphysics.MathExtensions;
namespace BEPUphysics.DataStructures
{
    ///<summary>
    /// Collection of triangle mesh data that directly returns vertices from its vertex buffer instead of transforming them first.
    ///</summary>
    public class StaticMeshData : MeshBoundingBoxTreeData
    {
        ///<summary>
        /// Constructs the triangle mesh data.
        ///</summary>
        ///<param name="vertices">Vertices to use in the data.</param>
        ///<param name="indices">Indices to use in the data.</param>
        public StaticMeshData(Vector3[] vertices, int[] indices)
        {
            Vertices = vertices;
            Indices = indices;
        }


        ///<summary>
        /// Gets the triangle vertex positions at a given index.
        ///</summary>
        ///<param name="triangleIndex">First index of a triangle's vertices in the index buffer.</param>
        ///<param name="v1">First vertex of the triangle.</param>
        ///<param name="v2">Second vertex of the triangle.</param>
        ///<param name="v3">Third vertex of the triangle.</param>
        public override void GetTriangle(int triangleIndex, out Vector3 v1, out Vector3 v2, out Vector3 v3)
        {
            if (byteVertices != null)
            {
                var bv1 = byteVertices[uindices[triangleIndex]].Position;
                var bv2 = byteVertices[uindices[triangleIndex + 1]].Position;
                var bv3 = byteVertices[uindices[triangleIndex + 2]].Position;

                v1 = new Vector3(_globalMove.X + bv1.X, _globalMove.Y + bv1.Y, _globalMove.Z + bv1.Z);
                v2 = new Vector3(_globalMove.X + bv2.X, _globalMove.Y + bv2.Y, _globalMove.Z + bv2.Z);
                v3 = new Vector3(_globalMove.X + bv3.X, _globalMove.Y + bv3.Y, _globalMove.Z + bv3.Z);
            }
            else
            {
                v1 = vertices[uindices[triangleIndex]];
                v2 = vertices[uindices[triangleIndex + 1]];
                v3 = vertices[uindices[triangleIndex + 2]];
            }
        }


        ///<summary>
        /// Gets the position of a vertex in the data.
        ///</summary>
        ///<param name="i">Index of the vertex.</param>
        ///<param name="vertex">Position of the vertex.</param>
        public override void GetVertexPosition(int i, out Vector3 vertex)
        {
            if (byteVertices != null)
            {
                var bv1 = byteVertices[i].Position;
                vertex = new Vector3(_globalMove.X + bv1.X, _globalMove.Y + bv1.Y, _globalMove.Z + bv1.Z);
            }
            else vertex = vertices[i];
        }


    }
}
