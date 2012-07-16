using System;
using System.Linq;
using System.Collections.Generic;
using S33M3CoreComponents.Cameras;
using S33M3CoreComponents.Cameras.Interfaces;
using S33M3CoreComponents.WorldFocus;
using S33M3DXEngine;
using S33M3DXEngine.RenderStates;
using S33M3Resources.Structs;
using S33M3Resources.Structs.Vertex;
using SharpDX;
using Utopia.Entities.Voxel;
using Ninject;
using Utopia.Entities.Managers.Interfaces;
using Utopia.Entities.Renderer.Interfaces;
using Utopia.Shared.Chunks;
using Utopia.Shared.Entities.Events;
using Utopia.Shared.Entities.Interfaces;
using S33M3DXEngine.Main;
using SharpDX.Direct3D11;
using Utopia.Shared.Entities.Models;
using Utopia.Shared.GameDXStates;
using Utopia.Shared.Settings;
using Utopia.Shared.World;
using Utopia.Worlds.SkyDomes;
using UtopiaContent.Effects.Entities;

namespace Utopia.Entities.Managers
{
    /// <summary>
    /// Keeps a collection of IDynamicEntity received from server.
    /// Responsible to Draw them, also used to check collision detection with Player
    /// </summary>
    public class DynamicEntityManager : DrawableGameComponent, IDynamicEntityManager
    {
        /// <summary>
        /// Allows to group instances by model to perform instanced drawing
        /// </summary>
        private class ModelAndInstances
        {
            public VisualVoxelModel VisualModel;                    //A model
            public Dictionary<uint, VoxelModelInstance> Instances;  //Instanced model list
        }
        
        private HLSLVoxelModelInstanced _voxelModelEffect;
        private readonly Dictionary<uint, VisualDynamicEntity> _dynamicEntitiesDico = new Dictionary<uint, VisualDynamicEntity>();
        private readonly D3DEngine _d3DEngine;
        private readonly VoxelModelManager _voxelModelManager;
        private readonly CameraManager<ICameraFocused> _camManager;
        private readonly WorldFocusManager _worldFocusManager;
        private readonly VisualWorldParameters _visualWorldParameters;
        private SingleArrayChunkContainer _chunkContainer;
        public List<IVisualEntityContainer> DynamicEntities { get; set; }

        // collection of the models and instances
        private Dictionary<string, ModelAndInstances> _models = new Dictionary<string, ModelAndInstances>();
        
        [Inject]
        public SingleArrayChunkContainer ChunkContainer
        {
            get { return _chunkContainer; }
            set { _chunkContainer = value; }
        }

        [Inject]
        public ISkyDome SkyDome { get; set; }

        public event EventHandler<DynamicEntityEventArgs> EntityAdded;

        private void OnEntityAdded(DynamicEntityEventArgs e)
        {
            var handler = EntityAdded;
            if (handler != null) handler(this, e);
        }

        public event EventHandler<DynamicEntityEventArgs> EntityRemoved;

        public void OnEntityRemoved(DynamicEntityEventArgs e)
        {
            var handler = EntityRemoved;
            if (handler != null) handler(this, e);
        }


        public DynamicEntityManager(D3DEngine d3DEngine, 
                                    VoxelModelManager voxelModelManager, 
                                    CameraManager<ICameraFocused> camManager,
                                    WorldFocusManager worldFocusManager,
                                    VisualWorldParameters visualWorldParameters)
        {
            _d3DEngine = d3DEngine;
            _voxelModelManager = voxelModelManager;
            _camManager = camManager;
            _worldFocusManager = worldFocusManager;
            _visualWorldParameters = visualWorldParameters;

            _voxelModelManager.VoxelModelAvailable += VoxelModelManagerVoxelModelReceived;

            DynamicEntities = new List<IVisualEntityContainer>();
        }

        void VoxelModelManagerVoxelModelReceived(object sender, VoxelModelReceivedEventArgs e)
        {
            //ForEach different Voxel model in local collection
            foreach (var modelAndInstances in _models)
            {
                //Check if the model receive is already existing. (By model name)
                if (modelAndInstances.Key == e.Model.Name)
                {
                    var model = _voxelModelManager.GetModel(e.Model.Name);
                    modelAndInstances.Value.VisualModel = model;
                    model.BuildMesh();
                    var keys = modelAndInstances.Value.Instances.Select(p => p.Key).ToList();

                    foreach (var id in keys)
                    {
                        var instance = model.VoxelModel.CreateInstance();
                        modelAndInstances.Value.Instances[id] = instance;
                        _dynamicEntitiesDico[id].ModelInstance = instance;
                    }
                }
            }
        }

        public override void BeforeDispose()
        {
            foreach (var item in _dynamicEntitiesDico.Values) item.Dispose();
        }

        public override void Initialize()
        {
            
        }

        public override void LoadContent(DeviceContext context)
        {
            _voxelModelEffect = new HLSLVoxelModelInstanced(_d3DEngine.Device, ClientSettings.EffectPack + @"Entities\VoxelModelInstanced.hlsl", VertexVoxelInstanced.VertexDeclaration);
        }

        public override void UnloadContent()
        {
            this.DisableComponent();
            foreach (var item in _dynamicEntitiesDico.Values) item.Dispose();
            _dynamicEntitiesDico.Clear();
            this.IsInitialized = false;
        }

        #region Private Methods
        private VisualDynamicEntity CreateVisualEntity(IDynamicEntity entity)
        {
            return new VisualDynamicEntity(entity, new VisualVoxelEntity(entity, _voxelModelManager));
        }
        #endregion

        #region Public Methods
        public override void Update(GameTime timeSpent)
        {
            foreach (var entity in _dynamicEntitiesDico.Values)
            {
                entity.Update(timeSpent);
            }
        }

        public override void Interpolation(double interpolationHd, float interpolationLd, long timePassed)
        {
            foreach (var entity in _dynamicEntitiesDico.Values)
            {
                entity.Interpolation(interpolationHd, interpolationLd, timePassed);

                // update model color, get the cube where model is
                var block = _chunkContainer.GetCube(entity.WorldPosition.ValueInterp);
                if (block.Id == 0)
                {
                    // we take the max color
                    var sunPart = (float)block.EmissiveColor.A / 255;
                    var sunColor = SkyDome.SunColor * sunPart;
                    var resultColor = Color3.Max(block.EmissiveColor.ToColor3(), sunColor);

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
            //Applying Correct Render States
            RenderStatesRepo.ApplyStates(DXStates.Rasters.Default, DXStates.Blenders.Disabled, DXStates.DepthStencils.DepthEnabled);
            _voxelModelEffect.Begin(context);
            _voxelModelEffect.CBPerFrame.Values.LightDirection = SkyDome.LightDirection;
            _voxelModelEffect.CBPerFrame.Values.ViewProjection = Matrix.Transpose(_camManager.ActiveCamera.ViewProjection3D);
            _voxelModelEffect.CBPerFrame.IsDirty = true;
            _voxelModelEffect.Apply(context);

            //For each existing model
            foreach (var modelAndInstances in _models)
            {
                //For each instance of the model
                foreach (var pairs in modelAndInstances.Value.Instances)
                {
                    var entityToRender = _dynamicEntitiesDico[pairs.Key];

                    //Draw only the entities that are in Client view range
                    if (_visualWorldParameters.WorldRange.Contains(entityToRender.VisualEntity.Position.ToCubePosition()))
                    {
                        pairs.Value.World = Matrix.Scaling(1f / 16) * entityToRender.VisualEntity.World;
                        pairs.Value.LightColor = entityToRender.ModelLight.ValueInterp;
                    }
                }

                var instancesToDraw = modelAndInstances.Value.Instances.Values;
                modelAndInstances.Value.VisualModel.DrawInstanced(_d3DEngine.ImmediateContext, _voxelModelEffect, instancesToDraw);
            }
        }

        public void AddEntity(IDynamicEntity entity)
        {
            //Do we already have thise entity ??
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
                _dynamicEntitiesDico.Add(entity.DynamicId, newEntity);
                DynamicEntities.Add(newEntity);

                //If the Model do have a Voxel Model (Search by Name)
                if (modelWithInstances.VisualModel != null)
                {
                    //Create a new Instance of the Model
                    var instance = new VoxelModelInstance(modelWithInstances.VisualModel.VoxelModel);
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

        public void RemoveEntityById(uint entityId,bool dispose=true)
        {
            VisualDynamicEntity entity;
            if (_dynamicEntitiesDico.TryGetValue(entityId, out entity))
            {
                ModelAndInstances instances;
                if (!_models.TryGetValue(entity.VisualEntity.VoxelEntity.ModelName, out instances))
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
            if (_dynamicEntitiesDico.TryGetValue(p,out e))
            {
                return e.DynamicEntity;
            }
            return null;
        }

        public IEnumerator<VisualVoxelEntity> EnumerateVisualEntities()
        {
            foreach (var visualEntityContainer in DynamicEntities)
            {
                yield return visualEntityContainer.VisualEntity;
            }

        }
        #endregion

    }
}
