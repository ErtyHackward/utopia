using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using S33M3Resources.VertexFormats.Interfaces;

namespace S33M3CoreComponents.Models.ModelMesh
{
    public interface IModelMeshComponents : IVertexType
    {
        ModelMesh.ModelMeshComponents Components { get; }
    }
}
