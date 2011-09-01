using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using S33M3Engines;
using S33M3Engines.Buffers;
using S33M3Engines.Cameras;
using S33M3Engines.D3D;
using S33M3Engines.D3D.Effects.Basics;
using S33M3Engines.InputHandler;
using S33M3Engines.Struct.Vertex;
using S33M3Engines.WorldFocus;
using SharpDX;
using SharpDX.Direct3D;
using SharpDX.Direct3D11;
using Utopia.Entities.Voxel;
using Utopia.Shared.Chunks.Entities.Inventory;
using Utopia.Shared.Chunks.Entities.Inventory.Tools;
using S33M3Engines.Shared.Math;

namespace Utopia.Entities
{
    public class ItemRenderer : GameComponent
    {
        public List<Item> Items = new List<Item>(); //TODO populate/fetch items to render from server 
        public List<VertexBuffer<VertexPositionColor>> ItemVBs = new List<VertexBuffer<VertexPositionColor>>();

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
            Item item = new Shovel(); //just an example, i need a concrete item class ! 

            item.Blocks = new byte[16,16,16];
            item.RandomFill(5);
            Items.Add(item);
            //note render item icons to one texture when player opens inventory. an idea that needs more thinking
        }

        public override void LoadContent()
        {
            _itemEffect = new HLSLVertexPositionColor(_d3DEngine, @"D3D/Effects/Basics/VertexPositionColor.hlsl",
                                                      VertexPositionColor.VertexDeclaration);
        }

        public override void UnloadContent()
        {
            if (_itemEffect != null) _itemEffect.Dispose();
            foreach (var buffer in ItemVBs) if (buffer != null) buffer.Dispose();
            foreach (var item in Items) if (item != null) item.Dispose();
        }

        public override void Update(ref GameTime TimeSpend)
        {
            InputHandler(false);
        }

        private bool _keyInsertBuffer;

        private void InputHandler(bool bufferMode)
        {
            if (_inputHandler.IsKeyPressed(Keys.Insert) || _keyInsertBuffer)
            {
                if (bufferMode)
                {
                    _keyInsertBuffer = true;
                    return;
                }
                else _keyInsertBuffer = false;

                Items[0].Position = _camManager.ActiveCamera.WorldPosition.AsVector3();
                //TODO (team talk) camera double position vs entiy float position
            }
        }

        public override void Interpolation(ref double interpolation_hd, ref float interpolation_ld)
        {
            InputHandler(true);
        }

        public override void DrawDepth0()
        {
        }

        private void DrawItems()
        {
            _itemEffect.Begin();
            
            _itemEffect.CBPerDraw.IsDirty = true;
            _itemEffect.CBPerFrame.Values.View = Matrix.Transpose(_camManager.ActiveCamera.View);
            _itemEffect.CBPerFrame.Values.Projection = Matrix.Transpose(_camManager.ActiveCamera.Projection3D);
            _itemEffect.CBPerFrame.IsDirty = true;
            _itemEffect.Apply();

            for (int i = 0; i < Items.Count; i++)
            {
                var item = Items[i];

                if (item.Altered)
                {
                    List<VertexPositionColor> vertice = _voxelMeshFactory.GenCubesFaces(item.Blocks);
                    VertexBuffer<VertexPositionColor> newVb = new VertexBuffer<VertexPositionColor>(_d3DEngine, vertice.Count,
                                                               VertexPositionColor.VertexDeclaration,
                                                               PrimitiveTopology.TriangleList,
                                                               ResourceUsage.Default, 10);
                    if (vertice.Count != 0)
                    {
                        newVb.SetData(vertice.ToArray());
                    }

                    ItemVBs.Add(newVb);

                    item.Altered = false;
                }

                VertexBuffer<VertexPositionColor> vb = ItemVBs[i];

                Matrix world = Matrix.Scaling(1f / 16f) * Matrix.RotationY(MathHelper.PiOver4) * Matrix.Translation(item.Position);
                world = _worldFocusManager.CenterOnFocus(ref world);
                _itemEffect.CBPerDraw.Values.World = Matrix.Transpose(world);

                vb.SetToDevice(0);
                _d3DEngine.Context.Draw(vb.VertexCount, 0);
            }
        }

        public override void DrawDepth1()
        {
        }

        public override void DrawDepth2()
        {
            DrawItems();
        }

        #endregion

        #region Private Methods

        #endregion
    }
}