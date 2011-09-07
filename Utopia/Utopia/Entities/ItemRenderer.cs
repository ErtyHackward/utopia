﻿using System.Collections.Generic;
using System.Windows.Forms;
using S33M3Engines;
using S33M3Engines.Cameras;
using S33M3Engines.D3D;
using S33M3Engines.D3D.Effects.Basics;
using S33M3Engines.InputHandler;
using S33M3Engines.Shared.Math;
using S33M3Engines.Struct.Vertex;
using S33M3Engines.WorldFocus;
using SharpDX;
using Utopia.Entities.Voxel;
using Utopia.Shared.Chunks.Entities.Inventory;
using Utopia.Shared.Chunks.Entities.Inventory.Tools;

namespace Utopia.Entities
{
    public class ItemRenderer : DrawableGameComponent
    {
        public List<VisualEntity> Items = new List<VisualEntity>(); //TODO populate/fetch items to render from server 

        private HLSLVertexPositionColor _itemEffect;
        private readonly D3DEngine _d3DEngine;
        private readonly WorldFocusManager _worldFocusManager;
        private readonly CameraManager _camManager;
        private readonly InputHandlerManager _inputHandler; //temporary, for spawning item
        private readonly VoxelMeshFactory _voxelMeshFactory;


        public ItemRenderer(D3DEngine d3DEngine, WorldFocusManager worldFocusManager, CameraManager camManager,
                            InputHandlerManager inputHandler, VoxelMeshFactory voxelItem)
        {
            _d3DEngine = d3DEngine;
            _voxelMeshFactory = voxelItem;
            _inputHandler = inputHandler;
            _camManager = camManager;
            _worldFocusManager = worldFocusManager;
        }

        #region Public Methods

        public override void Initialize()
        {
           
            //XXX render item icons to one texture when player opens inventory. an idea that needs more thinking
        }

        public override void LoadContent()
        {
            _itemEffect = new HLSLVertexPositionColor(_d3DEngine, @"D3D/Effects/Basics/VertexPositionColor.hlsl",
                                                      VertexPositionColor.VertexDeclaration);
        }

        public override void UnloadContent()
        {
            if (_itemEffect != null) _itemEffect.Dispose();
            foreach (var item in Items) if (item != null) item.Dispose();
        }

        public override void Update(ref GameTime timeSpent)
        {
            HandleInput(false);
        }

        private bool _keyInsertBuffer;

        private void HandleInput(bool bufferMode)
        {
            if (!_inputHandler.IsKeyPressed(Keys.Insert) && !_keyInsertBuffer) return;
            if (bufferMode)
            {
                _keyInsertBuffer = true;
                return;
            }
            _keyInsertBuffer = false;

            Item item = new Shovel(); //just an example, i need a concrete item class ! 
            item.Blocks = new byte[16, 16, 16];
            //item.RandomFill(5);//would come filled from server
            item.BordersFill();
            item.Position = _camManager.ActiveCamera.WorldPosition.AsVector3();
            
            Items.Add(new VisualEntity(_voxelMeshFactory, item));

            //TODO (team talk) camera double position vs entiy float position
        }

        public override void Interpolation(ref double interpolationHd, ref float interpolationLd)
        {
            HandleInput(true);
        }


        public override void Draw()
        {
            DrawItems();
        }

        #endregion

        #region Private Methods

        private void DrawItems()
        {
            //TODO : frustum culling of items in view

            _itemEffect.Begin();

            _itemEffect.CBPerDraw.IsDirty = true;
            _itemEffect.CBPerFrame.Values.View = Matrix.Transpose(_camManager.ActiveCamera.View);
            _itemEffect.CBPerFrame.Values.Projection = Matrix.Transpose(_camManager.ActiveCamera.Projection3D);
            _itemEffect.CBPerFrame.IsDirty = true;
            _itemEffect.Apply();

            foreach (var item in Items)
            {
                if (item.Altered)
                {
                    item.Update();
                }

                Matrix world = Matrix.Scaling(1f/16f)*Matrix.RotationY(MathHelper.PiOver4)*
                               Matrix.Translation(item.Position);
                world = _worldFocusManager.CenterOnFocus(ref world);
                _itemEffect.CBPerDraw.Values.World = Matrix.Transpose(world);

                item.VertexBuffer.SetToDevice(0);
                _d3DEngine.Context.Draw(item.VertexBuffer.VertexCount, 0);
            }
        }

        #endregion
    }
}