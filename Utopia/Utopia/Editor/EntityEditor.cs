using System;
using System.Windows.Forms;
using S33M3Engines;
using S33M3Engines.Cameras;
using S33M3Engines.D3D;
using S33M3Engines.D3D.Effects.Basics;
using S33M3Engines.InputHandler;
using S33M3Engines.Maths;
using S33M3Engines.Shared.Math;
using S33M3Engines.Struct.Vertex;
using S33M3Engines.Textures;
using S33M3Engines.WorldFocus;
using SharpDX;
using SharpDX.Direct3D11;
using Utopia.Entities.Voxel;
using Utopia.GUI.D3D;
using Utopia.Shared.Chunks.Entities.Concrete;
using Utopia.Shared.Structs;
using Utopia.Shared.World;
using UtopiaContent.Effects.Terran;
using Screen = Nuclex.UserInterface.Screen;
using Utopia.Action;
using S33M3Engines.StatesManager;

namespace Utopia.Editor
{
    public class EntityEditor : DrawableGameComponent
    {
        private readonly Screen _screen;

        private readonly VisualEntity _editedEntity;
        private readonly D3DEngine _d3DEngine;
        private HLSLTerran _itemEffect;
        private readonly CameraManager _camManager;
        private readonly WorldFocusManager _worldFocusManager;
        private readonly VoxelMeshFactory _voxelMeshFactory;
        private readonly EntityEditorUi _ui;
        private readonly ActionsManager _actions;
        private readonly Hud _hudComponent;

        private const float _scale = 1f/16f;

        public EntityEditor(Screen screen, D3DEngine d3DEngine, CameraManager camManager,
                            VoxelMeshFactory voxelMeshFactory, WorldFocusManager worldFocusManager,
                            ActionsManager actions, Hud hudComponent)
        {
            _screen = screen;
            _actions = actions;
            _worldFocusManager = worldFocusManager;
            _voxelMeshFactory = voxelMeshFactory;
            _camManager = camManager;
            _d3DEngine = d3DEngine;
            _hudComponent = hudComponent;
            _ui = new EntityEditorUi(this);

            VoxelEntity entity = new EditableVoxelEntity();

            entity.Blocks = new byte[16,16,16];
            entity.PlainCubeFill();
            _editedEntity = new VisualEntity(_voxelMeshFactory, entity);
            _editedEntity.Position = _camManager.ActiveCamera.WorldPosition.AsVector3() + new Vector3(-1, 0, -3);

            // inactive by default, use F12 UI to enable :)
            this.Visible = false;
            this.Enabled = false;
            DrawOrder = 5000;
        }

        public override void Initialize()
        {
            base.Initialize();
        }

        public override void LoadContent()
        {

            ArrayTexture.CreateTexture2DFromFiles(_d3DEngine.Device, @"Textures/Terran/", @"ct*.png", FilterFlags.Point, out _texture);

            _itemEffect = new HLSLTerran(_d3DEngine, @"Effects/Terran/Terran.hlsl", VertexCubeSolid.VertexDeclaration);

            _itemEffect.TerraTexture.Value = _texture;
            
            _itemEffect.SamplerDiffuse.Value = StatesRepository.GetSamplerState(GameDXStates.DXStates.Samplers.UVWrap_MinMagMipLinear);

            
        }

        public override void Update(ref GameTime timeSpent)
        {
            HandleInput();
            //setSelection();
        }

        public override void Draw()
        {
            DrawItems();
        }

        protected override void OnEnabledChanged(object sender, System.EventArgs args)
        {
            base.OnEnabledChanged(sender, args);

            if (Enabled)
            {
                _hudComponent.Enabled = false;

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
                foreach (var control in _ui.Children)
                    _screen.Desktop.Children.Remove(control);
                _hudComponent.Enabled = true;
            }
        }

        private void DrawItems()
        {
            //Applying Correct Render States
            StatesRepository.ApplyStates(GameDXStates.DXStates.Rasters.Default, GameDXStates.DXStates.NotSet, GameDXStates.DXStates.DepthStencils.DepthEnabled);

            _itemEffect.Begin();

            _itemEffect.CBPerFrame.Values.ViewProjection = Matrix.Transpose(_camManager.ActiveCamera.ViewProjection3D);
            _itemEffect.CBPerFrame.Values.SunColor = Vector3.One;
            _itemEffect.CBPerFrame.Values.dayTime = 0.5f;
            _itemEffect.CBPerFrame.Values.fogdist = 100;//TODO FOGDIST in editor
            _itemEffect.CBPerFrame.IsDirty = true;

            Matrix world = Matrix.Scaling(1f/16f)*Matrix.RotationY(MathHelper.PiOver4)*
                           Matrix.Translation(_editedEntity.Position);

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
        private Location3<int> _currentSelectionBlock;
        private DVector3 _currentSelectionWorld;
        private ShaderResourceView _texture;//it's a field for being able to dispose the resource

        private void HandleInput()
        {
            if (_actions.isTriggered(Actions.Block_Add))
            {
                byte[,,] blocks = _editedEntity.VoxelEntity.Blocks;


                int x = _currentSelectionBlock.X;
                int y = _currentSelectionBlock.Y;
                int z = _currentSelectionBlock.Z;

                blocks[x, y, z] = _ui.SelectedColor;


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
        private void setSelection()
        {
            ICamera cam = _camManager.ActiveCamera;
            byte[,,] blocks = _editedEntity.VoxelEntity.Blocks;

             Vector3 mousePos = new Vector3(Mouse.GetState().X,
                               Mouse.GetState().Y,
                               cam.Viewport.MinDepth);

            Vector3 unproject = Vector3.Unproject(mousePos, cam.Viewport.TopLeftX, cam.Viewport.TopLeftY, cam.Viewport.Width,
                              cam.Viewport.Height, cam.Viewport.MinDepth, cam.Viewport.MaxDepth,
                               Matrix.Translation(cam.WorldPosition.AsVector3())*cam.ViewProjection3D);

           // Console.WriteLine(unproject);
            /*
            for (float x = 0.5f; x < 8f; x += 0.1f)
            {
                DVector3 targetPoint = (cam.WorldPosition + (cam.LookAt*x))/_scale;

                int i = (int) (targetPoint.X);
                int j = (int) (targetPoint.Y);
                int k = (int) (targetPoint.Z);
                if (i >= 0 && j >= 0 && k >= 0 && i < blocks.GetLength(0) && j < blocks.GetLength(1) &&
                    k < blocks.GetLength(2))
                {
                    //if _model.blocks[i,j,k]

                    _currentSelectionBlock = new Location3<int>(i, j, k);

                    _currentSelectionWorld = targetPoint;
                    // Debug.WriteLine("{0} -- {1}", _currentSelectionBlock, _currentSelectionWorld);
                    break;
                }
            }*/
        }

        public override void Dispose()
        {
            
            _itemEffect.Dispose();
            _texture.Dispose();

            base.Dispose();
        }
    }
}