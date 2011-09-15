using System;
using S33M3Engines;
using S33M3Engines.Cameras;
using S33M3Engines.D3D;
using S33M3Engines.InputHandler;
using S33M3Engines.Shared.Math;
using S33M3Engines.StatesManager;
using S33M3Engines.Struct.Vertex;
using S33M3Engines.Textures;
using S33M3Engines.WorldFocus;
using SharpDX;
using SharpDX.Direct3D11;
using Utopia.Action;
using Utopia.Entities.Voxel;
using Utopia.GUI.D3D;
using Utopia.Shared.Chunks.Entities;
using Utopia.Shared.Chunks.Entities.Concrete;
using Utopia.Shared.Chunks.Entities.Inventory;
using Utopia.Shared.Structs;
using UtopiaContent.Effects.Terran;
using Screen = Nuclex.UserInterface.Screen;

namespace Utopia.Editor
{
    public class EntityEditor : DrawableGameComponent
    {
        private readonly Screen _screen;

        private VisualEntity _editedEntity;
        private readonly D3DEngine _d3DEngine;
        private HLSLTerran _itemEffect;
        private readonly CameraManager _camManager;
        private readonly WorldFocusManager _worldFocusManager;
        private readonly VoxelMeshFactory _voxelMeshFactory;
        private EntityEditorUi _ui;
        private readonly ActionsManager _actions;
        private readonly Hud _hudComponent;
        private readonly PlayerCharacter _player;

        private const float Scale = 1f/16f;

        private Location3<int>? _currentSelectionBlock;
        private DVector3 _currentSelectionWorld;
        public ShaderResourceView _texture; //it's a field for being able to dispose the resource

        private Tool _leftToolbeforeEnteringEditor;

        public EntityEditor(Screen screen, D3DEngine d3DEngine, CameraManager camManager,
                            VoxelMeshFactory voxelMeshFactory, WorldFocusManager worldFocusManager,
                            ActionsManager actions, Hud hudComponent, PlayerCharacter player)
        {
            _screen = screen;
            _player = player;
            _actions = actions;
            _worldFocusManager = worldFocusManager;
            _voxelMeshFactory = voxelMeshFactory;
            _camManager = camManager;
            _d3DEngine = d3DEngine;
            _hudComponent = hudComponent;

            // inactive by default, use F12 UI to enable :)
            this.Visible = false;
            this.Enabled = false;

            DrawOrders.UpdateIndex(0, 5000);
        }

        public void SpawnEntity()
        {
            VoxelEntity entity = new EditableVoxelEntity();

            entity.Blocks = new byte[16,16,16];
            entity.PlainCubeFill();


            int x = entity.Blocks.GetLength(0);
            int y = entity.Blocks.GetLength(1);
            int z = entity.Blocks.GetLength(2);
            byte[,,] overlays = new byte[x,y,z];
            /*  for (int i = 0; i < 16; i++)
            {
                overlays[i, 8, 0]= 22;
                overlays[8, i, 0]= 22;
                overlays[0, 8, i]=22;
            }*/

            _editedEntity = new VisualEntity(_voxelMeshFactory, entity, overlays);


            _editedEntity.Position = new DVector3((int) _player.Position.X, (int) _player.Position.Y,
                                                  (int) _player.Position.Z);

            /* Matrix worldTranslation = Matrix.Translation(_player.EntityState.PickedBlockPosition.X, _player.EntityState.PickedBlockPosition.Y, _player.EntityState.PickedBlockPosition.Z);
            
            Matrix focused = Matrix.Identity;
            _worldFocusManager.CenterTranslationMatrixOnFocus(ref worldTranslation,ref focused);

            _editedEntity.Position = new DVector3(focused.TranslationVector);
            */
        }

        public override void Initialize()
        {
            base.Initialize();
        }

        public override void LoadContent()
        {
            String[] dirs = new String[] {@"Textures/Terran/", @"Textures/Editor/"};

            ArrayTexture.CreateTexture2DFromFiles(_d3DEngine.Device, dirs, @"ct*.png", FilterFlags.Point, out _texture);

            _itemEffect = new HLSLTerran(_d3DEngine, @"Effects/Terran/TerranEditor.hlsl",
                                         VertexCubeSolid.VertexDeclaration);

            _itemEffect.TerraTexture.Value = _texture;

            _itemEffect.SamplerDiffuse.Value =
                StatesRepository.GetSamplerState(GameDXStates.DXStates.Samplers.UVWrap_MinMagMipLinear);

            _ui = new EntityEditorUi(this);
        }

        public override void Update(ref GameTime timeSpent)
        {
            if (_editedEntity == null) return;

            Location3<int>? selection = SetSelection();
            if (_currentSelectionBlock != null &&
                (selection.HasValue && _currentSelectionBlock.Value != selection.Value))
            {
                int x = _currentSelectionBlock.Value.X;
                int y = _currentSelectionBlock.Value.Y;
                int z = _currentSelectionBlock.Value.Z;
                _editedEntity.AlterOverlay(x, y, z, 22);
                _currentSelectionBlock = selection;
                _editedEntity.Altered = true;
            }

            //HandleInput();
        }

        public override void Draw(int index)
        {
            DrawItems();
        }

        protected override void OnEnabledChanged(object sender, System.EventArgs args)
        {
            base.OnEnabledChanged(sender, args);

            if (Enabled)
            {
                _hudComponent.Enabled = false;
                _leftToolbeforeEnteringEditor = _player.Equipment.LeftTool;
                _player.Equipment.LeftTool = null;
                foreach (var control in _ui.Children)
                {
                    if (!_screen.Desktop.Children.Contains(control))
                    {
                        _screen.Desktop.Children.Add(control);
                    }
                }
            }
            else
            {
                _player.Equipment.LeftTool = _leftToolbeforeEnteringEditor;
                if (_ui != null)
                {
                    foreach (var control in _ui.Children)
                        _screen.Desktop.Children.Remove(control);
                    _hudComponent.Enabled = true;
                }
            }
        }

        private void DrawItems()
        {
            if (_editedEntity == null) return;

            //Applying Correct Render States
            StatesRepository.ApplyStates(GameDXStates.DXStates.Rasters.Default, GameDXStates.DXStates.NotSet,
                                         GameDXStates.DXStates.DepthStencils.DepthEnabled);

            _itemEffect.Begin();

            _itemEffect.CBPerFrame.Values.ViewProjection = Matrix.Transpose(_camManager.ActiveCamera.ViewProjection3D);
            _itemEffect.CBPerFrame.Values.SunColor = Vector3.One;
            _itemEffect.CBPerFrame.Values.dayTime = 0.5f;
            _itemEffect.CBPerFrame.Values.fogdist = 100; //TODO FOGDIST in editor

            _itemEffect.CBPerFrame.IsDirty = true;

            Matrix world = Matrix.Scaling(1f/16f)*
                           Matrix.Translation(_editedEntity.Position.AsVector3());

            world = _worldFocusManager.CenterOnFocus(ref world);

            _itemEffect.CBPerDraw.Values.World = Matrix.Transpose(world);
            _itemEffect.CBPerDraw.Values.popUpYOffset = 0;
            _itemEffect.CBPerDraw.IsDirty = true;
            _itemEffect.Apply();

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

//        private int _y = 0;


        private void HandleInput()
        {
            if (_actions.isTriggered(Actions.Use_Left))
            {
                byte[,,] blocks = _editedEntity.VoxelEntity.Blocks;


                int x = _currentSelectionBlock.Value.X;
                int y = _currentSelectionBlock.Value.Y;
                int z = _currentSelectionBlock.Value.Z;

                blocks[x, y, z] = _ui.SelectedIndex;


                /*               if (_y == blocks.GetLength(1)) _y = 0;

                //just for demo prototype i color a y slice each mouseclick

                for (int x = 0; x < blocks.GetLength(0); x++)
                {
                    for (int z = 0; z < blocks.GetLength(2); z++)
                    {
                        blocks[x, _y, z] = _ui.SelectedColor;
                    }
                }*/

                _editedEntity.Altered = true;
                _editedEntity.Update();

                // _y++;
            }
        }

        //not in use, need to debug the rendering first ! 
        private Location3<int>? SetSelection()
        {
            FirstPersonCamera cam = (FirstPersonCamera) _camManager.ActiveCamera;
            byte[,,] blocks = _editedEntity.VoxelEntity.Blocks;

            Vector3 mousePos = new Vector3(Mouse.GetState().X,
                                           Mouse.GetState().Y,
                                           cam.Viewport.MinDepth);

            DVector3 unproject = new DVector3(Vector3.Unproject(mousePos, cam.Viewport.TopLeftX, cam.Viewport.TopLeftY,
                                                                cam.Viewport.Width,
                                                                cam.Viewport.Height, cam.Viewport.MinDepth,
                                                                cam.Viewport.MaxDepth,
                                                                Matrix.Translation(cam.WorldPosition.AsVector3())*
                                                                cam.ViewProjection3D));

            // Console.WriteLine(unproject);
            Quaternion dir = cam.Orientation;
            Matrix rotation;
            Matrix.RotationQuaternion(ref dir, out rotation);
            //DVector3 xAxis = new DVector3(rotation.M11, rotation.M21, rotation.M31);
            // DVector3 yAxis = new DVector3(rotation.M12, rotation.M22, rotation.M32);
            DVector3 zAxis = new DVector3(rotation.M13, rotation.M23, rotation.M33);

            DVector3 lookAt = new DVector3(-zAxis.X, -zAxis.Y, -zAxis.Z);
            lookAt.Normalize();

            DVector3 pos = cam.WorldPosition - _editedEntity.Position;

            for (float x = 0.5f; x < 8f; x += 0.1f)
            {
                DVector3 targetPoint = (pos + (lookAt*x))/Scale;
                //DVector3 targetPoint = (unproject + (lookAt * x)) / Scale;

                int i = (int) (targetPoint.X);
                int j = (int) (targetPoint.Y);
                int k = (int) (targetPoint.Z);
                if (i >= 0 && j >= 0 && k >= 0 && i < blocks.GetLength(0) && j < blocks.GetLength(1) &&
                    k < blocks.GetLength(2))
                {
                    //if _model.blocks[i,j,k]

                    return new Location3<int>(i, j, k);

                    //_currentSelectionWorld = targetPoint;
                    // Debug.WriteLine("{0} -- {1}", _currentSelectionBlock, _currentSelectionWorld);
                    break;
                }
            }
            return null;
        }

        public override void Dispose()
        {
            _itemEffect.Dispose();
            _texture.Dispose();

            base.Dispose();
        }
    }
}