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
using Utopia.GUI.D3D;
using Utopia.Shared.Chunks.Entities.Concrete;
using Screen = Nuclex.UserInterface.Screen;

namespace Utopia.Editor
{
    public class EntityEditor : GameComponent
    {
        private readonly Screen _screen;

        private readonly VisualEntity _editedEntity;
        private readonly D3DEngine _d3DEngine;
        private HLSLVertexPositionColor _itemEffect;
        private readonly CameraManager _camManager;
        private readonly WorldFocusManager _worldFocusManager;
        private readonly VoxelMeshFactory _voxelMeshFactory;
        private readonly EntityEditorUi _ui;
        private readonly InputHandlerManager _inputHandler;
        private readonly Hud _hudComponent;

        public EntityEditor(Screen screen, D3DEngine d3DEngine, CameraManager camManager,
                            VoxelMeshFactory voxelMeshFactory, WorldFocusManager worldFocusManager,
                            InputHandlerManager inputHandler, Hud hudComponent)
        {
            _screen = screen;
            _inputHandler = inputHandler;
            _worldFocusManager = worldFocusManager;
            _voxelMeshFactory = voxelMeshFactory;
            _camManager = camManager;
            _d3DEngine = d3DEngine;
            _hudComponent = hudComponent;
            _ui = new EntityEditorUi(this);

            VoxelEntity entity = new EditableVoxelEntity();

            entity.Blocks = new byte[16,16,16];
            entity.BordersFill();
            _editedEntity = new VisualEntity(_voxelMeshFactory, entity);
            _editedEntity.Position = _camManager.ActiveCamera.WorldPosition.AsVector3() + new Vector3(0, 0, 2);

            // inactive by default, use F12 UI to enable :)
            this.CallDraw = false;
            this.CallUpdate = false;
        }

        public override void Initialize()
        {
            base.Initialize();
        }

        public override void LoadContent()
        {
            _itemEffect = new HLSLVertexPositionColor(_d3DEngine, @"D3D/Effects/Basics/VertexPositionColor.hlsl",
                                                      VertexPositionColor.VertexDeclaration);
        }

        public override void Update(ref GameTime timeSpent)
        {
            HandleInput(false);
        }


        public override void DrawDepth1()
        {
        }

        public override void DrawDepth2()
        {
            DrawItems();
        }

        protected override void OnDisable()
        {
            foreach (var control in _ui.Children)
                _screen.Desktop.Children.Remove(control);
            _hudComponent.CallUpdate= true;

        }

        protected override void OnEnable()
        {
            _hudComponent.CallUpdate = false;

            foreach (var control in _ui.Children)
            {
                if (!_screen.Desktop.Children.Contains(control))
                {
                    _screen.Desktop.Children.Add(control);
                }
            }
        }

        private void DrawItems()
        {
            _itemEffect.Begin();

            _itemEffect.CBPerDraw.IsDirty = true;
            _itemEffect.CBPerFrame.Values.View = Matrix.Transpose(_camManager.ActiveCamera.View);
            _itemEffect.CBPerFrame.Values.Projection = Matrix.Transpose(_camManager.ActiveCamera.Projection3D);
            _itemEffect.CBPerFrame.IsDirty = true;
            _itemEffect.Apply();

            Matrix world = Matrix.Scaling(1f/16f)*Matrix.RotationY(MathHelper.PiOver4)*
                           Matrix.Translation(_editedEntity.Position);

            world = _worldFocusManager.CenterOnFocus(ref world);
            _itemEffect.CBPerDraw.Values.World = Matrix.Transpose(world);

            _editedEntity.VertexBuffer.SetToDevice(0);
            _d3DEngine.Context.Draw(_editedEntity.VertexBuffer.VertexCount, 0);
        }

        public void ClearEntity()
        {
        }

        public void LoadEntity()
        {
        }

        public void SaveEntity()
        {
        }

        private bool KeyMBuffer, _lButtonBuffer, RButtonBuffer, WheelForward, WheelBackWard;

        private int _y = 0;

        private void HandleInput(bool bufferMode)
        {
            if ((_inputHandler.PrevKeyboardState.IsKeyUp(Keys.LButton) &&
                 _inputHandler.CurKeyboardState.IsKeyDown(Keys.LButton)) || _lButtonBuffer)
            {
                if (bufferMode)
                {
                    _lButtonBuffer = true;
                    return;
                }
                else _lButtonBuffer = false;


                byte[,,] blocks = _editedEntity.VoxelEntity.Blocks;

                if (_y == blocks.GetLength(1)) _y = 0;

                //just for demo prototype i color a y slice each mouseclick

                for (int x = 0; x < blocks.GetLength(0); x++)
                {
                    for (int z = 0; z < blocks.GetLength(2); z++)
                    {
                        blocks[x, _y, z] = _ui.SelectedColor;
                    }
                }

                _editedEntity.Altered = true;
                _editedEntity.Update();

                _y++;
            }
        }
    }
}