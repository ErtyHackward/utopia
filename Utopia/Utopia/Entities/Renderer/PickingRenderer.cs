using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Utopia.Shared.Entities.Interfaces;
using Utopia.Shared.Structs;
using SharpDX;
using Utopia.Entities.Voxel;
using Utopia.Entities.Renderer.Interfaces;
using Utopia.Resources.ModelComp;
using S33M3DXEngine.Main;
using S33M3DXEngine;
using S33M3CoreComponents.WorldFocus;
using S33M3CoreComponents.Cameras;
using S33M3CoreComponents.Cameras.Interfaces;
using S33M3Resources.Effects.Basics;
using S33M3Resources.VertexFormats;
using SharpDX.Direct3D11;
using S33M3Resources.Structs;
using S33M3DXEngine.RenderStates;
using Utopia.Shared.GameDXStates;

namespace Utopia.Entities.Renderer
{
    public class PickingRenderer : DrawableGameComponent, IPickingRenderer
    {
        #region Private Variable
        private Vector3I _pickedUpCube;
        private VisualEntity _pickedEntity;

        private BoundingBox3D _pickedCube;
        private HLSLVertexPositionColor _blockpickedUPEffect;

        private ByteColor _cursorColor = Colors.Red;//Color.FromNonPremultiplied(20,20,20, 255);
        private D3DEngine _engine;
        private WorldFocusManager _focusManager;
        private IDynamicEntity _player;
        private CameraManager<ICameraFocused> _camManager;

        private Vector3 _cubeScaling = new Vector3(1.005f, 1.005f, 1.005f);

        private double _cubeYOffset;
        #endregion

        #region Public Variable
        #endregion

        public PickingRenderer(D3DEngine engine,
                                      WorldFocusManager focusManager,
                                      IDynamicEntity player,
                                      CameraManager<ICameraFocused> camManager)
        {
            _engine = engine;
            _focusManager = focusManager;
            _player = player;
            _camManager = camManager;
            //Change default Draw order to 10000
            this.DrawOrders.UpdateIndex(0, 1001);
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
                Vector3 EntityWorldPosition = _pickedEntity.Entity.Position.AsVector3();
                EntityWorldPosition.Y += _pickedEntity.Entity.Size.Y / 2.0f;
                _pickedCube.Update(EntityWorldPosition, _pickedEntity.Entity.Size);
            }

        }
        #endregion

        #region public methods
        public override void LoadContent(DeviceContext context)
        {
            _blockpickedUPEffect = new HLSLVertexPositionColor(_engine.Device);
            _pickedCube = new BoundingBox3D(_engine, _focusManager, new Vector3(1.000f, 1.000f, 1.000f), _blockpickedUPEffect, _cursorColor);
        }

        public override void Draw(DeviceContext context, int index)
        {
            //Applying Correct Render States
            RenderStatesRepo.ApplyStates(DXStates.Rasters.Default, DXStates.Blenders.Disabled, DXStates.DepthStencils.DepthEnabled);

            if (_player.EntityState.IsBlockPicked || _player.EntityState.IsEntityPicked)
            {
                _pickedCube.Draw(context, _camManager.ActiveCamera);
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
