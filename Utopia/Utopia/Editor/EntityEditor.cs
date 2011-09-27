﻿using System;
using System.Collections.Generic;
using System.IO;
using S33M3Engines;
using S33M3Engines.Cameras;
using S33M3Engines.D3D;
using S33M3Engines.D3D.DebugTools;
using S33M3Engines.InputHandler;
using S33M3Engines.InputHandler.MouseHelper;
using S33M3Engines.Shared.Math;
using S33M3Engines.StatesManager;
using S33M3Engines.Struct.Vertex;
using S33M3Engines.Textures;
using S33M3Engines.WorldFocus;
using SharpDX;
using SharpDX.Direct3D11;
using Utopia.Action;
using Utopia.Entities;
using Utopia.Entities.Managers.Interfaces;
using Utopia.Entities.Renderer;
using Utopia.Entities.Voxel;
using Utopia.GUI.D3D;
using Utopia.InputManager;
using Utopia.Shared.Chunks.Entities;
using Utopia.Shared.Chunks.Entities.Concrete;
using Utopia.Shared.Chunks.Entities.Interfaces;
using Utopia.Shared.Chunks.Entities.Inventory;
using Utopia.Shared.Structs;
using UtopiaContent.Effects.Terran;
using Screen = Nuclex.UserInterface.Screen;

namespace Utopia.Editor
{
    public class EntityEditor : DrawableGameComponent, IDebugInfo
    {
        private readonly Screen _screen;

        private VisualEntity _editedEntity;
        private readonly D3DEngine _d3DEngine;
        private readonly InputsManager _inputManager;
        private readonly IPickingRenderer _pickingRenderer;
        private readonly IDynamicEntityManager _entityManager;
        private HLSLTerran _itemEffect;
        private readonly CameraManager _camManager;
        private readonly WorldFocusManager _worldFocusManager;
        private readonly VoxelMeshFactory _voxelMeshFactory;
        private EntityEditorUi _ui;
        private readonly ActionsManager _actions;
        private readonly Hud _hudComponent;
        private readonly PlayerCharacter _player;


        private const double Scale = 1f/16f;


       
        public ShaderResourceView Texture; //it's a field for being able to dispose the resource

        private Tool _leftToolbeforeEnteringEditor;

        public Vector3I? NewCubePlace;
        private Vector3I? _prevNewCubePlace;

        public Vector3I? PickedCubeLoc;
        private Vector3I? _prevPickedBlock;

        public List<Vector3I> Selected = new List<Vector3I>();

        public byte SelectedIndex { get; set; }
        public bool IsTexture { get; set; }

        public bool IsColor
        {
            get { return !IsTexture; }
            set { IsTexture = !value; }
        }

        public byte[,,] Blocks
        {
            get { return _editedEntity.VoxelEntity.Model.Blocks; }
        }

        public bool MultiSelectEnabled = false;


        public EditorTool LeftTool;
        public EditorTool RightTool;

        public bool HorizontalSymetryEnabled;
        public bool VerticalSymetryEnabled;

        public EntityEditor(Screen screen, D3DEngine d3DEngine, CameraManager camManager,
                            VoxelMeshFactory voxelMeshFactory, WorldFocusManager worldFocusManager,
                            ActionsManager actions, Hud hudComponent, PlayerCharacter player,
                            InputsManager inputsManager,IPickingRenderer pickingRenderer,IDynamicEntityManager entityManager)
        {
            LeftTool = new EditorRemove(this);
            RightTool = new EditorAdd(this);
            _screen = screen;
            _player = player;
            _actions = actions;
            _worldFocusManager = worldFocusManager;
            _voxelMeshFactory = voxelMeshFactory;
            _camManager = camManager;
            _d3DEngine = d3DEngine;
            _hudComponent = hudComponent;
            _inputManager = inputsManager;
            _pickingRenderer = pickingRenderer;
            _entityManager = entityManager;

            // inactive by default, use F12 UI to enable :)
            _leftToolbeforeEnteringEditor = _player.Equipment.LeftTool;
            Visible = false;
            Enabled = false;

            DrawOrders.UpdateIndex(0, 5000);
        }

        public void SpawnEntity(VoxelEntity entity)
        {
            int x = entity.Model.Blocks.GetLength(0);
            int y = entity.Model.Blocks.GetLength(1);
            int z = entity.Model.Blocks.GetLength(2);
            byte[,,] overlays = new byte[x,y,z];

            if (_player.EntityState.PickedEntityId==0)
            {
                //a terrain block
                Vector3I pos = _player.EntityState.PickedBlockPosition;
                _editedEntity = new VisualEntity(_voxelMeshFactory, entity, overlays, IsColor);
                _editedEntity.Position = new Vector3D(pos.X, pos.Y, pos.Z);
                _pickingRenderer.Enabled = false;
                _pickingRenderer.Visible = false;
            } else
            {
                 Console.WriteLine(@"cant spawn an entity over an entity");
            } 
        }

        internal void EditSelectedEntity()
        {
            if (_player.EntityState.PickedEntityId == 0)
            {
                //picked a terrain block
                Vector3I pos = _player.EntityState.PickedBlockPosition;
            } else
            {
                //picked an entity
                uint id = _player.EntityState.PickedEntityId;
                VoxelEntity voxel = (DynamicEntity)_entityManager.GetEntityById(id);
                _entityManager.RemoveEntityById(id,false);
                //becomes managed by the editor, TODO : should notify server the entity is in a suspended mode !                
                int x = voxel.Model.Blocks.GetLength(0);
                int y = voxel.Model.Blocks.GetLength(1);
                int z = voxel.Model.Blocks.GetLength(2);
                byte[, ,] overlays = new byte[x, y, z];
                _editedEntity = new VisualEntity(_voxelMeshFactory, voxel, overlays, IsColor);

                //after editing whether restore 'voxel' in _entityManager or dispose it at beginning and have server notify it changed 
            }
        }
        public override void LoadContent()
        {
            String[] dirs = new[] {@"Textures/Terran/", @"Textures/Editor/"};

            ArrayTexture.CreateTexture2DFromFiles(_d3DEngine.Device, dirs, @"ct*.png", FilterFlags.Point,
                                                  "ArrayTexture_EntityEditor", out Texture);

            _itemEffect = new HLSLTerran(_d3DEngine, @"Effects/Terran/TerranEditor.hlsl",
                                         VertexCubeSolid.VertexDeclaration);

            _itemEffect.TerraTexture.Value = Texture;

            _itemEffect.SamplerDiffuse.Value =
                StatesRepository.GetSamplerState(GameDXStates.DXStates.Samplers.UVWrap_MinMagMipLinear);

            _ui = new EntityEditorUi(this);
        }

        public override void Update(ref GameTime timeSpent)
        {
            if (_editedEntity == null) return;

            GetSelectedBlock();

            if (!PickedCubeLoc.HasValue && _prevPickedBlock.HasValue)
            {
                _editedEntity.AlterOverlay(_prevPickedBlock.Value, 0);
            }

            if (PickedCubeLoc.HasValue && PickedCubeLoc != _prevPickedBlock)
            {
                int x = PickedCubeLoc.Value.X;
                int y = PickedCubeLoc.Value.Y;
                int z = PickedCubeLoc.Value.Z;

                _editedEntity.AlterOverlay(x, y, z, 21);

                if (MultiSelectEnabled && Mouse.GetState().LeftButton == ButtonState.Pressed)
                {
                    //XXX avoid direct mouseState ButtonState.Pressed in editor ?
                    Selected.Add(PickedCubeLoc.Value);
                }
                else
                {
                    if (_prevPickedBlock.HasValue)
                        _editedEntity.AlterOverlay(_prevPickedBlock.Value, 0);
                }

                _prevPickedBlock = PickedCubeLoc;
                _editedEntity.Altered = true;
            }

            if (NewCubePlace.HasValue && NewCubePlace != _prevNewCubePlace)
            {
                _editedEntity.AlterOverlay(NewCubePlace.Value, 20);


                if (_prevNewCubePlace.HasValue)
                    _editedEntity.AlterOverlay(_prevNewCubePlace.Value, 0);

                _prevNewCubePlace = NewCubePlace;
                _editedEntity.Altered = true;
            }

            HandleInput();

            _editedEntity.Update();
        }

        private void HandleInput()
        {
            //XXX MultiSelectEnabled is handled quick and dirty, could be more tool oriented
            if (! MultiSelectEnabled && _actions.isTriggered(Actions.Use_Left))
            {
                LeftTool.Use();
                _editedEntity.Altered = true;
            }
            if (_actions.isTriggered(Actions.Use_Right))
            {
                RightTool.Use();
                _editedEntity.Altered = true;
            }
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
                }
                _hudComponent.Enabled = true;
                _pickingRenderer.Enabled = true;
                _pickingRenderer.Visible = true;

            }
        }

        public void UpdatePickedCube(byte value)
        {
            if (PickedCubeLoc.HasValue)
                SafeSetBlock(PickedCubeLoc.Value.X, PickedCubeLoc.Value.Y, PickedCubeLoc.Value.Z, value);
        }

        public void UpdateNewPlace(byte value)
        {
            if (NewCubePlace.HasValue)
                SafeSetBlock(NewCubePlace.Value.X, NewCubePlace.Value.Y, NewCubePlace.Value.Z, value);
        }

        private void DrawItems()
        {
            if (_editedEntity == null) return;

            //Applying Correct Render States
            StatesRepository.ApplyStates(GameDXStates.DXStates.Rasters.Default, GameDXStates.DXStates.NotSet,
                                         GameDXStates.DXStates.DepthStencils.DepthEnabled);

            _itemEffect.Begin();

            _itemEffect.CBPerFrame.Values.ViewProjection =
                Matrix.Transpose(_camManager.ActiveCamera.ViewProjection3D_focused);
            _itemEffect.CBPerFrame.Values.SunColor = Vector3.One;
            _itemEffect.CBPerFrame.Values.fogdist = 100;

            _itemEffect.CBPerFrame.IsDirty = true;

            Matrix world = Matrix.Scaling((float) Scale)*
                           Matrix.Translation(_editedEntity.Position.AsVector3());

            world = _worldFocusManager.CenterOnFocus(ref world);

            _itemEffect.CBPerDraw.Values.World = Matrix.Transpose(world);
            _itemEffect.CBPerDraw.Values.popUpYOffset = 0;
            _itemEffect.CBPerDraw.IsDirty = true;
            _itemEffect.Apply();

            _editedEntity.VertexBuffer.SetToDevice(0);
            _d3DEngine.Context.Draw(_editedEntity.VertexBuffer.VertexCount, 0);
        }


        private int _lastLoaded = -1;

        public void LoadEntity()
        {
            //quick n dirty local disk load system : each time you call it you load a new file !
            _lastLoaded++;
            String path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Utopia", "entity_" + _lastLoaded + ".bin");
            if (File.Exists(path))
            {
                using (BinaryReader reader = new BinaryReader(File.Open(path, FileMode.Open)))
                {
                    _editedEntity.VoxelEntity.Model.Load(reader);
                }
            } else
            {
                if (_lastLoaded == 0) return;//0 doesnt exist = you click load but you never saved

                _lastLoaded = -1; //really quick n dirty
                LoadEntity(); 
            }
        }

        public void SaveEntity()
        {
            //quick n dirty saving on local disk. each call saves a new copy. don't know if we will keep it or save on server only
            string path="";
            int i = 0;
            do
            {
                path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),"Utopia", "entity_"+i+".bin");
                i++;
            } while(File.Exists(path));

          
            using ( BinaryWriter writer = new BinaryWriter(File.Open(path,FileMode.CreateNew)))
            {
                _editedEntity.VoxelEntity.Model.Save(writer);    
            }
            
            //TODO commit entity to server = _editedEntity.Commit();
        }


        private Vector3D _castedFrom, _castedTo;
       

        private void GetSelectedBlock()
        {
            byte[,,] blocks = _editedEntity.VoxelEntity.Model.Blocks;

            //XXX avoid unnecessay picking loops. but not just mousestate changes , you can still pick in freelook mode. 

            Vector3D mouseWorldPosition, mouseLookAt;
            _inputManager.UnprojectMouseCursor(out mouseWorldPosition, out mouseLookAt);

            //Create a ray from MouseWorldPosition to a specific size (That we will increment) and then check if we intersect an existing cube !
            int nbrpt = 0;

            double start = 0;
            //how far you need to be from the edited cube. 0 means you can pick right in your eye (ouch) 
            double end = start + 16;
            //TODO magic number for editor picking, should be related to scale and block array size  

            PickedCubeLoc = null;
            //loose the state of the pickedBlock : the for loop must find a new one, we dont want a stale picked blocks 

            double startX = _editedEntity.Position.X;
            double startY = _editedEntity.Position.Y;
            double startZ = _editedEntity.Position.Z;

            double endX = blocks.GetLength(0)*Scale + startX;
            double endY = blocks.GetLength(1)*Scale + startY;
            double endZ = blocks.GetLength(2)*Scale + startZ;

            double? found = null;
            const double step = 0.05d;

            for (double x = start; x < end; x += step) //for scale=1, was0.5 40 0.1  40 seems a high pick distance ! 
            {
                nbrpt++;
                Vector3D targetPoint = (mouseWorldPosition + (mouseLookAt*x));

                if (x == start) _castedFrom = targetPoint;
                _castedTo = targetPoint;

                if (targetPoint.X >= startX && targetPoint.Y >= startY && targetPoint.Z >= startZ
                    && targetPoint.X < endX && targetPoint.Y < endY && targetPoint.Z < endZ)
                {
                    Vector3I hit = new Vector3I((int) ((targetPoint.X - startX)/Scale),
                                                (int) ((targetPoint.Y - startY)/Scale),
                                                (int) ((targetPoint.Z - startZ)/Scale));
                    if (Pickable(hit))
                    {
                        found = x;
                        PickedCubeLoc = hit;
                        Console.WriteLine(@"{0} - {1} -  {2}",PickedCubeLoc,x,nbrpt);
                        break;
                    }
                }
            }

            if (found == null) return;
          
            NewCubePlace = null;

            start = found.Value - step;
            for (double x = start; x > 0.7d*Scale; x -= step)
            {
                Vector3D targetPoint = (mouseWorldPosition + (mouseLookAt*x));

                if (targetPoint.X >= startX && targetPoint.Y >= startY && targetPoint.Z >= startZ
                    && targetPoint.X < endX && targetPoint.Y < endY && targetPoint.Z < endZ)
                {
                    Vector3I hit = new Vector3I((int) ((targetPoint.X - startX)/Scale),
                                                (int) ((targetPoint.Y - startY)/Scale),
                                                (int) ((targetPoint.Z - startZ)/Scale));

                    NewCubePlace = hit;
                    break;
                }
            }
        }


        /// <summary>
        /// check if a hit is pickable
        /// </summary>
        /// <param name="hit">picking possibility</param>
        /// <returns>true if acceptable for picking</returns>
        private bool Pickable(Vector3I hit)
        {
            return Blocks[hit.X, hit.Y, hit.Z] != 0;
        }


        public override void Dispose()
        {
            _itemEffect.Dispose();
            _ui = null; //TODO _ui references the texture, check if more dispose work is needed
            Texture.Dispose();
            if (_editedEntity != null) _editedEntity.Dispose();
            base.Dispose();
        }

        public string GetInfo()
        {
            return string.Empty;
        }

        public bool SafeSetBlock(int x, int y, int z, byte value)
        {
            if (x >= 0 && y >= 0 && z >= 0 && x < Blocks.GetLength(0) && y < Blocks.GetLength(1) &&
                z < Blocks.GetLength(2))
            {
                Blocks[x, y, z] = value;

                if (VerticalSymetryEnabled)
                {
                    int xMirror = Blocks.GetLength(0) - 1 - x;
                    Blocks[xMirror, y, z] = value;
                }
                return true;
            }
            return false;
        }

        public byte BlockAt(Vector3I loc)
        {
            return Blocks[loc.X, loc.Y, loc.Z];
        }

        public void ClearSelected()
        {
            foreach (var selectedPos in Selected)
            {
                _editedEntity.AlterOverlay(selectedPos.X, selectedPos.Y, selectedPos.Z, 0);
            }
            Selected.Clear();
        }

       
    }
}