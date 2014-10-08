using Ninject;
using Utopia.Entities.Managers;
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

namespace Utopia.Entities.Renderer
{
    public class PickingRenderer : DrawableGameComponent, IPickingRenderer
    {
        #region Private Variable
        private Vector3I _pickedUpCube;
        private VisualEntity _pickedEntity;

        private BoundingBox3D _pickedCube;
        
        private HLSLVertexPositionColor _blockpickedUPEffect;

        private ByteColor _cursorColor = new ByteColor(20,20,20, 255);
        private ByteColor _selectedColor = new ByteColor(20, 140, 20, 255);
        private D3DEngine _engine;
        private WorldFocusManager _focusManager;
        private PlayerEntityManager _playerEntityManager;
        private CameraManager<ICameraFocused> _camManager;

        private Vector3 _cubeScaling = new Vector3(1.005f, 1.005f, 1.005f);

        private double _cubeYOffset;
        #endregion

        [Inject]
        public PlayerEntityManager PlayerEntityManager
        {
            get { return _playerEntityManager; }
            set { _playerEntityManager = value; }
        }


        public PickingRenderer(D3DEngine engine,
                               WorldFocusManager focusManager,
                               CameraManager<ICameraFocused> camManager)
        {

            _engine = engine;
            _focusManager = focusManager;
            _camManager = camManager;



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
        }

        public override void Draw(DeviceContext context, int index)
        {
            //Applying Correct Render States
            RenderStatesRepo.ApplyStates(context, DXStates.Rasters.Default, DXStates.Blenders.Disabled, DXStates.DepthStencils.DepthReadWriteEnabled);
            
            // draw hover selection
            if (PlayerEntityManager.PlayerCharacter.EntityState.IsBlockPicked || PlayerEntityManager.PlayerCharacter.EntityState.IsEntityPicked)
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
