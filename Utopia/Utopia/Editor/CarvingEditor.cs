using System;
using System.Collections.Generic;
using SharpDX;
using SharpDX.Direct3D11;
using Utopia.Effects.Shared;
using Utopia.Entities.Managers;
using Utopia.Entities.Managers.Interfaces;
using Utopia.Entities.Renderer.Interfaces;
using Utopia.Entities.Voxel;
using Utopia.Resources.Effects.Terran;
using Utopia.Shared.Entities.Concrete;
using Utopia.Shared.Entities.Dynamic;
using Utopia.Shared.Entities.Interfaces;
using Utopia.Shared.Structs;
using Utopia.Worlds.Chunks.ChunkEntityImpacts;

namespace Utopia.Editor
{
    //public class CarvingEditor : DrawableGameComponent
    //{
    //    private const double Scale = 1f/8f;
    //    private readonly ActionsManager _actions;
    //    private readonly CameraManager _camManager;
    //    private readonly IChunkEntityImpactManager _chunkEntityImpactManager;
    //    private readonly D3DEngine _d3DEngine;
    //    private readonly IDynamicEntityManager _entityManager;
    //    private readonly Hud _hudComponent;
    //    private readonly InputsManager _inputManager;
    //    private readonly IPickingRenderer _pickingRenderer;
    //    private readonly PlayerCharacter _player;
    //    private readonly PlayerEntityManager _playerMgr;
    //    private readonly Screen _screen;
    //    private readonly VoxelMeshFactory _voxelMeshFactory;
    //    private readonly WorldFocusManager _worldFocusManager;
    //    public Vector3I? NewCubePlace;
    //    public Vector3I? PickedCubeLoc;
    //    public List<Vector3I> Selected = new List<Vector3I>();


    //    public ShaderResourceView Texture; //it's a field for being able to dispose the resource
    //    private VisualVoxelEntity _editedEntity;
    //    private HLSLTerran _itemEffect;

    //    private ITool _leftToolbeforeEnteringEditor;

    //    private Vector3I? _prevNewCubePlace;

    //    private Vector3I? _prevPickedBlock;
    //    private SharedFrameCB _sharedFrameCB;
    //   // private EntityEditorUi _ui;

    //    public CarvingEditor(Screen screen, D3DEngine d3DEngine, CameraManager camManager,
    //                         VoxelMeshFactory voxelMeshFactory, WorldFocusManager worldFocusManager,
    //                         ActionsManager actions, Hud hudComponent,
    //                         InputsManager inputsManager, IPickingRenderer pickingRenderer,
    //                         IDynamicEntityManager entityManager, PlayerEntityManager playerMgr,
    //                         SharedFrameCB sharedFrameCB, IChunkEntityImpactManager chunkEntityImpactManager)
    //    {
    //        _screen = screen;
    //        _playerMgr = playerMgr;
    //        _player = _playerMgr.Player;
    //        _actions = actions;
    //        _worldFocusManager = worldFocusManager;
    //        _voxelMeshFactory = voxelMeshFactory;
    //        _camManager = camManager;
    //        _d3DEngine = d3DEngine;
    //        _hudComponent = hudComponent;
    //        _inputManager = inputsManager;
    //        _pickingRenderer = pickingRenderer;
    //        _entityManager = entityManager;

    //        _sharedFrameCB = sharedFrameCB;
    //        _chunkEntityImpactManager = chunkEntityImpactManager;

    //        // inactive by default, use F12 UI to enable :)
    //        _leftToolbeforeEnteringEditor = _player.Equipment.LeftTool;
    //        Visible = false;
    //        Enabled = false;

    //        DrawOrders.UpdateIndex(0, 5000);
    //    }

    //    public byte SelectedCubeId { get; set; }

    //    public byte[,,] Blocks
    //    {
    //        get { return _editedEntity.VoxelEntity.Model.Blocks; }
    //    }


    //    public void StartCarving()
    //    {
    //        SelectedCubeId = _playerMgr.PickedCube.Cube.Id;
    //        var spawn = new EditableVoxelEntity();
    //        spawn.Model.Blocks = new byte[8,8,8];

    //        Vector3I tmp = _player.EntityState.PickedBlockPosition;
    //        _chunkEntityImpactManager.ReplaceBlock(ref tmp, 0); //TODO currently, removes edited block client side

    //        for (int x = 0; x < spawn.Model.Blocks.GetLength(0); x++)
    //        {
    //            for (int y = 0; y < spawn.Model.Blocks.GetLength(1); y++)
    //            {
    //                for (int z = 0; z < spawn.Model.Blocks.GetLength(2); z++)
    //                {
    //                    spawn.Model.Blocks[x, y, z] = SelectedCubeId;
    //                }
    //            }
    //        }

    //        var overlays = new byte[8,8,8];

    //        Vector3I pos = _player.EntityState.PickedBlockPosition;
    //        _editedEntity = new VisualVoxelEntity(_voxelMeshFactory, spawn, overlays, false);
    //        _editedEntity.Position = new Vector3D(pos.X, pos.Y, pos.Z);
    //        _pickingRenderer.Enabled = false;
    //        _pickingRenderer.Visible = false;
    //    }


    //    public override void LoadContent(DeviceContext Context)
    //    {
    //        String[] dirs = new[] {ClientSettings.TexturePack + @"Terran/", ClientSettings.TexturePack + @"Editor/"};

    //        ArrayTexture.CreateTexture2DFromFiles(_d3DEngine.Device, dirs, @"ct*.png", FilterFlags.Point,
    //                                              "ArrayTexture_EntityEditor", out Texture);

    //        _itemEffect = new HLSLTerran(_d3DEngine, ClientSettings.EffectPack + @"Terran/TerranEditor.hlsl",
    //                                     VertexCubeSolid.VertexDeclaration, _sharedFrameCB.CBPerFrame);

    //        _itemEffect.TerraTexture.Value = Texture;

    //        _itemEffect.SamplerDiffuse.Value =
    //            RenderStatesRepo.GetSamplerState(GameDXStates.DXStates.Samplers.UVWrap_MinMagMipLinear);

    //        //_ui = new EntityEditorUi(this);
    //    }

    //    public override void Update( GameTime timeSpend)
    //    {
    //        if (_editedEntity == null) return;

    //        GetSelectedBlock();

    //        if (!PickedCubeLoc.HasValue && _prevPickedBlock.HasValue)
    //        {
    //            _editedEntity.AlterOverlay(_prevPickedBlock.Value, 0);
    //        }

    //        if (PickedCubeLoc.HasValue && PickedCubeLoc != _prevPickedBlock)
    //        {
    //            int x = PickedCubeLoc.Value.X;
    //            int y = PickedCubeLoc.Value.Y;
    //            int z = PickedCubeLoc.Value.Z;

    //            _editedEntity.AlterOverlay(x, y, z, 21);

    //            if (Mouse.GetState().LeftButton == ButtonState.Pressed)
    //            {
    //                //XXX avoid direct mouseState ButtonState.Pressed in editor ?
    //                Selected.Add(PickedCubeLoc.Value);
    //            }
    //            else
    //            {
    //                if (_prevPickedBlock.HasValue)
    //                    _editedEntity.AlterOverlay(_prevPickedBlock.Value, 0);
    //            }

    //            _prevPickedBlock = PickedCubeLoc;
    //            _editedEntity.Altered = true;
    //        }

    //        if (NewCubePlace.HasValue && NewCubePlace != _prevNewCubePlace)
    //        {
    //            _editedEntity.AlterOverlay(NewCubePlace.Value, 20);


    //            if (_prevNewCubePlace.HasValue)
    //                _editedEntity.AlterOverlay(_prevNewCubePlace.Value, 0);

    //            _prevNewCubePlace = NewCubePlace;
    //            _editedEntity.Altered = true;
    //        }

    //        HandleInput();

    //        _editedEntity.Update();
    //    }

    //    private void HandleInput()
    //    {
    //        if (_actions.isTriggered(Actions.Use_Left))
    //        {
    //            UpdatePickedCube(0);
    //            _editedEntity.Altered = true;
    //        }
    //        if (_actions.isTriggered(Actions.Use_Right))
    //        {
    //            UpdateNewPlace(SelectedCubeId);
    //            _editedEntity.Altered = true;
    //        }
    //    }

    //    public override void Draw(DeviceContext context, int index)
    //    {
    //        DrawItems();
    //    }

    //    protected override void OnEnabledChanged()
    //    {
    //        base.OnEnabledChanged();

    //        if (Enabled)
    //        {
    //            _playerMgr.HasMouseFocus = false;
    //            _playerMgr.MousepickDisabled = true;
    //            _playerMgr.Player.MoveSpeed = 0.25f;
    //            _hudComponent.Enabled = false;

    //            _leftToolbeforeEnteringEditor = _player.Equipment.LeftTool;
    //            //_player.Equipment.LeftTool = null;
               
    //        }
    //        else
    //        {
    //            _playerMgr.HasMouseFocus = true;
    //            _playerMgr.MousepickDisabled = false;
    //            _playerMgr.Player.MoveSpeed = PlayerCharacter.DefaultMoveSpeed;
    //            //_player.Equipment.LeftTool = _leftToolbeforeEnteringEditor;
    //            _hudComponent.Enabled = true;
    //            _pickingRenderer.Enabled = true;
    //            _pickingRenderer.Visible = true;
    //        }
    //    }

    //    public void UpdatePickedCube(byte value)
    //    {
    //        if (PickedCubeLoc.HasValue)
    //            SafeSetBlock(PickedCubeLoc.Value.X, PickedCubeLoc.Value.Y, PickedCubeLoc.Value.Z, value);
    //    }

    //    public void UpdateNewPlace(byte value)
    //    {
    //        if (NewCubePlace.HasValue)
    //            SafeSetBlock(NewCubePlace.Value.X, NewCubePlace.Value.Y, NewCubePlace.Value.Z, value);
    //    }

    //    private void DrawItems()
    //    {
    //        if (_editedEntity == null) return;

    //        //Applying Correct Render States
    //        RenderStatesRepo.ApplyStates(GameDXStates.DXStates.Rasters.Default, GameDXStates.DXStates.NotSet,
    //                                     GameDXStates.DXStates.DepthStencils.DepthEnabled);

    //        _itemEffect.Begin();

    //        //_itemEffect.CBPerFrame.Values.ViewProjection =
    //        //    Matrix.Transpose(_camManager.ActiveCamera.ViewProjection3D_focused);
    //        //_itemEffect.CBPerFrame.Values.SunColor = Vector3.One;
    //        //_itemEffect.CBPerFrame.Values.fogdist = 100;

    //        //_itemEffect.CBPerFrame.IsDirty = true;

    //        Matrix world = Matrix.Scaling((float) Scale)*
    //                       Matrix.Translation(_editedEntity.Position.AsVector3());

    //        world = _worldFocusManager.CenterOnFocus(ref world);

    //        _itemEffect.CBPerDraw.Values.World = Matrix.Transpose(world);
    //        _itemEffect.CBPerDraw.Values.popUpYOffset = 0;
    //        _itemEffect.CBPerDraw.IsDirty = true;
    //        _itemEffect.Apply();

    //        _editedEntity.VertexBuffer.SetToDevice(0);
    //        _d3DEngine.Context.Draw(_editedEntity.VertexBuffer.VertexCount, 0);
    //    }

    //    private void GetSelectedBlock()
    //    {
    //        byte[,,] blocks = _editedEntity.VoxelEntity.Model.Blocks;

    //        //XXX avoid unnecessay picking loops. but not just mousestate changes , you can still pick in freelook mode. 

    //        Vector3D mouseWorldPosition, mouseLookAt;
    //        _inputManager.UnprojectMouseCursor(out mouseWorldPosition, out mouseLookAt);

    //        //Create a ray from MouseWorldPosition to a specific size (That we will increment) and then check if we intersect an existing cube !
    //        int nbrpt = 0;

    //        double start = 0;
    //        //how far you need to be from the edited cube. 0 means you can pick right in your eye (ouch) 
    //        double end = start + 16;

    //        PickedCubeLoc = null;
    //        //loose the state of the pickedBlock : the for loop must find a new one, we dont want a stale picked blocks 

    //        double startX = _editedEntity.Position.X;
    //        double startY = _editedEntity.Position.Y;
    //        double startZ = _editedEntity.Position.Z;

    //        double endX = blocks.GetLength(0)*Scale + startX;
    //        double endY = blocks.GetLength(1)*Scale + startY;
    //        double endZ = blocks.GetLength(2)*Scale + startZ;

    //        double? found = null;
    //        const double step = 0.05d;

    //        for (double x = start; x < end; x += step) //for scale=1, was0.5 40 0.1  40 seems a high pick distance ! 
    //        {
    //            nbrpt++;
    //            Vector3D targetPoint = (mouseWorldPosition + (mouseLookAt*x));

    //            // if (x == start) _castedFrom = targetPoint;
    //            // _castedTo = targetPoint;

    //            if (targetPoint.X >= startX && targetPoint.Y >= startY && targetPoint.Z >= startZ
    //                && targetPoint.X < endX && targetPoint.Y < endY && targetPoint.Z < endZ)
    //            {
    //                Vector3I hit = new Vector3I((int) ((targetPoint.X - startX)/Scale),
    //                                            (int) ((targetPoint.Y - startY)/Scale),
    //                                            (int) ((targetPoint.Z - startZ)/Scale));
    //                if (Pickable(hit))
    //                {
    //                    found = x;
    //                    PickedCubeLoc = hit;
    //                    Console.WriteLine(@"{0} - {1} -  {2}", PickedCubeLoc, x, nbrpt);
    //                    break;
    //                }
    //            }
    //        }

    //        if (found == null) return;

    //        NewCubePlace = null;

    //        start = found.Value - step;
    //        for (double x = start; x > 0.7d*Scale; x -= step)
    //        {
    //            Vector3D targetPoint = (mouseWorldPosition + (mouseLookAt*x));

    //            if (targetPoint.X >= startX && targetPoint.Y >= startY && targetPoint.Z >= startZ
    //                && targetPoint.X < endX && targetPoint.Y < endY && targetPoint.Z < endZ)
    //            {
    //                Vector3I hit = new Vector3I((int) ((targetPoint.X - startX)/Scale),
    //                                            (int) ((targetPoint.Y - startY)/Scale),
    //                                            (int) ((targetPoint.Z - startZ)/Scale));

    //                NewCubePlace = hit;
    //                break;
    //            }
    //        }
    //    }


    //    /// <summary>
    //    /// check if a hit is pickable
    //    /// </summary>
    //    /// <param name="hit">picking possibility</param>
    //    /// <returns>true if acceptable for picking</returns>
    //    private bool Pickable(Vector3I hit)
    //    {
    //        return Blocks[hit.X, hit.Y, hit.Z] != 0;
    //    }


    //    public override void Dispose()
    //    {
    //        if(_itemEffect != null) _itemEffect.Dispose();
    //        if (Texture != null) Texture.Dispose();
    //        if (_editedEntity != null) _editedEntity.Dispose();
    //        base.Dispose();
    //    }

    //    public bool SafeSetBlock(int x, int y, int z, byte value)
    //    {
    //        if (x >= 0 && y >= 0 && z >= 0 && x < Blocks.GetLength(0) && y < Blocks.GetLength(1) &&
    //            z < Blocks.GetLength(2))
    //        {
    //            Blocks[x, y, z] = value;
    //            return true;
    //        }
    //        return false;
    //    }

    //    public byte BlockAt(Vector3I loc)
    //    {
    //        return Blocks[loc.X, loc.Y, loc.Z];
    //    }
    //}
}