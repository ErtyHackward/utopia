using System.Linq;
using Ninject;
using S33M3CoreComponents.Cameras;
using S33M3CoreComponents.Cameras.Interfaces;
using S33M3DXEngine.Main;
using S33M3DXEngine.RenderStates;
using S33M3Resources.Structs.Vertex;
using SharpDX;
using Utopia.Entities.Voxel;
using Utopia.Resources.Effects.Entities;
using Utopia.Shared.Entities;
using Utopia.Shared.Entities.Dynamic;
using Utopia.Shared.Entities.Interfaces;
using Utopia.Shared.Entities.Models;
using Utopia.Shared.GameDXStates;
using Utopia.Shared.Settings;
using Utopia.Shared.World;

namespace Utopia.Entities.Renderer
{
    /// <summary>
    /// Renders semi-transparent voxel model predicting the entity position
    /// </summary>
    public class GhostedEntityRenderer : DrawableGameComponent
    {
        private readonly IDynamicEntity _playerEntity;
        private readonly VoxelModelManager _voxelModelManager;
        private readonly CameraManager<ICameraFocused> _cameraManager;
        private readonly WorldParameters _worldParameters;
        private readonly EntityFactory _factory;

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
            IDynamicEntity playerEntity, 
            VoxelModelManager voxelModelManager,
            CameraManager<ICameraFocused> cameraManager,
            WorldParameters worldParameters,
            EntityFactory factory
            )
        {
            _playerEntity = playerEntity;
            _voxelModelManager = voxelModelManager;
            _cameraManager = cameraManager;
            _worldParameters = worldParameters;
            _factory = factory;
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

            if (_playerEntity is PlayerCharacter)
                tool = ((PlayerCharacter)_playerEntity).Equipment.RightTool;

            if (_playerEntity is GodEntity)
            {
                var godEntity = (GodEntity)_playerEntity;

                if (godEntity.DesignationBlueprintId != 0)
                {
                    tool = (IItem)_worldParameters.Configuration.BluePrints[godEntity.DesignationBlueprintId];
                    _factory.PrepareEntity(tool);
                }
            }

            if (tool != Tool)
            {
                Tool = tool;
                PrepareModel();
            }
            
            if (Tool == null)
                return;

            var pos = Tool.GetPosition(_playerEntity);

            if (pos.Valid)
                Transform = Matrix.RotationQuaternion(pos.Rotation) * Matrix.Translation(pos.Position.AsVector3());
            else
                Transform = null;


            _alpha += ( _alphaRaise ? 0.4f : -0.4f ) * elapsedTime;

            if (_alpha < 0.2f)
                _alphaRaise = true;
            if (_alpha > 0.5f)
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
                _toolVoxelInstance.LightColor = new Color3(0, 0, 1);
                _toolVoxelInstance.Alpha = _alpha;

                _toolVoxelModel.Draw(context, _voxelModelEffect, _toolVoxelInstance);
            }

            var godEntity = _playerEntity as GodEntity;

            if (godEntity != null)
            {
                RenderStatesRepo.ApplyStates(context, DXStates.Rasters.Default, DXStates.Blenders.Enabled, DXStates.DepthStencils.DepthReadEnabled);
                var faction = _factory.GlobalStateManager.GlobalState.Factions[godEntity.FactionId];

                foreach (var placement in faction.Designations.OfType<PlaceDesignation>())
                {
                    if (placement.ModelInstance == null)
                    {
                        var voxelEntity = (IVoxelEntity)_factory.Config.BluePrints[placement.BlueprintId];

                        if (voxelEntity != null)
                        {
                            var voxelModel = _voxelModelManager.GetModel(voxelEntity.ModelName);

                            if (voxelModel != null)
                            {
                                if (!voxelModel.Initialized)
                                {
                                    voxelModel.BuildMesh();
                                }

                                placement.ModelInstance = voxelModel.VoxelModel.CreateInstance();
                                placement.ModelInstance.SetState(voxelModel.VoxelModel.GetMainState());

                                var pos = placement.Position;
                                placement.ModelInstance.World =  Matrix.Scaling(1f / 16) * Matrix.RotationQuaternion(pos.Rotation) * Matrix.Translation(pos.Position.AsVector3());
                            }
                        }
                    }

                    if (placement.ModelInstance != null)
                    {
                        _voxelModelEffect.Begin(context);
                        _voxelModelEffect.CBPerFrame.Values.ViewProjection = Matrix.Transpose(_cameraManager.ActiveCamera.ViewProjection3D);
                        _voxelModelEffect.CBPerFrame.IsDirty = true;

                        placement.ModelInstance.LightColor = new Color3(0, 0, 1);
                        placement.ModelInstance.Alpha = _alpha;

                        var visualVoxelModel = _voxelModelManager.GetModel(placement.ModelInstance.VoxelModel.Name);
                        visualVoxelModel.Draw(context, _voxelModelEffect, placement.ModelInstance);
                    }
                }
            }
        }
    }
}
