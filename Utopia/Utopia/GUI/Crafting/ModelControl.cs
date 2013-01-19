using S33M3CoreComponents.GUI.Nuclex.Controls;
using S33M3CoreComponents.Sprites2D;
using SharpDX;
using Utopia.Entities.Voxel;
using Utopia.Resources.Effects.Entities;
using Utopia.Shared.Entities.Models;

namespace Utopia.GUI.Crafting
{
    /// <summary>
    /// Shows a 3d model and allows to rotate
    /// </summary>
    public class ModelControl : Control
    {
        private readonly VoxelModelManager _manager;

        public VoxelModelInstance ModelInstance { get; set; }

        public VisualVoxelModel VisualVoxelModel { get; set; }

        public HLSLVoxelModel VoxelEffect { get; set; }

        public SpriteTexture ModelTexture { get; set; }

        public Quaternion Rotation { get; set; }

        public ModelControl(VoxelModelManager manager)
        {
            Rotation = Quaternion.Identity;
            _manager = manager;
        }

        public void SetModel(string modelName)
        {
            VisualVoxelModel = _manager.GetModel(modelName);
            ModelInstance = VisualVoxelModel.VoxelModel.CreateInstance();
        }
    }
}
