using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using S33M3Engines;
using S33M3Engines.Cameras;
using S33M3Engines.D3D;
using S33M3Engines.D3D.Effects.Basics;
using S33M3Engines.InputHandler;
using S33M3Engines.Struct.Vertex;
using S33M3Engines.WorldFocus;
using SharpDX;
using Utopia.Entities.Voxel;

namespace Utopia.Entities
{
    public class EntityRenderer : GameComponent
    {
        #region Private variables

        #endregion

        #region Public variables/properties

        public List<IEntity> Entities = new List<IEntity>();
        private HLSLVertexPositionColor _itemEffect;
        private readonly D3DEngine _d3DEngine;
        private readonly WorldFocusManager _worldFocusManager;
        private readonly CameraManager _camManager;
        private readonly InputHandlerManager _inputHandler; //temporary, for spawning item
        private VoxelMeshFactory _voxelItem;

        #endregion

        public EntityRenderer(D3DEngine d3DEngine, WorldFocusManager worldFocusManager, CameraManager camManager,
                              InputHandlerManager inputHandler)
        {
            _d3DEngine = d3DEngine;
            _inputHandler = inputHandler;
            _camManager = camManager;
            _worldFocusManager = worldFocusManager;
        }

        #region Public Methods

        public override void Initialize()
        {
        }

        public override void LoadContent()
        {
            _itemEffect = new HLSLVertexPositionColor(_d3DEngine, @"D3D/Effects/Basics/VertexPositionColor.hlsl",
                                                      VertexPositionColor.VertexDeclaration);

            for (int i = 0; i < Entities.Count; i++)
            {
                Entities[i].LoadContent();
            }
        }

        public override void UnloadContent()
        {
            for (int i = 0; i < Entities.Count; i++)
            {
                Entities[i].UnloadContent();
            }

            if (_itemEffect != null) _itemEffect.Dispose();
        }

        public override void Update(ref GameTime TimeSpend)
        {
            for (int i = 0; i < Entities.Count; i++)
            {
                Entities[i].Update(ref TimeSpend);
            }


            if (_inputHandler.IsKeyPressed(Keys.Insert))
            {
                _voxelItem = new VoxelMeshFactory(_d3DEngine);
                _voxelItem.Blocks = new byte[16,16,16];
                _voxelItem.RandomFill(5);
                _voxelItem.GenCubesFaces();
             }
        }

        public override void Interpolation(ref double interpolation_hd, ref float interpolation_ld)
        {
            for (int i = 0; i < Entities.Count; i++)
            {
                Entities[i].Interpolation(ref interpolation_hd, ref interpolation_ld);
            }
        }

        public override void DrawDepth0()
        {
            for (int i = 0; i < Entities.Count; i++)
            {
                Entities[i].DrawDepth0();
            }

          
        }

        private void DrawItem()
        {
            Matrix worldFocused = Matrix.Identity;
            Matrix world =Matrix.Translation((float)_camManager.ActiveCamera.WorldPosition.X,
                                              -(float) _camManager.ActiveCamera.WorldPosition.Y,
                                              (float) _camManager.ActiveCamera.WorldPosition.Z);
            
            _worldFocusManager.CenterOnFocus(ref world, ref world);

            _itemEffect.Begin();
            _itemEffect.CBPerDraw.Values.World = Matrix.Transpose(worldFocused);
            _itemEffect.CBPerDraw.IsDirty = true;
            _itemEffect.CBPerFrame.Values.View = Matrix.Transpose(_camManager.ActiveCamera.View);
            _itemEffect.CBPerFrame.Values.Projection = Matrix.Transpose(_camManager.ActiveCamera.Projection3D);
            _itemEffect.CBPerFrame.IsDirty = true;
            _itemEffect.Apply();

            _voxelItem.SendMeshToGraphicCard();
            if (_voxelItem.VertexBuffer != null)
            {
                _voxelItem.VertexBuffer.SetToDevice(0);

                _d3DEngine.Context.Draw(_voxelItem.VertexBuffer.VertexCount, 0);
            }
        }

        public override void DrawDepth1()
        {
            for (int i = 0; i < Entities.Count; i++)
            {
                Entities[i].DrawDepth1();
            }
        }

        public override void DrawDepth2()
        {
            for (int i = 0; i < Entities.Count; i++)
            {
                Entities[i].DrawDepth2();
            }
            if (_voxelItem != null)
                DrawItem();
        }

        #endregion

        #region Private Methods

        #endregion
    }
}