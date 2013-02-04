using System;
using System.Diagnostics;
using System.Linq;
using System.Collections.Generic;
using S33M3CoreComponents.Cameras;
using S33M3CoreComponents.Cameras.Interfaces;
using S33M3CoreComponents.WorldFocus;
using S33M3DXEngine;
using S33M3DXEngine.RenderStates;
using S33M3Resources.Structs.Vertex;
using SharpDX;
using Utopia.Entities.Voxel;
using Ninject;
using Utopia.Entities.Managers.Interfaces;
using Utopia.Shared.Chunks;
using Utopia.Shared.Entities.Dynamic;
using Utopia.Shared.Entities.Events;
using Utopia.Shared.Entities.Interfaces;
using S33M3DXEngine.Main;
using SharpDX.Direct3D11;
using Utopia.Shared.Entities.Models;
using Utopia.Shared.GameDXStates;
using Utopia.Shared.Settings;
using Utopia.Shared.World;
using Utopia.Worlds.SkyDomes;
using S33M3CoreComponents.Maths;
using Utopia.Shared.Entities.Concrete;
using S33M3CoreComponents.Meshes;
using S33M3CoreComponents.Meshes.Factories;
using S33M3DXEngine.Textures;
using S33M3DXEngine.Buffers;
using S33M3Resources.Structs;
using S33M3CoreComponents.Sprites2D;
using S33M3CoreComponents.Sprites3D;
using S33M3CoreComponents.Sprites3D.Interfaces;
using Utopia.Components;
using Utopia.Resources.Effects.Entities;
using Utopia.Resources.Effects;
using Utopia.Resources.Sprites;
using Utopia.Shared.Configuration;

namespace Utopia.Entities.Managers
{
    /// <summary>
    /// Keeps a collection of IDynamicEntities received from the server.
    /// Responsible to draw them, also used to check collission detection with the player
    /// Draws the player entity if player is in 3rd person mode
    /// </summary>
    public class DynamicEntityManager : DrawableGameComponent, IVisualDynamicEntityManager
    {
        /// <summary>
        /// Allows to group instances by model to perform instanced drawing
        /// </summary>
        private class ModelAndInstances
        {
            public VisualVoxelModel VisualModel;                    //A model
            public Dictionary<uint, VoxelModelInstance> Instances;  //Instanced model list
        }

        #region Private Variables
        private readonly int VOXEL_DRAW = 0;
        private readonly int SPRITENAME_DRAW;

        private HLSLVoxelModelInstanced _voxelModelEffect;
        private HLSLVoxelModel _voxelToolEffect;

        private Sprite3DRenderer<Sprite3DTextProc> _dynamicEntityNameRenderer;
        private SpriteFont _dynamicEntityNameFont;

        private readonly Dictionary<uint, VisualDynamicEntity> _dynamicEntitiesDico = new Dictionary<uint, VisualDynamicEntity>();
        private readonly D3DEngine _d3DEngine;
        private readonly VoxelModelManager _voxelModelManager;
        private readonly CameraManager<ICameraFocused> _camManager;
        private readonly WorldFocusManager _worldFocusManager;
        private readonly VisualWorldParameters _visualWorldParameters;
        private readonly SingleArrayChunkContainer _chunkContainer;
        private int _staticEntityViewRange;
        private IDynamicEntity _playerEntity;

        //Cube Rendering
        private Mesh _cubeMesh;
        private Mesh _cubeMeshBluePrint;

        private Dictionary<string, KeyValuePair<VisualVoxelModel, VoxelModelInstance>> _toolsModels = new Dictionary<string, KeyValuePair<VisualVoxelModel, VoxelModelInstance>>();
        
        //List of all models use by Dynamic Entity, associated with the Instances created from them
        private readonly Dictionary<string, ModelAndInstances> _models = new Dictionary<string, ModelAndInstances>();
        #endregion

        #region Public Properties
        public List<IVisualVoxelEntityContainer> DynamicEntities { get; set; }
        /// <summary>
        /// Gets or sets current player entity to display
        /// Set to null in first person mode
        /// </summary>
        public IDynamicEntity PlayerEntity
        {
            get { return _playerEntity; }
            set
            {
                if (_playerEntity == value)
                    return;

                if (value != null)
                {
                    AddEntity(value, false);
                }
                else
                {
                    RemoveEntity(_playerEntity);
                }

                _playerEntity = value;
            }
        }
        #endregion

        public event EventHandler<DynamicEntityEventArgs> EntityAdded;
        public event EventHandler<DynamicEntityEventArgs> EntityRemoved;

        #region DI
        [Inject]
        public PlayerEntityManager PlayerEntityManager
        {
            get { return _playerEntityManager; }
            set { _playerEntityManager = value; }
        }

        [Inject]
        public ISkyDome SkyDome
        {
            get { return _skyDome; }
            set { _skyDome = value; }
        }

        [Inject]
        public SharedFrameCB SharedFrameCB
        {
            get { return _sharedFrameCB; }
            set { _sharedFrameCB = value; }
        }

        #endregion

        public DynamicEntityManager(D3DEngine d3DEngine,
                                    VoxelModelManager voxelModelManager,
                                    CameraManager<ICameraFocused> camManager,
                                    WorldFocusManager worldFocusManager,
                                    VisualWorldParameters visualWorldParameters,
                                    SingleArrayChunkContainer chunkContainer)
        {
            _d3DEngine = d3DEngine;
            _voxelModelManager = voxelModelManager;
            _camManager = camManager;
            _chunkContainer = chunkContainer;
            _worldFocusManager = worldFocusManager;
            _visualWorldParameters = visualWorldParameters;

            _voxelModelManager.VoxelModelAvailable += VoxelModelManagerVoxelModelReceived;
            _camManager.ActiveCameraChanged += CamManagerActiveCameraChanged;

            DynamicEntities = new List<IVisualVoxelEntityContainer>();

            DrawOrders.UpdateIndex(VOXEL_DRAW, 99, "VOXEL_DRAW");
            SPRITENAME_DRAW = DrawOrders.AddIndex(1060, "NAME_DRAW");

            this.IsDefferedLoadContent = true;
        }

        public override void BeforeDispose()
        {
            _voxelModelManager.VoxelModelAvailable -= VoxelModelManagerVoxelModelReceived;
            _camManager.ActiveCameraChanged -= CamManagerActiveCameraChanged;
            foreach (var item in _dynamicEntitiesDico.Values) item.Dispose();
        }

        #region Public Methods

        public override void Initialize()
        {
            if (ClientSettings.Current.Settings.GraphicalParameters.StaticEntityViewSize > (ClientSettings.Current.Settings.GraphicalParameters.WorldSize / 2) - 2.5)
            {
                _staticEntityViewRange = (int)((ClientSettings.Current.Settings.GraphicalParameters.WorldSize / 2) - 2.5) * 16;
            }
            else
            {
                _staticEntityViewRange = ClientSettings.Current.Settings.GraphicalParameters.StaticEntityViewSize * 16;
            }

            _milkShapeMeshfactory = new MilkShape3DMeshFactory();
            //Prepare Textured Block rendering when equiped ==============================================================
            _milkShapeMeshfactory.LoadMesh(@"\Meshes\block.txt", out _cubeMeshBluePrint, 0);
        }


        private HLSLCubeTool _cubeToolEffect;
        private IMeshFactory _milkShapeMeshfactory;
        private ShaderResourceView _cubeTextureView;
        private VertexBuffer<VertexMesh> _cubeVb;
        private IndexBuffer<ushort> _cubeIb;
        private Dictionary<int, int> _materialChangeMapping;
        private PlayerEntityManager _playerEntityManager;
        private ISkyDome _skyDome;
        private SharedFrameCB _sharedFrameCB;

        public override void LoadContent(DeviceContext context)
        {
            ArrayTexture.CreateTexture2DFromFiles(context.Device, context, ClientSettings.TexturePack + @"Terran/", @"ct*.png", FilterFlags.Point, "ArrayTexture_DefaultEntityRenderer", out _cubeTextureView);
            ToDispose(_cubeTextureView);
            //Create Vertex/Index Buffer to store the loaded cube mesh.
            _cubeVb = ToDispose(new VertexBuffer<VertexMesh>(context.Device, _cubeMeshBluePrint.Vertices.Length, VertexMesh.VertexDeclaration, SharpDX.Direct3D.PrimitiveTopology.TriangleList, "Block VB"));
            _cubeIb = ToDispose(new IndexBuffer<ushort>(context.Device, _cubeMeshBluePrint.Indices.Length, SharpDX.DXGI.Format.R16_UInt, "Block IB"));

            _cubeToolEffect = ToDispose(new HLSLCubeTool(context.Device, ClientSettings.EffectPack + @"Entities/CubeTool.hlsl", VertexMesh.VertexDeclaration));
            _cubeToolEffect.DiffuseTexture.Value = _cubeTextureView;
            _cubeToolEffect.SamplerDiffuse.Value = RenderStatesRepo.GetSamplerState(DXStates.Samplers.UVClamp_MinMagMipPoint);

            _voxelModelEffect = ToDispose(new HLSLVoxelModelInstanced(context.Device, ClientSettings.EffectPack + @"Entities\VoxelModelInstanced.hlsl", VertexVoxelInstanced.VertexDeclaration));
            _voxelToolEffect = ToDispose(new HLSLVoxelModel(context.Device, ClientSettings.EffectPack + @"Entities\VoxelModel.hlsl", VertexVoxel.VertexDeclaration));
            _materialChangeMapping = new Dictionary<int, int>();

            //Create the font to base use by the sprite3dText Processor
            _dynamicEntityNameFont = ToDispose(new SpriteFont());
            _dynamicEntityNameFont.Initialize("Lucida Console", 32f, System.Drawing.FontStyle.Regular, true, context.Device, false);
            
            //Create the processor that will be used by the Sprite3DRenderer
            Sprite3DTextProc textProcessor = ToDispose(new Sprite3DTextProc(_dynamicEntityNameFont, RenderStatesRepo.GetSamplerState(DXStates.Samplers.UVWrap_Text), ToDispose(new UtopiaIncludeHandler()), _sharedFrameCB.CBPerFrame, ClientSettings.EffectPack + @"Sprites\PointSprite3DText.hlsl"));

            //Create a sprite3Drenderer that will use the previously created processor to accumulate text data for drawing.
            _dynamicEntityNameRenderer = ToDispose(new Sprite3DRenderer<Sprite3DTextProc>(textProcessor, 
                                                                        DXStates.Rasters.Default,
                                                                        DXStates.Blenders.Enabled,
                                                                        DXStates.DepthStencils.DepthReadWriteEnabled,
                                                                        context));
        }

        public override void UnloadContent()
        {
            DisableComponent();
            foreach (var item in _dynamicEntitiesDico.Values) item.Dispose();
            _dynamicEntitiesDico.Clear();
            IsInitialized = false;
        }

        public override void FTSUpdate(GameTime timeSpent)
        {
            foreach (var entity in _dynamicEntitiesDico.Values)
            {
                entity.Update(timeSpent);
            }
        }

        public override void VTSUpdate(double interpolationHd, float interpolationLd, long timePassed)
        {
            foreach (var entity in _dynamicEntitiesDico.Values)
            {
                entity.Interpolation(interpolationHd, interpolationLd, timePassed);

                // update model color, get the cube where model is
                var result = _chunkContainer.GetCube(entity.WorldPosition.ValueInterp);
                if (result.isValid && result.Cube.Id == WorldConfiguration.CubeId.Air)
                {
                    // we take the max color
                    var sunPart = (float)result.Cube.EmissiveColor.A / 255;
                    var sunColor = _skyDome.SunColor * sunPart;
                    var resultColor = Color3.Max(result.Cube.EmissiveColor.ToColor3(), sunColor);

                    entity.ModelLight.Value = resultColor;

                    if (entity.ModelLight.ValueInterp != entity.ModelLight.Value)
                    {
                        Color3.Lerp(ref entity.ModelLight.ValueInterp, ref entity.ModelLight.Value, timePassed / 100f, out entity.ModelLight.ValueInterp);
                    }
                }
            }
        }

        public override void Draw(DeviceContext context, int index)
        {
            if (index == VOXEL_DRAW)
            {
                VoxelDraw(context);
                return;
            }

            if (index == SPRITENAME_DRAW)
            {
                DrawEntitiesName(context);
                return;
            }
        }

        public IEnumerable<IDynamicEntity> EnumerateAround(Vector3 pos)
        {
            // as we store all entites in one collection
            // we will enumerate by all of them and add a player entity separately

            yield return _playerEntityManager.Player;

            foreach (var entity in _dynamicEntitiesDico.Values.Select(visualDynamicEntity => visualDynamicEntity.DynamicEntity))
            {
                yield return entity;
            }
        }

        #endregion

        #region Private Methods

        private void VoxelDraw(DeviceContext context)
        {
            //Applying Correct Render States
            RenderStatesRepo.ApplyStates(context, DXStates.Rasters.Default, DXStates.Blenders.Disabled, DXStates.DepthStencils.DepthReadWriteEnabled);
            _voxelModelEffect.Begin(context);
            _voxelModelEffect.CBPerFrame.Values.LightDirection = _skyDome.LightDirection;
            _voxelModelEffect.CBPerFrame.Values.ViewProjection = Matrix.Transpose(_camManager.ActiveCamera.ViewProjection3D);
            _voxelModelEffect.CBPerFrame.IsDirty = true;
            _voxelModelEffect.Apply(context);

            //Draw each buffered Models =====================================
            foreach (var modelAndInstances in _models)
            {
                //For each instance of the model that have received a body
                foreach (var pairs in modelAndInstances.Value.Instances.Where(x => x.Value != null))
                {
                    var entityToRender = _dynamicEntitiesDico[pairs.Key];
                    var modelInstance = pairs.Value;
                    //Draw only the entities that are in Client view range
                    //if (_visualWorldParameters.WorldRange.Contains(entityToRender.VisualEntity.Position.ToCubePosition()))
                    if (MVector3.Distance2D(entityToRender.VisualVoxelEntity.VoxelEntity.Position, _camManager.ActiveCamera.WorldPosition.ValueInterp) <= _staticEntityViewRange)
                    {
                        modelInstance.World = Matrix.Scaling(1f / 16) * Matrix.Translation(entityToRender.WorldPosition.ValueInterp.AsVector3());
                        modelInstance.LightColor = entityToRender.ModelLight.ValueInterp;
                    }
                    else
                    {
                        modelInstance.World = Matrix.Zero;
                    }
                }

                if (modelAndInstances.Value.VisualModel != null && modelAndInstances.Value.Instances != null)
                {
                    var instancesToDraw = modelAndInstances.Value.Instances.Values.Where(x => x.World != Matrix.Zero).ToList();
                    modelAndInstances.Value.VisualModel.DrawInstanced(_d3DEngine.ImmediateContext, _voxelModelEffect, instancesToDraw);
                }
            }

            _voxelToolEffect.Begin(context);
            _voxelToolEffect.CBPerFrame.Values.LightDirection = _skyDome.LightDirection;
            _voxelToolEffect.CBPerFrame.Values.ViewProjection = Matrix.Transpose(_camManager.ActiveCamera.ViewProjection3D);
            _voxelToolEffect.CBPerFrame.IsDirty = true;
            _voxelToolEffect.Apply(context);

            // draw tools ================
            foreach (var pair in _dynamicEntitiesDico)
            {
                var charEntity = pair.Value.DynamicEntity as CharacterEntity;
                if (charEntity != null && pair.Value.ModelInstance != null && pair.Value.ModelInstance.World != Matrix.Zero)
                {
                    //Take the Tool entity equiped in character Right hand
                    if (charEntity.Equipment.RightTool is CubeResource)
                    {
                        DrawCube(context, (CubeResource)charEntity.Equipment.RightTool, charEntity);
                    }
                    else if (charEntity.Equipment.RightTool is IVoxelEntity)
                    {
                        IVoxelEntity voxelItem = charEntity.Equipment.RightTool as IVoxelEntity;
                        if (!string.IsNullOrEmpty(voxelItem.ModelName)) //Check if a voxel model is associated with the entity
                        {
                            DrawTool(voxelItem, charEntity);
                        }
                    }
                }
            }
        }

        private void DrawEntitiesName(DeviceContext context)
        {
            _dynamicEntityNameRenderer.Begin(context, true);

            foreach (VisualDynamicEntity dynamicEntity in _dynamicEntitiesDico.Values.Where(x => x.ModelInstance != null && x.ModelInstance.World != Matrix.Zero))
            {
                bool isMultiline = false;
                string Name;
                Vector3 textPosition = dynamicEntity.WorldPosition.ValueInterp.AsVector3();
                textPosition.Y += (dynamicEntity.ModelInstance.State.BoundingBox.Maximum.Y / 16); //Place the text above the BoundingBox
                ByteColor color = Color.White;

                if (dynamicEntity.DynamicEntity is CharacterEntity)
                {
                    Name = ((CharacterEntity)dynamicEntity.DynamicEntity).CharacterName;
                    if (_playerEntity == dynamicEntity.DynamicEntity)
                    {
                        color = Color.Yellow;
                    }
                    else
                    {
                        Name += Environment.NewLine + "<" + dynamicEntity.DynamicEntity.Name + ">";
                        isMultiline = true;
                    }
                }
                else
                {
                    Name = dynamicEntity.DynamicEntity.Name;
                    color = Color.WhiteSmoke;
                }

                var distance = MVector3.Distance(dynamicEntity.WorldPosition.ValueInterp, _camManager.ActiveCamera.WorldPosition.ValueInterp);
                float scaling = Math.Min( 0.040f, Math.Max(0.01f, 0.01f / 12 * (float)distance ));
                _dynamicEntityNameRenderer.Processor.DrawText(Name, ref textPosition, scaling, ref color, _camManager.ActiveCamera, MultiLineHandling: isMultiline);
            }

            _dynamicEntityNameRenderer.End(context);
        }

        private void DrawTool(IVoxelEntity voxelTool, CharacterEntity charEntity)
        {
            // get the model and instance
            KeyValuePair<VisualVoxelModel, VoxelModelInstance> mPair;
            if (!_toolsModels.TryGetValue(voxelTool.ModelName, out mPair))
            {
                var model = _voxelModelManager.GetModel(voxelTool.ModelName, false);
                mPair = new KeyValuePair<VisualVoxelModel, VoxelModelInstance>(model, model.VoxelModel.CreateInstance());
                mPair.Value.SetState(model.VoxelModel.GetMainState());
                _toolsModels.Add(voxelTool.ModelName, mPair);
            }

            var instance = mPair.Value;

            var voxelBB = instance.State.BoundingBox.GetSize();
            float scale = MathHelper.Min(1.0f, 32 / MathHelper.Max(MathHelper.Max(voxelBB.X, voxelBB.Y), voxelBB.Z));
            scale *= 0.70f;

            // setup the tool instance
            instance.World = Matrix.Scaling(scale) * charEntity.ModelInstance.GetToolTransform();
            instance.LightColor = charEntity.ModelInstance.LightColor;

            // draw it
            mPair.Key.Draw(_d3DEngine.ImmediateContext, _voxelToolEffect, instance);
        }

        private void DrawCube(DeviceContext context, CubeResource cube, CharacterEntity charEntity)
        {
            //Get the cube profile.
            var blockProfile = _visualWorldParameters.WorldParameters.Configuration.BlockProfiles[cube.CubeId];

            //Prapare to creation a new mesh with the correct texture mapping ID
            _materialChangeMapping[0] = blockProfile.Tex_Back;    //Change the Back Texture Id
            _materialChangeMapping[1] = blockProfile.Tex_Front;   //Change the Front Texture Id
            _materialChangeMapping[2] = blockProfile.Tex_Bottom;  //Change the Bottom Texture Id
            _materialChangeMapping[3] = blockProfile.Tex_Top;     //Change the Top Texture Id
            _materialChangeMapping[4] = blockProfile.Tex_Left;    //Change the Left Texture Id
            _materialChangeMapping[5] = blockProfile.Tex_Right;   //Change the Right Texture Id

            //Create the cube Mesh from the blue Print one
            _cubeMesh = _cubeMeshBluePrint.Clone(_materialChangeMapping);

            //Refresh the mesh data inside the buffers
            _cubeVb.SetData(context, _cubeMesh.Vertices);
            _cubeIb.SetData(context, _cubeMesh.Indices);

            //Render First person view of the tool, only if the tool is used by the current playing person !
            _cubeToolEffect.Begin(context);
            _cubeToolEffect.CBPerDraw.Values.Projection = Matrix.Transpose(_camManager.ActiveCamera.ViewProjection3D);
            _cubeToolEffect.CBPerDraw.Values.Screen = Matrix.Transpose(Matrix.Scaling(10.0f) * Matrix.Translation(0f,5.5f,1.8f) * charEntity.ModelInstance.GetToolTransform());
            _cubeToolEffect.CBPerDraw.Values.LightColor = charEntity.ModelInstance.LightColor;
            _cubeToolEffect.CBPerDraw.IsDirty = true;

            _cubeToolEffect.Apply(context);
            //Set the buffer to the device
            _cubeVb.SetToDevice(context, 0);
            _cubeIb.SetToDevice(context, 0);

            //Draw things here.
            context.DrawIndexed(_cubeIb.IndicesCount, 0, 0);
        }

        //Raised Events
        private void OnEntityAdded(DynamicEntityEventArgs e)
        {
            if (EntityAdded != null) EntityAdded(this, e);
        }

        public void OnEntityRemoved(DynamicEntityEventArgs e)
        {
            if (EntityRemoved != null) EntityRemoved(this, e);
        }

        //Dynamic Entity management
        private VisualDynamicEntity CreateVisualEntity(IDynamicEntity entity)
        {
            return new VisualDynamicEntity(entity, new VisualVoxelEntity(entity, _voxelModelManager));
        }

        public void AddEntity(IDynamicEntity entity, bool withNetworkInterpolation)
        {
            //Do we already have this entity ??
            if (_dynamicEntitiesDico.ContainsKey(entity.DynamicId) == false)
            {
                ModelAndInstances modelWithInstances;
                //Does the entity model exist in the collection ?
                //If yes, will send back the instance Ids registered to this Model
                if (!_models.TryGetValue(entity.ModelName, out modelWithInstances))
                {
                    //Model not existing ====
                    // load a new model
                    modelWithInstances = new ModelAndInstances
                    {
                        VisualModel = _voxelModelManager.GetModel(entity.ModelName), //Get the model from the VoxelModelManager
                        Instances = new Dictionary<uint, VoxelModelInstance>()       //Create a new dico of modelInstance  
                    };

                    //If the voxel model was send back by the manager, create the Mesh from it (Vx and idx buffers)
                    if (modelWithInstances.VisualModel != null)
                    {
                        // todo: probably do this in another thread
                        modelWithInstances.VisualModel.BuildMesh();
                    }
                    _models.Add(entity.ModelName, modelWithInstances);
                }

                VisualDynamicEntity newEntity = CreateVisualEntity(entity);
                newEntity.WithNetworkInterpolation = withNetworkInterpolation;
                _dynamicEntitiesDico.Add(entity.DynamicId, newEntity);
                DynamicEntities.Add(newEntity);

                //If the Model has a Voxel Model (Search by Name)
                if (modelWithInstances.VisualModel != null)
                {
                    //Create a new Instance of the Model
                    var instance = new VoxelModelInstance(modelWithInstances.VisualModel.VoxelModel);
                    entity.ModelInstance = instance;
                    modelWithInstances.Instances.Add(entity.DynamicId, instance);
                    _dynamicEntitiesDico[entity.DynamicId].ModelInstance = instance;
                }
                else
                {
                    //Add a new instance for the model, but without Voxel Body (=null instance)
                    modelWithInstances.Instances.Add(entity.DynamicId, null);
                }

                OnEntityAdded(new DynamicEntityEventArgs { Entity = entity });
            }
        }

        public void RemoveEntity(IDynamicEntity entity)
        {
            if (_dynamicEntitiesDico.ContainsKey(entity.DynamicId))
            {
                ModelAndInstances instances;
                if (!_models.TryGetValue(entity.ModelName, out instances))
                {
                    throw new InvalidOperationException("we have no such model");
                }
                instances.Instances.Remove(entity.DynamicId);

                VisualDynamicEntity visualEntity = _dynamicEntitiesDico[entity.DynamicId];
                DynamicEntities.Remove(visualEntity);
                _dynamicEntitiesDico.Remove(entity.DynamicId);
                visualEntity.Dispose();

                OnEntityRemoved(new DynamicEntityEventArgs { Entity = entity });
            }
        }

        public void RemoveEntityById(uint entityId, bool dispose = true)
        {
            VisualDynamicEntity entity;
            if (_dynamicEntitiesDico.TryGetValue(entityId, out entity))
            {
                ModelAndInstances instances;
                if (!_models.TryGetValue(entity.VisualVoxelEntity.VoxelEntity.ModelName, out instances))
                {
                    throw new InvalidOperationException("we have no such model");
                }
                instances.Instances.Remove(entityId);

                VisualDynamicEntity visualEntity = _dynamicEntitiesDico[entityId];
                DynamicEntities.Remove(_dynamicEntitiesDico[entityId]);
                _dynamicEntitiesDico.Remove(entityId);
                if (dispose) visualEntity.Dispose();
                OnEntityRemoved(new DynamicEntityEventArgs { Entity = visualEntity.DynamicEntity });
            }
        }

        public IDynamicEntity GetEntityById(uint p)
        {
            VisualDynamicEntity e;
            if (_dynamicEntitiesDico.TryGetValue(p, out e))
            {
                return e.DynamicEntity;
            }
            return null;
        }


        #region Events handling
        private void CamManagerActiveCameraChanged(object sender, CameraChangedEventArgs e)
        {
            if (e.Camera.CameraType == CameraType.FirstPerson)
            {
                PlayerEntity = null;
            }
            else
            {
                PlayerEntity = null;
                PlayerEntity = _playerEntityManager.Player;
            }
        }

        private void VoxelModelManagerVoxelModelReceived(object sender, VoxelModelReceivedEventArgs e)
        {
            //ForEach different Voxel model in local collection
            foreach (var modelAndInstances in _models)
            {
                //Check if the model receive is already existing. (By model name)
                if (modelAndInstances.Key == e.Model.Name)
                {
                    var model = _voxelModelManager.GetModel(e.Model.Name); //I'm SURE the model exist ! (It's the reason of this)
                    modelAndInstances.Value.VisualModel = model;
                    model.BuildMesh();
                    var keys = modelAndInstances.Value.Instances.Select(p => p.Key).ToList(); //Get all model instances where the instances where missing

                    foreach (var id in keys)
                    {
                        var instance = model.VoxelModel.CreateInstance();
                        modelAndInstances.Value.Instances[id] = instance;
                        _dynamicEntitiesDico[id].ModelInstance = instance;
                    }
                }
            }
        }

        #endregion

        #endregion

    }
}
