using S33M3CoreComponents.GUI.Nuclex.Controls;
using S33M3CoreComponents.Maths;
using SharpDX;
using Utopia.Entities.Renderer;
using Utopia.Entities.Voxel;
using Utopia.Resources.Effects.Entities;
using Utopia.Shared.Entities.Concrete;
using Utopia.Shared.Entities.Models;

namespace Utopia.GUI
{
    /// <summary>
    /// Shows a 3d model and allows to rotate
    /// </summary>
    public class ModelControl : Control
    {
        private readonly VoxelModelManager _manager;
        private Vector2 _mousePosition;

        public Matrix AlterTransform { get; set; }

        public VoxelModelInstance ModelInstance { get; set; }

        public VisualVoxelModel VisualVoxelModel { get; set; }

        public HLSLVoxelModel VoxelEffect { get; set; }

        public CubeRenderer CubeRenderer { get; set; }

        public CubeResource SelectedCube { get; set; }

        public Quaternion Rotation { get; set; }

        public bool ManualRotation { get; private set; }

        protected override void OnMousePressed(S33M3CoreComponents.GUI.Nuclex.Input.MouseButtons button)
        {
            ManualRotation = true;
            base.OnMousePressed(button);
        }

        protected override void OnMouseReleased(S33M3CoreComponents.GUI.Nuclex.Input.MouseButtons button)
        {
            ManualRotation = false;
            base.OnMouseReleased(button);
        }

        protected override void OnMouseMoved(float x, float y)
        {
            if (ManualRotation)
            {
                Rotation *= Quaternion.RotationYawPitchRoll(( _mousePosition.X - x ) * 0.01f, 0, 0);
            }

            _mousePosition = new Vector2(x,y);
            base.OnMouseMoved(x, y);
        }

        public ModelControl(VoxelModelManager manager)
        {
            Rotation = Quaternion.Identity;
            _manager = manager;
            AlterTransform = Matrix.RotationX(-MathHelper.Pi / 5);
        }

        public void SetModel(string modelName)
        {
            if (VisualVoxelModel != null && VisualVoxelModel.VoxelModel.Name == modelName)
                return;

            if (!string.IsNullOrEmpty(modelName))
            {
                VisualVoxelModel = _manager.GetModel(modelName);
                ModelInstance = VisualVoxelModel.VoxelModel.CreateInstance();
                SelectedCube = null;
            }
            else
            {
                VisualVoxelModel = null;
                ModelInstance = null;
            }
        }

        public void SetCube(CubeResource cube)
        {
            SetModel(null);

            if (CubeRenderer != null)
            {
                CubeRenderer.PrepareCubeRendering(cube);
                SelectedCube = cube;
            }
        }
    }
}
