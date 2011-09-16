﻿using System;
using S33M3Engines;
using S33M3Engines.Cameras;
using S33M3Engines.D3D;
using S33M3Engines.InputHandler;
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

namespace Utopia.Editor
{
    public class EntityEditor : DrawableGameComponent, IDebugInfo
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

        private const float Scale = 1;//1f/16f;

        private Location3<int>? _prevPickedBlock;
        public ShaderResourceView _texture; //it's a field for being able to dispose the resource

        private Tool _leftToolbeforeEnteringEditor;

        private Location3<int>? _newCubePlace;
        private Location3<int>? _pickedBlock;

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


        private void UnprojectMouseCursor(out DVector3 MouseWorldPosition, out DVector3 MouseLookAt)
        {
            //Get mouse Position on the screen
            var mouseState = Mouse.GetState();

            Vector3 nearClipVector = new Vector3(mouseState.X, mouseState.Y, 0);
            Vector3 farClipVector = new Vector3(mouseState.X, mouseState.Y, 1);

            Matrix cameraWVP = Matrix.Translation(-_camManager.ActiveCamera.WorldPosition.AsVector3()) *
                               Matrix.RotationQuaternion(_camManager.ActiveCamera.Orientation) *
                               _camManager.ActiveCamera.ViewProjection3D;

            Vector3 UnprojecNearClipVector;
            Vector3.Unproject(ref nearClipVector,
                              _d3DEngine.ViewPort.TopLeftX,
                              _d3DEngine.ViewPort.TopLeftY,
                              _d3DEngine.ViewPort.Width,
                              _d3DEngine.ViewPort.Height,
                              _d3DEngine.ViewPort.MinDepth,
                              _d3DEngine.ViewPort.MaxDepth,
                              ref cameraWVP,
                              out UnprojecNearClipVector);

            Vector3 UnprojecFarClipVector;
            Vector3.Unproject(ref farClipVector,
                              _d3DEngine.ViewPort.TopLeftX,
                              _d3DEngine.ViewPort.TopLeftY,
                              _d3DEngine.ViewPort.Width,
                              _d3DEngine.ViewPort.Height,
                              _d3DEngine.ViewPort.MinDepth,
                              _d3DEngine.ViewPort.MaxDepth,
                              ref cameraWVP,
                              out UnprojecFarClipVector);

            //To apply From Camera Position !
            MouseWorldPosition = new DVector3(UnprojecNearClipVector);
            MouseLookAt = new DVector3(Vector3.Normalize(UnprojecFarClipVector - UnprojecNearClipVector));
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

            //HandleInput();


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

            _itemEffect.CBPerFrame.Values.ViewProjection = Matrix.Transpose(_camManager.ActiveCamera.ViewProjection3D);
            _itemEffect.CBPerFrame.Values.SunColor = Vector3.One;
            _itemEffect.CBPerFrame.Values.dayTime = 0.5f;
            _itemEffect.CBPerFrame.Values.fogdist = 100; //TODO FOGDIST in editor

            _itemEffect.CBPerFrame.IsDirty = true;

            //Matrix world = Matrix.Scaling(1f/16f)*
            //               Matrix.Translation(_editedEntity.Position.AsVector3());
            Matrix world = Matrix.Translation(_editedEntity.Position.AsVector3());

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


                int x = _prevPickedBlock.Value.X;
                int y = _prevPickedBlock.Value.Y;
                int z = _prevPickedBlock.Value.Z;

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

        DVector3 CastedFrom, CastedTo;
        private void GetSelectedBlock()
        {
            FirstPersonCamera cam = (FirstPersonCamera) _camManager.ActiveCamera;
            byte[,,] blocks = _editedEntity.VoxelEntity.Blocks;

            DVector3 mouseWorldPosition, mouseLookAt;
            UnprojectMouseCursor(out mouseWorldPosition, out mouseLookAt);
            //Create a ray from MouseWorldPosition to a specific size (That we will increment) and then check if we intersect an existing cube !

            for (float x = 0.5f; x < 10f; x += 0.1f)
            {
                DVector3 targetPoint = (mouseWorldPosition + (mouseLookAt * x));// / Scale;
                if (x == 0.5) CastedFrom = targetPoint;
                CastedTo = targetPoint;

                if (targetPoint.X >= _editedEntity.Position.X && targetPoint.Y >= _editedEntity.Position.Y && targetPoint.Z >= _editedEntity.Position.Z &&
                    targetPoint.X < blocks.GetLength(0) + _editedEntity.Position.X && targetPoint.Y < blocks.GetLength(1) + _editedEntity.Position.Y && targetPoint.Z < blocks.GetLength(2) + _editedEntity.Position.Z)
                {
                    _pickedBlock = new Location3<int>((int)(targetPoint.X - mouseWorldPosition.X),
                                                       (int)(targetPoint.Y - mouseWorldPosition.Y),
                                                       (int)(targetPoint.Z - mouseWorldPosition.Z));

                    Console.WriteLine("Selected");
                   break;
                }
            }

            debugdata = "from : " + CastedFrom + " TO : " + CastedTo + " with Direction : " + mouseLookAt + " mouse pointer location : " + mouseWorldPosition;
          
        }

        private DVector3 UnprojectMouse(FirstPersonCamera cam)
        {
            Vector3 mousePos = new Vector3(Mouse.GetState().X,
                                           Mouse.GetState().Y,
                                           cam.Viewport.MinDepth);

            return new DVector3(Vector3.Unproject(mousePos, cam.Viewport.TopLeftX, cam.Viewport.TopLeftY,
                                                  cam.Viewport.Width,
                                                  cam.Viewport.Height, cam.Viewport.MinDepth,
                                                  cam.Viewport.MaxDepth,
                                                  Matrix.Translation(cam.WorldPosition.AsVector3())*
                                                  cam.ViewProjection3D));
        }

        private DVector3 GetLookAt()
        {
            FirstPersonCamera cam = (FirstPersonCamera) _camManager.ActiveCamera;
            Quaternion dir = cam.Orientation;
            Matrix rotation;
            Matrix.RotationQuaternion(ref dir, out rotation);
            //DVector3 xAxis = new DVector3(rotation.M11, rotation.M21, rotation.M31);
            // DVector3 yAxis = new DVector3(rotation.M12, rotation.M22, rotation.M32);
            DVector3 zAxis = new DVector3(rotation.M13, rotation.M23, rotation.M33);

            DVector3 lookAt = new DVector3(-zAxis.X, -zAxis.Y, -zAxis.Z);
            lookAt.Normalize();
            return lookAt;
        }


        private void GetSelectedBlock2()
        {

            DVector3? worldPosition = _editedEntity.Position - _player.Position; 

            Vector3 entityEyeOffset= new Vector3(0, _player.Size.Y / 100 * 80, 0);//unproject here

            DVector3 lookAt = GetLookAt();

            byte[,,] blocks = _editedEntity.VoxelEntity.Blocks;

            DVector3 pickingPointInLine = worldPosition.Value + entityEyeOffset;
            //Sample 500 points in the view direction vector
            for (int ptNbr = 0; ptNbr < 500; ptNbr++)
            {
                pickingPointInLine += lookAt*0.02;
                //pickingPointInLine = pickingPointInLine / Scale;

                int x = MathHelper.Fastfloor(pickingPointInLine.X);
                int y = MathHelper.Fastfloor(pickingPointInLine.Y);
                int z = MathHelper.Fastfloor(pickingPointInLine.Z);

                if (x >= 0 && y >= 0 && z >= 0
                    && x < blocks.GetLength(0) && y < blocks.GetLength(1) && z < blocks.GetLength(2)
                    && _editedEntity.VoxelEntity.Blocks[x, y, z] != 0
                    )
                {
                    _pickedBlock = new Location3<int>(x, y, z);

                    //Find the face picked up !
                    float faceDistance;
                    Ray newRay = new Ray((worldPosition.Value + entityEyeOffset).AsVector3(), lookAt.AsVector3());
                    BoundingBox bBox = new BoundingBox(new Vector3(x, y, z), new Vector3(x + 1, y + 1, z + 1));
                    newRay.Intersects(ref bBox, out faceDistance);

                    DVector3 collisionPoint = worldPosition.Value + entityEyeOffset + (lookAt*faceDistance);
                    MVector3.Round(ref collisionPoint, 4);

                    Location3<int> newCubePlace = new Location3<int>(x, y, z);
                    if (collisionPoint.X == x) newCubePlace.X--;
                    else if (collisionPoint.X == x + 1) newCubePlace.X++;
                    else if (collisionPoint.Y == y) newCubePlace.Y--;
                    else if (collisionPoint.Y == y + 1) newCubePlace.Y++;
                    else if (collisionPoint.Z == z) newCubePlace.Z--;
                    else if (collisionPoint.Z == z + 1) newCubePlace.Z++;
                    _newCubePlace = newCubePlace;

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

        string debugdata;
        public string GetInfo()
        {
            return debugdata;
        }
    }
}