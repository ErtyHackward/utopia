using LtreeVisualizer.DataPipe;
using LtreeVisualizer.Shadder;
using S33M3CoreComponents.Cameras;
using S33M3CoreComponents.Cameras.Interfaces;
using S33M3CoreComponents.Maths;
using S33M3DXEngine.Buffers;
using S33M3DXEngine.Main;
using S33M3Resources.Primitives;
using S33M3Resources.Structs;
using S33M3Resources.VertexFormats;
using SharpDX;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Utopia.Shared.LandscapeEntities.Trees;
using Utopia.Shared.Structs.Landscape;
using S33M3CoreComponents.Inputs;
using S33M3CoreComponents.Inputs.Actions;
using S33M3CoreComponents.Inputs.KeyboardHandler;
using System.Windows.Forms;
using S33M3CoreComponents.Sprites2D;
using S33M3DXEngine;
using S33M3DXEngine.RenderStates;
using Utopia.Shared.GameDXStates;

namespace LtreeVisualizer.Components
{
    public class LTreeVisu : DrawableGameComponent
    {
        private TreeBluePrint _newTemplate;
        private TreeLSystem _treeSystem = new TreeLSystem();
        private Vector3[] vertexCube;
        private short[] indicesCube;

        private SpriteRenderer _spriteRenderer;
        private SpriteFont _font;

        private VertexBuffer<VertexHLSLLTree> _vb;
        private IndexBuffer<ushort> _ib;

        private HLSLLTree _shader;

        private List<VertexHLSLLTree> _letreeVertexCollection = new List<VertexHLSLLTree>();
        private List<ushort> _letreeIndexCollection = new List<ushort>();

        private bool _bufferDirty = true;

        private CameraManager<ICamera> _cameraManager;
        private InputsManager _inputManager;
        private D3DEngine _engine;
        public LTreeVisu(D3DEngine engine, CameraManager<ICamera> cameraManager, InputsManager inputManager)
        {
            _cameraManager = cameraManager;
            _inputManager = inputManager;
            _engine = engine;

            _inputManager.ActionsManager.AddActions(new KeyboardTriggeredAction()
            {
                ActionId = Actions.EngineReserved1,
                TriggerType = KeyboardTriggerMode.KeyPressed,
                Binding = new KeyWithModifier() { MainKey = Keys.R }
            });

            _inputManager.ActionsManager.AddActions(new KeyboardTriggeredAction()
            {
                ActionId = Actions.EngineReserved2,
                TriggerType = KeyboardTriggerMode.KeyPressed,
                Binding = new KeyWithModifier() { MainKey = Keys.T }
            });

            _inputManager.MouseManager.IsRunning = true;

            _spriteRenderer = ToDispose(new SpriteRenderer(engine));
            _font = ToDispose(new SpriteFont());
            _font.Initialize("Tahoma", 12f, System.Drawing.FontStyle.Bold, true, engine.Device);
        }

        public override void Initialize()
        {
            //Create the mesh from the result.
            Generator.Cube(1, out vertexCube, out indicesCube);
        }

        public override void LoadContent(SharpDX.Direct3D11.DeviceContext context)
        {
            _shader = new HLSLLTree(context.Device, @"Shadder\LTreeVisu.hlsl", VertexHLSLLTree.VertexDeclaration);

            _vb = new VertexBuffer<VertexHLSLLTree>(context.Device, 16, SharpDX.Direct3D.PrimitiveTopology.TriangleList, "VB", AutoResizePerc: 10);
            _ib = new IndexBuffer<ushort>(context.Device, 32, "IB");
        }

        float rotation = 0;
        FTSValue<Quaternion> rotationQ = new FTSValue<Quaternion>(Quaternion.Identity);
        Matrix MatrixWorldRotation = Matrix.Identity;
        bool _withRotation = true;
        public override void FTSUpdate(GameTime timeSpent)
        {
            if (_inputManager.ActionsManager.isTriggered(Actions.EngineReserved1))
            {
                rotationQ.ValuePrev = rotationQ.Value;
                _withRotation = !_withRotation;
            }

            if (_inputManager.ActionsManager.isTriggered(Actions.EngineReserved2))
            {
                if(_newTemplate != null) RefreshBuffers(_newTemplate);
            }

            if (Pipe.MessagesQueue.Count > 0)
            {
                TreeBluePrint newBluePrint;
                if (Pipe.MessagesQueue.TryDequeue(out newBluePrint))
                {
                    _newTemplate = newBluePrint;
                    RefreshBuffers(_newTemplate);
                }
            }

            if (_withRotation)
            {
                rotation += 0.01f;
            }

            rotationQ.BackUpValue();
            rotationQ.Value = Quaternion.RotationAxis(Vector3.UnitY, rotation);
            if (rotation >= MathHelper.TwoPi) rotation = 0;
            if (rotation <= -MathHelper.TwoPi) rotation = 0;
        }

        public override void VTSUpdate(double interpolationHd, float interpolationLd, float elapsedTime)
        {
            float MouseRotation = 0;
            if (_inputManager.MouseManager.CurMouseState.LeftButton == S33M3CoreComponents.Inputs.MouseHandler.ButtonState.Pressed)
            {
                MouseRotation = (_inputManager.MouseManager.MouseMoveDelta.X / 80.0f);
                rotation += MouseRotation;
            }

            Quaternion.Slerp(ref rotationQ.ValuePrev, ref rotationQ.Value, interpolationLd, out rotationQ.ValueInterp);
        }

        public override void Draw(SharpDX.Direct3D11.DeviceContext context, int index)
        {
            RenderStatesRepo.ApplyStates(context, DXStates.Rasters.Default, DXStates.Blenders.Disabled, DXStates.DepthStencils.DepthReadWriteEnabled);

            if (_bufferDirty)
            {
                _vb.SetData(context, _letreeVertexCollection.ToArray());
                _ib.SetData(context, _letreeIndexCollection.ToArray());

                _bufferDirty = false;
            }

            _shader.Begin(context);
            _shader.CBPerFrame.Values.ViewProjection = Matrix.Transpose(_cameraManager.ActiveCamera.ViewProjection3D);
            _shader.CBPerFrame.IsDirty = true;

            _shader.CBPerDraw.Values.World = Matrix.Transpose(Matrix.RotationQuaternion(rotationQ.ValueInterp));
            _shader.CBPerDraw.IsDirty = true;
            _shader.Apply(context);

            _vb.SetToDevice(context, 0);
            _ib.SetToDevice(context, 0);

            context.DrawIndexed(_ib.IndicesCount, 0, 0);

            //Draw text
            _spriteRenderer.Begin(false, context);

            Vector2 positionText = new Vector2(0, 0);
            ByteColor color = Color.White;

            string info = "[R] Toggle rotation; [T] New generation";
            info += Environment.NewLine + "Blocks quantity : " + _letreeIndexCollection.Count / 36;
            info += Environment.NewLine + "Tree size : " + (MaxtreeSize.X - MintreeSize.X + 1).ToString("X : 00 ") + (MaxtreeSize.Y - MintreeSize.Y + 1).ToString("Y : 00 ") + (MaxtreeSize.Z - MintreeSize.Z + 1).ToString("Z : 00 ");
            if (_newTemplate != null) info += Environment.NewLine + "Axiome : " + _newTemplate.Axiom;

            _spriteRenderer.DrawText(_font, info, ref positionText, ref color, -1, -1, SpriteRenderer.TextFontPosition.RelativeToFontUp);

            _spriteRenderer.End(context);
        }

        Vector3I MaxtreeSize = new Vector3I();
        Vector3I MintreeSize = new Vector3I();
        private void RefreshBuffers(TreeBluePrint treeTemplate)
        {
            FastRandom rnd = new FastRandom();
            FastRandom rndColor = new FastRandom();
            //Generate the list of Tree points.
            List<BlockWithPosition> result = _treeSystem.Generate(rnd.Next(), new S33M3Resources.Structs.Vector3I(), treeTemplate);

            _letreeVertexCollection = new List<VertexHLSLLTree>();
            _letreeIndexCollection = new List<ushort>();

            MaxtreeSize.X = int.MinValue;
            MaxtreeSize.Y = int.MinValue;
            MaxtreeSize.Z = int.MinValue;

            MintreeSize.X = int.MaxValue;
            MintreeSize.Y = int.MaxValue;
            MintreeSize.Z = int.MaxValue;
            //For each block
            foreach (BlockWithPosition block in result)
            {
                float blockShade = rnd.NextFloat(0.8f, 1.0f);
                int vertexOffset = _letreeVertexCollection.Count;
                //Create the 24 vertex + 36 Index data per cube !
                for (int i = 0; i < vertexCube.Length; i++)
                {
                    ByteColor c;
                    if (block.BlockId == treeTemplate.TrunkBlock)
                    {
                        c = Color.Brown;
                    }
                    else
                    {
                        c = Color.Green;
                    }
                    //int blue = c.B + rndColor.Next(-10, 10); if (blue < 0 || blue > 255) blue = c.B; c.B += (byte)blue;
                    //int red = c.R + rndColor.Next(-10, 10); if (red < 0 || red > 255) red = c.R; c.R += (byte)red;
                    //int green = c.G + rndColor.Next(-10, 10); if (green < 0 || green > 255) blue = c.G; c.G += (byte)green;
                    if (block.WorldPosition.X > MaxtreeSize.X) MaxtreeSize.X = block.WorldPosition.X;
                    if (block.WorldPosition.Y > MaxtreeSize.Y) MaxtreeSize.Y = block.WorldPosition.Y;
                    if (block.WorldPosition.Z > MaxtreeSize.Z) MaxtreeSize.Z = block.WorldPosition.Z;
                    if (block.WorldPosition.X < MintreeSize.X) MintreeSize.X = block.WorldPosition.X;
                    if (block.WorldPosition.Y < MintreeSize.Y) MintreeSize.Y = block.WorldPosition.Y;
                    if (block.WorldPosition.Z < MintreeSize.Z) MintreeSize.Z = block.WorldPosition.Z;
                    _letreeVertexCollection.Add(new VertexHLSLLTree(vertexCube[i] + block.WorldPosition, blockShade, c));
                }

                foreach (var index in indicesCube)
                {
                    _letreeIndexCollection.Add((ushort)(index + vertexOffset));
                }

            }
            _bufferDirty = true;

        }
    }
}
