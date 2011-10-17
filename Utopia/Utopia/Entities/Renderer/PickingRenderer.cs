using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using S33M3Engines.D3D;
using Utopia.Shared.Entities.Interfaces;
using Utopia.Shared.Structs;
using S33M3Engines.D3D.Effects.Basics;
using S33M3Engines;
using S33M3Engines.Struct.Vertex;
using SharpDX;
using S33M3Engines.WorldFocus;
using S33M3Engines.Cameras;
using S33M3Engines.StatesManager;
using Utopia.Entities.Voxel;
using Utopia.Entities.Renderer.Interfaces;
using Utopia.Settings;
using Utopia.Resources.ModelComp;

namespace Utopia.Entities.Renderer
{
    public class PickingRenderer : DrawableGameComponent, IPickingRenderer
    {
        #region Private Variable
        private Vector3I _pickedUpCube;
        private VisualEntity _pickedEntity;

        private BoundingBox _pickedCubeBox;
        private BoundingBox3D _pickedCube;
        private HLSLVertexPositionColor _blockpickedUPEffect;

        private Color _cursorColor = Color.Red; //Color.FromNonPremultiplied(30,30,30, 255);
        private D3DEngine _engine;
        private WorldFocusManager _focusManager;
        private IDynamicEntity _player;
        private CameraManager _camManager;

        private float _cubeYOffset;
        #endregion

        #region Public Variable
        #endregion

        public PickingRenderer(D3DEngine engine,
                                      WorldFocusManager focusManager,
                                      IDynamicEntity player,
                                      CameraManager camManager)
        {
            _engine = engine;
            _focusManager = focusManager;
            _player = player;
            _camManager = camManager;
            //Change default Draw order to 10000
            this.DrawOrders.UpdateIndex(0, 1001);
        }

        public override void Dispose()
        {
        }

        #region private methods
        private void RefreshpickedBoundingBox(bool fromCube)
        {
            if (fromCube)
            {
                _pickedCubeBox = new BoundingBox(new Vector3(_pickedUpCube.X - 0.002f, _pickedUpCube.Y - 0.002f, _pickedUpCube.Z - 0.002f), new Vector3(_pickedUpCube.X + 1.002f, _pickedUpCube.Y + 1.002f - _cubeYOffset, _pickedUpCube.Z + 1.002f));
                if (_cubeYOffset > 0)
                    _pickedCube.Update(ref _pickedCubeBox, _cubeYOffset);
                else
                    _pickedCube.Update(ref _pickedCubeBox);
            }
            else
            {
                _pickedCubeBox = new BoundingBox(new Vector3(_pickedEntity.WorldBBox.Minimum.X, _pickedEntity.WorldBBox.Minimum.Y + 0.002f, _pickedEntity.WorldBBox.Minimum.Z), _pickedEntity.WorldBBox.Maximum);
                _pickedCube.Update(ref _pickedCubeBox, _pickedEntity.Entity.Size);
            }

        }
        #endregion

        #region public methods
        public override void LoadContent()
        {
            _blockpickedUPEffect = new HLSLVertexPositionColor(_engine, @"D3D/Effects/Basics/VertexPositionColor.hlsl", VertexPositionColor.VertexDeclaration);
            _pickedCube = new BoundingBox3D(_engine, _focusManager, new Vector3(1.002f, 1.002f, 1.002f), _blockpickedUPEffect, _cursorColor);
        }

        public override void UnloadContent()
        {
            if (_blockpickedUPEffect != null) _blockpickedUPEffect.Dispose();
            if (_pickedCube != null) _pickedCube.Dispose();
        }

        public override void Draw(int Index)
        {
            //Applying Correct Render States
            StatesRepository.ApplyStates(GameDXStates.DXStates.Rasters.Default, GameDXStates.DXStates.Blenders.Disabled, GameDXStates.DXStates.DepthStencils.DepthEnabled);

            if (_player.EntityState.IsPickingActive)
            {
                _pickedCube.Draw(_camManager.ActiveCamera);
            }
        }

        public void SetPickedBlock(ref Vector3I pickedUpCube, float cubeYOffset)
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
