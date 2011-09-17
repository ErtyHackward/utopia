using System;
using S33M3Engines;
using S33M3Engines.Cameras;
using S33M3Engines.D3D;
using S33M3Engines.InputHandler;
using S33M3Engines.InputHandler.MouseHelper;
using S33M3Engines.Maths;
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
using S33M3Engines.D3D.DebugTools;
using Utopia.InputManager;

namespace Utopia.Editor
{
    public class EntityEditor : DrawableGameComponent, IDebugInfo
    {
        private readonly Screen _screen;

        private VisualEntity _editedEntity;
        private readonly D3DEngine _d3DEngine;
        private readonly InputsManager _inputManager;
        private HLSLTerran _itemEffect;
        private readonly CameraManager _camManager;
        private readonly WorldFocusManager _worldFocusManager;
        private readonly VoxelMeshFactory _voxelMeshFactory;
        private EntityEditorUi _ui;
        private readonly ActionsManager _actions;
        private readonly Hud _hudComponent;
        private readonly PlayerCharacter _player;


        private const double Scale = 1f/16f;


        private Location3<int>? _prevPickedBlock;
        public ShaderResourceView _texture; //it's a field for being able to dispose the resource

        private Tool _leftToolbeforeEnteringEditor;

        private Location3<int>? _newCubePlace;
        private Location3<int>? _pickedBlock;

        public EntityEditor(Screen screen, D3DEngine d3DEngine, CameraManager camManager,
                            VoxelMeshFactory voxelMeshFactory, WorldFocusManager worldFocusManager,
                            ActionsManager actions, Hud hudComponent, PlayerCharacter player, InputsManager inputsManager)
        {
            _screen = screen;
            _player = player;
            _actions = actions;
            _worldFocusManager = worldFocusManager;
            _voxelMeshFactory = voxelMeshFactory;
            _camManager = camManager;
            _d3DEngine = d3DEngine;
            _hudComponent = hudComponent;
            _inputManager = inputsManager;

            // inactive by default, use F12 UI to enable :)
            Visible = false;
            Enabled = false;

            DrawOrders.UpdateIndex(0, 5000);
        }

        public void SpawnEntity()
        {
            VoxelEntity entity = new EditableVoxelEntity();

            entity.Blocks = new byte[16,16,16];
            entity.PlainCubeFill();

            entity.Blocks[0, 0, 0] = 2;
            entity.Blocks[1, 0, 0] = 3;

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
            String[] dirs = new[] {@"Textures/Terran/", @"Textures/Editor/"};

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

            GetSelectedBlock();

            if (_pickedBlock.HasValue && _pickedBlock != _prevPickedBlock)
            {
                int x = _pickedBlock.Value.X;
                int y = _pickedBlock.Value.Y;
                int z = _pickedBlock.Value.Z;

                _editedEntity.AlterOverlay(x, y, z, 22);
                
                if (_prevPickedBlock.HasValue)
                    _editedEntity.AlterOverlay(_prevPickedBlock.Value.X, _prevPickedBlock.Value.Y, _prevPickedBlock.Value.Z, 0);

                _prevPickedBlock = _pickedBlock;
                _editedEntity.Altered = true;             
            }

            HandleInput();

            _editedEntity.Update();
        }

        public override void Draw(int index)
        {
            DrawItems();
        }

        protected override void OnEnabledChanged(object sender, EventArgs args)
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

            _itemEffect.CBPerFrame.Values.ViewProjection = Matrix.Transpose(_camManager.ActiveCamera.ViewProjection3D_focused);
            _itemEffect.CBPerFrame.Values.SunColor = Vector3.One;
            _itemEffect.CBPerFrame.Values.dayTime = 0.5f;
            _itemEffect.CBPerFrame.Values.fogdist = 100; //TODO FOGDIST in editor

            _itemEffect.CBPerFrame.IsDirty = true;

            Matrix world = Matrix.Scaling((float)Scale)*
                           Matrix.Translation(_editedEntity.Position.AsVector3());
            //Matrix world = Matrix.Translation(_editedEntity.Position.AsVector3());

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

        private void HandleInput()
        {
            if (_actions.isTriggered(Actions.Use_Left))
            {
                byte[,,] blocks = _editedEntity.VoxelEntity.Blocks;

                int x = _pickedBlock.Value.X;
                int y = _pickedBlock.Value.Y;
                int z = _pickedBlock.Value.Z;

                blocks[x, y, z] = _ui.SelectedIndex;

                _editedEntity.Altered = true;
               }
        }

        DVector3 CastedFrom, CastedTo;
        private void GetSelectedBlock()
        {
            byte[,,] blocks = _editedEntity.VoxelEntity.Blocks;

            //XXX avoid unnecessay picking loops. but not just mousestate changes , you can still pick in freelook mode. 

            DVector3 mouseWorldPosition, mouseLookAt;
            _inputManager.UnprojectMouseCursor(out mouseWorldPosition, out mouseLookAt);

            //Create a ray from MouseWorldPosition to a specific size (That we will increment) and then check if we intersect an existing cube !
            int nbrpt = 0;

            double start = 0; //how far you need to be from the edited cube. 0 means you can pick right in your eye (ouch) 
            double end = start + 40; //todo magic number 40 for editor picking, should be related to scale and block array size  

            for (double x = start; x < end; x += 0.08) //for scale=1, was0.5 40 0.1  40 seems a high pick distance ! 
            {
                nbrpt++;
                DVector3 targetPoint = (mouseWorldPosition + (mouseLookAt*x));
              
                if (x == start) CastedFrom = targetPoint;
                CastedTo = targetPoint;

                double startX = _editedEntity.Position.X;
                double startY = _editedEntity.Position.Y;
                double startZ = _editedEntity.Position.Z;

                double endX = blocks.GetLength(0)*Scale + startX;
                double endY = blocks.GetLength(1)*Scale + startY;
                double endZ = blocks.GetLength(2)*Scale + startZ;

                if (targetPoint.X >= startX && targetPoint.Y >= startY && targetPoint.Z >= startZ
                    && targetPoint.X < endX && targetPoint.Y < endY && targetPoint.Z < endZ)
                {
                    _pickedBlock = new Location3<int>((int)((targetPoint.X - startX) / Scale),
                                                       (int)((targetPoint.Y  - startY)/ Scale),
                                                       (int)((targetPoint.Z - startZ) / Scale));
                   break;
                }
            }
        }

       

        public override void Dispose()
        {
            _itemEffect.Dispose();
            _texture.Dispose();

            base.Dispose();
        }

        public string GetInfo()
        {
            return string.Empty;
        }
    }
}