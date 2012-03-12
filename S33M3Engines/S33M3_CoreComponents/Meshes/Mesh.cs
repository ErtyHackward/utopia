using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using S33M3_Resources.Struct.Vertex;

namespace S33M3_CoreComponents.Meshes
{
    public class Mesh
    {
        #region Private variables
        #endregion

        #region Public variables
        public string Name;
        public VertexMesh[] Vertices;
        public ushort[] Indices;
        #endregion

        #region Public Methods
        public Mesh Clone(Dictionary<int, int> NewMaterialMapping = null)
        {
            Mesh clonedMesh = new Mesh();
            clonedMesh.Name = this.Name;
            clonedMesh.Vertices = (VertexMesh[])this.Vertices.Clone();
            clonedMesh.Indices = (ushort[])Indices.Clone();

            if (NewMaterialMapping != null)
            {
                for (int vertexId = 0; vertexId < Vertices.Length; vertexId++)
                {
                    clonedMesh.Vertices[vertexId].TextureCoordinate.Z = NewMaterialMapping[(int)clonedMesh.Vertices[vertexId].TextureCoordinate.Z];
                }
            }
            return clonedMesh;
        }

        public void ChangeMaterialMapping(Dictionary<int, int> NewMaterialMapping)
        {
            if (NewMaterialMapping != null)
            {
                for (int vertexId = 0; vertexId < Vertices.Length; vertexId++)
                {
                    Vertices[vertexId].TextureCoordinate.Z = NewMaterialMapping[(int)Vertices[vertexId].TextureCoordinate.Z];
                }
            }
        }
        #endregion

        #region Private Methods
        #endregion
    }
}
