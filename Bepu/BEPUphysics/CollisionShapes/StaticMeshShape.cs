using System.Collections.Generic;
using BEPUphysics.MathExtensions;
 
using BEPUphysics.DataStructures;
using S33M3Engines.Struct.Vertex;
using Utopia.Shared.Structs;

namespace BEPUphysics.CollisionShapes
{
    ///<summary>
    /// The local space information needed by a StaticMesh.
    /// Since the hierarchy is in world space and owned by the StaticMesh collidable,
    /// this is a pretty lightweight object.
    ///</summary>
    public class StaticMeshShape : CollisionShape
    {
        TransformableMeshData triangleMeshData;
        ///<summary>
        /// Gets the triangle mesh data composing the StaticMeshShape.
        ///</summary>
        public TransformableMeshData TriangleMeshData
        {
            get
            {
                return triangleMeshData;
            }
        }






        ///<summary>
        /// Constructs a new StaticMeshShape.
        ///</summary>
        ///<param name="vertices">Vertices of the mesh.</param>
        ///<param name="indices">Indices of the mesh.</param>
        ///<param name="worldTransform">World transform to use in the local space data.</param>
        public StaticMeshShape(Vector3 globalMove, List<VertexCubeSolid> vertices, List<ushort> indices, AffineTransform worldTransform)
        {
            triangleMeshData = new TransformableMeshData(globalMove, vertices, indices, worldTransform);
        }

        ///<summary>
        /// Constructs a new StaticMeshShape.
        ///</summary>
        ///<param name="vertices">Vertices of the mesh.</param>
        ///<param name="indices">Indices of the mesh.</param>
        ///<param name="worldTransform">World transform to use in the local space data.</param>
        public StaticMeshShape(Vector3[] vertices, int[] indices, AffineTransform worldTransform)
        {
            triangleMeshData = new TransformableMeshData(vertices, indices, worldTransform);
        }


        ///<summary>
        /// Constructs a new StaticMeshShape.
        ///</summary>
        ///<param name="vertices">Vertices of the mesh.</param>
        ///<param name="indices">Indices of the mesh.</param>
        public StaticMeshShape(Vector3 globalMove, List<VertexCubeSolid> vertices, List<ushort> indices)
        {
            triangleMeshData = new TransformableMeshData(globalMove, vertices, indices);
        }

        ///<summary>
        /// Constructs a new StaticMeshShape.
        ///</summary>
        ///<param name="vertices">Vertices of the mesh.</param>
        ///<param name="indices">Indices of the mesh.</param>
        ///<param name="worldTransform">World transform to use in the local space data.</param>
        public StaticMeshShape(Vector3[] vertices, int[] indices)
        {
            triangleMeshData = new TransformableMeshData(vertices, indices);
        }

    }
}
