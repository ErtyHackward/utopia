using System;
using Utopia.Entities.Managers.Interfaces;
using Utopia.Shared.Entities.Dynamic;
using Utopia.Shared.Entities.Interfaces;
using SharpDX;
using Utopia.Entities.Renderer.Interfaces;
using Utopia.Resources.ModelComp;
using S33M3DXEngine.Main;
using S33M3DXEngine;
using S33M3CoreComponents.WorldFocus;
using S33M3CoreComponents.Cameras;
using S33M3CoreComponents.Cameras.Interfaces;
using S33M3Resources.Effects.Basics;
using SharpDX.Direct3D11;
using S33M3Resources.Structs;
using S33M3DXEngine.RenderStates;
using Utopia.Shared.GameDXStates;
using Utopia.Worlds.Chunks;

namespace Utopia.Entities.Renderer
{
    public class PickingRenderer : DrawableGameComponent, IPickingRenderer
    {
        #region Private Variable
        private Vector3I _pickedUpCube;
        private VisualEntity _pickedEntity;

        private BoundingBox3D _pickedCube;
        private BoundingBox3D _selectedBox;
        private HLSLVertexPositionColor _blockpickedUPEffect;

        private ByteColor _cursorColor = new ByteColor(20,20,20, 255);
        private ByteColor _selectedColor = new ByteColor(20, 140, 20, 255);
        private D3DEngine _engine;
        private WorldFocusManager _focusManager;
        private IDynamicEntity _player;
        private CameraManager<ICameraFocused> _camManager;
        private readonly IVisualDynamicEntityManager _dynamicEntityManager;
        private readonly IWorldChunks _worldChunks;

        private Vector3 _cubeScaling = new Vector3(1.005f, 1.005f, 1.005f);

        private double _cubeYOffset;
        #endregion

        public PickingRenderer(D3DEngine engine,
                               WorldFocusManager focusManager,
                               IDynamicEntity player,
                               CameraManager<ICameraFocused> camManager,
                               IVisualDynamicEntityManager dynamicEntityManager,
                               IWorldChunks worldChunks)
        {
            if (dynamicEntityManager == null) throw new ArgumentNullException("dynamicEntityManager");

            _engine = engine;
            _focusManager = focusManager;
            _player = player;
            _camManager = camManager;
            _dynamicEntityManager = dynamicEntityManager;
            _worldChunks = worldChunks;


            //Change default Draw order to 10000
            this.DrawOrders.UpdateIndex(0, 1020);

            this.IsDefferedLoadContent = true;
        }

        public override void BeforeDispose()
        {
            if (_blockpickedUPEffect != null) _blockpickedUPEffect.Dispose();
            if (_pickedCube != null) _pickedCube.Dispose();
        }

        #region private methods
        private void RefreshpickedBoundingBox(bool fromCube)
        {
            if (fromCube)
            {
                _pickedCube.Update(new Vector3(_pickedUpCube.X + 0.5f, (float)(_pickedUpCube.Y + ((1.0f - _cubeYOffset) / 2)), (float)(_pickedUpCube.Z + 0.5f)), _cubeScaling, (float)_cubeYOffset);
            }
            else
            {
                _pickedCube.Update(ref _pickedEntity.WorldBBox);
            }

        }
        #endregion

        #region public methods
        public override void LoadContent(DeviceContext context)
        {
            _blockpickedUPEffect = new HLSLVertexPositionColor(_engine.Device);
            _pickedCube = new BoundingBox3D(_engine, _focusManager, new Vector3(1.000f, 1.000f, 1.000f), _blockpickedUPEffect, _cursorColor);
            _selectedBox = ToDispose(new BoundingBox3D(_engine, _focusManager, new Vector3(1.000f, 1.000f, 1.000f), _blockpickedUPEffect, _selectedColor));
        }

        public override void Draw(DeviceContext context, int index)
        {
            //Applying Correct Render States
            RenderStatesRepo.ApplyStates(context, DXStates.Rasters.Default, DXStates.Blenders.Disabled, DXStates.DepthStencils.DepthReadWriteEnabled);
            
            // draw hover selection
            if (_player.EntityState.IsBlockPicked || _player.EntityState.IsEntityPicked)
            {
                _pickedCube.Draw(context, _camManager.ActiveCamera);
            }

            // there could be selected entities by the player in god mode
            if (_player is GodEntity)
            {
                var focusEntity = _player as GodEntity;

                foreach (var link in focusEntity.SelectedEntities)
                {
                    if (link.IsDynamic)
                    {
                        foreach (var container in _dynamicEntityManager.DynamicEntities)
                        {
                            var entity = container.VisualVoxelEntity.Entity as IDynamicEntity;

                            if (entity.DynamicId == link.DynamicEntityId)
                            {
                                _selectedBox.Update(ref container.VisualVoxelEntity.WorldBBox);
                                _selectedBox.Draw(context, _camManager.ActiveCamera);
                                break;
                            }
                        }
                    }
                    else
                    {
                        var chunk = _worldChunks.GetChunkFromChunkCoord(link.ChunkPosition);
                        
                        var entity = chunk.Entities.Entities[link.Tail[0]];

                        var voxelEntity = entity as IVoxelEntity;
                        var staticEntity = entity as IStaticEntity;

                        var visualEntity = chunk.VisualVoxelEntities[voxelEntity.ModelName].Find(v => (v.Entity as IStaticEntity).StaticId == staticEntity.StaticId);

                        _selectedBox.Update(ref visualEntity.WorldBBox);
                        _selectedBox.Draw(context, _camManager.ActiveCamera);
                    }
                }
                
            }
        }

        public void SetPickedBlock(ref Vector3I pickedUpCube, double cubeYOffset)
        {
            _pickedUpCube = pickedUpCube;
            _cubeYOffset = cubeYOffset;
            RefreshpickedBoundingBox(true);
        }

        public void SetPickedEntity(VisualEntity pickedEntity)
        {
            _pickedEntity = pickedEntity;
            RefreshpickedBoundingBox(false);
        }
        #endregion

    }
}
