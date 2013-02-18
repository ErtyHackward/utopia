using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using S33M3CoreComponents.Models.ModelMesh;
using S33M3Resources.VertexFormats.Interfaces;
using SharpDX.Direct3D11;

namespace S33M3CoreComponents.Models
{
    /// <summary>
    /// Class containing all data needed to draw a model
    /// </summary>
    public class Model<VertexFormat, IndexFormat> : IModel
        where VertexFormat : IModelMeshComponents
        where IndexFormat : struct
    {
        #region Private Variables
        #endregion

        #region Public Properties
        public string Name { get; set; }
        public string ModelFilePath { get; set; }
        public IModelMesh[] Meshes { get; set; }
        #endregion

        #region Public Methods
        #endregion

        #region Private Methods
        #endregion
    }

    public interface IModel
    {
        string Name { get; set; }
        string ModelFilePath { get; set; }
        IModelMesh[] Meshes { get; set; }
    }
}
