using S33M3CoreComponents.Cameras;
using S33M3CoreComponents.Cameras.Interfaces;
using S33M3DXEngine.Main;
using S33M3DXEngine.RenderStates;
using S33M3Resources.Structs.Vertex;
using SharpDX;
using Utopia.Entities.Managers;
using Utopia.Entities.Voxel;
using Utopia.Resources.Effects.Entities;
using Utopia.Shared.Entities.Interfaces;
using Utopia.Shared.Entities.Models;
using Utopia.Shared.GameDXStates;
using Utopia.Shared.Settings;

namespace Utopia.Entities.Renderer
{
    /// <summary>
    /// Renders semi-transparent voxel model predicting the entity position
    /// </summary>
    public class GhostedEntityRenderer : DrawableGameComponent
    {
        private readonly PlayerEntityManager _playerEntityManager;
        private readonly VoxelModelManager _voxelModelManager;
        private readonly CameraManager<ICameraFocused> _cameraManager;

        private HLSLVoxelModel _voxelModelEffect;
        private VisualVoxelModel _toolVoxelModel;
        private VoxelModelInstance _toolVoxelInstance;
        private float _alpha = 0.4f;
        private bool _alphaRaise;

        public IItem Tool { get; set; }

        /// <summary>
        /// Gets or sets value indicating if we need to draw ghosted item
        /// </summary>
        public bool Display { get; set; }

        /// <summary>
        /// Gets or sets world item transformation
        /// </summary>
        public Matrix? Transform { get; set; }
       

        public GhostedEntityRenderer(
            PlayerEntityManager playerEntityManager, 
            VoxelModelManager voxelModelManager,
            CameraManager<ICameraFocused> cameraManager
            )
        {
            _playerEntityManager = playerEntityManager;
            _voxelModelManager = voxelModelManager;
            _cameraManager = cameraManager;
            Transform = Matrix.Identity;
            Display = true;

            DrawOrders.UpdateIndex(0, 1070);
        }

        private void PrepareModel()
        {
            var voxelEntity = (IVoxelEntity)Tool;

            if (voxelEntity == null || string.IsNullOrEmpty(voxelEntity.ModelName))
            {
                _toolVoxelModel = null;
                return;
            }

            _toolVoxelModel = _voxelModelManager.GetModel(voxelEntity.ModelName);

            if (_toolVoxelModel != null)
            {
                if (!_toolVoxelModel.Initialized)
                {
                    _toolVoxelModel.BuildMesh();
                }

                _toolVoxelInstance = _toolVoxelModel.VoxelModel.CreateInstance();
                _toolVoxelInstance.SetState(_toolVoxelModel.VoxelModel.GetMainState());
            }
        }

        public override void LoadContent(SharpDX.Direct3D11.DeviceContext context)
        {
            _voxelModelEffect = ToDispose(new HLSLVoxelModel(context.Device, ClientSettings.EffectPack + @"Entities\VoxelModel.hlsl", VertexVoxel.VertexDeclaration));
        }

        public override void VTSUpdate(double interpolationHd, float interpolationLd, float elapsedTime)
        {
            IItem tool = null;

            tool = _playerEntityManager.PlayerCharacter.Equipment.RightTool;

            if (tool != Tool)
            {
                Tool = tool;
                PrepareModel();
            }
            
            if (Tool == null)
                return;

            var pos = Tool.GetPosition(_playerEntityManager.PlayerCharacter);

            if (pos.Valid)
                Transform = Matrix.RotationQuaternion(pos.Rotation) * Matrix.Translation(pos.Position.AsVector3());
            else
                Transform = null;


            _alpha += ( _alphaRaise ? 0.4f : -0.4f ) * elapsedTime;

            if (_alpha < 0.1f)
                _alphaRaise = true;
            if (_alpha > 0.3f)
                _alphaRaise = false;
        }

        public override void Draw(SharpDX.Direct3D11.DeviceContext context, int index)
        {
            if (Display && Transform.HasValue && _toolVoxelModel != null)
            {
                RenderStatesRepo.ApplyStates(context, DXStates.Rasters.Default, DXStates.Blenders.Enabled, DXStates.DepthStencils.DepthReadEnabled);

                _voxelModelEffect.Begin(context);
                _voxelModelEffect.CBPerFrame.Values.ViewProjection = Matrix.Transpose(_cameraManager.ActiveCamera.ViewProjection3D);
                _voxelModelEffect.CBPerFrame.IsDirty = true;

                _toolVoxelInstance.World = Matrix.Scaling(1f / 16) * Transform.Value; 
                _toolVoxelInstance.LightColor = new Color3(0.0f, 0.0f, 1f);
                _toolVoxelInstance.Alpha = _alpha;

                _toolVoxelModel.Draw(context, _voxelModelEffect, _toolVoxelInstance);
            }
        }
    }
}
