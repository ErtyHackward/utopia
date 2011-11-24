using System.Collections.Generic;
using BEPUphysics.MathExtensions;
using S33M3Engines.Struct.Vertex;
using Utopia.Shared.Structs;


namespace BEPUphysics.DataStructures
{
    ///<summary>
    /// Collection of mesh data which transforms its vertices before returning them.
    ///</summary>
    public class TransformableMeshData : MeshBoundingBoxTreeData
    {
        ///<summary>
        /// Constructs the mesh data.
        ///</summary>
        ///<param name="globalMove"></param>
        ///<param name="vertices">Vertices to use in the mesh data.</param>
        ///<param name="indices">Indices to use in the mesh data.</param>
        public TransformableMeshData(Vector3 globalMove, List<VertexCubeSolid> vertices, List<ushort> indices)
        {
            _globalMove = globalMove;
            ByteVertices = vertices;
            uIndices = indices;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="vertices"></param>
        /// <param name="indices"></param>
        public TransformableMeshData(Vector3[] vertices, int[] indices)
        {
            this.vertices = vertices;
            this._indices = indices;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="vertices"></param>
        /// <param name="indices"></param>
        /// <param name="worldTransform"></param>
        public TransformableMeshData(Vector3[] vertices, int[] indices, AffineTransform worldTransform)
        {
            this.worldTransform = worldTransform;
            this.vertices = vertices;
            this._indices = indices;
        }

        ///<summary>
        /// Constructs the mesh data.
        ///</summary>
        ///<param name="globalMove"></param>
        ///<param name="vertices">Vertice sto use in the mesh data.</param>
        ///<param name="indices">Indices to use in the mesh data.</param>
        ///<param name="worldTransform">Transform to apply to vertices before returning their positions.</param>
        public TransformableMeshData(Vector3 globalMove, List<VertexCubeSolid> vertices, List<ushort> indices, AffineTransform worldTransform)
        {
            _globalMove = globalMove;
            this.worldTransform = worldTransform;
            ByteVertices = vertices;
            uIndices = indices;
        }


        internal AffineTransform worldTransform = AffineTransform.Identity;

        ///<summary>
        /// Gets or sets the transform to apply to the vertices before returning their position.
        ///</summary>
        public AffineTransform WorldTransform
        {
            get
            {
                return worldTransform;
            }
            set
            {
                worldTransform = value;
            }
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
            Vector3 tv1;
            Vector3 tv2;
            Vector3 tv3;

            if (byteVertices != null)
            {
                var bv1 = byteVertices[uindices[triangleIndex]].Position;
                var bv2 = byteVertices[uindices[triangleIndex + 1]].Position;
                var bv3 = byteVertices[uindices[triangleIndex + 2]].Position;

                tv1 = new Vector3(_globalMove.X + bv1.X, _globalMove.Y + bv1.Y, _globalMove.Z + bv1.Z);
                tv2 = new Vector3(_globalMove.X + bv2.X, _globalMove.Y + bv2.Y, _globalMove.Z + bv2.Z);
                tv3 = new Vector3(_globalMove.X + bv3.X, _globalMove.Y + bv3.Y, _globalMove.Z + bv3.Z);
            }
            else
            {
                tv1 = vertices[_indices[triangleIndex]];
                tv2 = vertices[_indices[triangleIndex + 1]];
                tv3 = vertices[_indices[triangleIndex + 2]];
            }

            AffineTransform.Transform(ref tv1, ref worldTransform, out v1);
            AffineTransform.Transform(ref tv2, ref worldTransform, out v2);
            AffineTransform.Transform(ref tv3, ref worldTransform, out v3);
        }

        ///<summary>
        /// Gets the position of a vertex in the data.
        ///</summary>
        ///<param name="i">Index of the vertex.</param>
        ///<param name="vertex">Position of the vertex.</param>
        public override void GetVertexPosition(int i, out Vector3 vertex)
        {
            Vector3 v;
            if (byteVertices != null)
            {
                var bv = byteVertices[i].Position;
                v = new Vector3(_globalMove.X + bv.X, _globalMove.Y + bv.Y, _globalMove.Z + bv.Z);
            }
            else v = vertices[i];
            AffineTransform.Transform(ref v, ref worldTransform, out vertex);
        }


    }
}
