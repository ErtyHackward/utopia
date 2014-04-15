using System;
using System.Collections.Generic;
using S33M3CoreComponents.Cameras;
using S33M3CoreComponents.Cameras.Interfaces;
using S33M3CoreComponents.Maths;
using S33M3CoreComponents.WorldFocus;
using S33M3DXEngine;
using S33M3DXEngine.Threading;
using S33M3Resources.Effects.Basics;
using S33M3Resources.Structs;
using SharpDX;
using SharpDX.Direct3D11;
using Utopia.Entities;
using Utopia.Entities.Voxel;
using Utopia.Resources.ModelComp;
using Utopia.Shared.Chunks;
using Utopia.Shared.Configuration;
using Utopia.Shared.Entities.Concrete;
using Utopia.Shared.Entities.Concrete.Interface;
using Utopia.Shared.Entities.Events;
using Utopia.Shared.Entities.Interfaces;
using Utopia.Shared.Entities.Models;
using Utopia.Shared.Structs;
using Utopia.Shared.Structs.Landscape;
using Utopia.Shared.World;
using Utopia.Worlds.Chunks.ChunkEntityImpacts;
using Utopia.Shared.Entities;

namespace Utopia.Worlds.Chunks
{
    public abstract class VisualChunkBase : CompressibleChunk, IDisposable
    {
        private struct TreeBpSeed
        {
            public int TreeBlueprint;
            public int TreeSeed;

            public bool Equals(TreeBpSeed other)
            {
                return TreeBlueprint == other.TreeBlueprint && TreeSeed == other.TreeSeed;
            }

            public override bool Equals(object obj)
            {
                if (ReferenceEquals(null, obj)) return false;
                return obj is TreeBpSeed && Equals((TreeBpSeed)obj);
            }

            public override int GetHashCode()
            {
                unchecked
                {
                    return (TreeBlueprint * 397) ^ TreeSeed;
                }
            }
        }

        #region Private variables
        private readonly object _syncRoot = new object();
        /// <summary>
        /// Dictionary by the model name of the entity
        /// </summary>
        private readonly Dictionary<string, List<VisualVoxelEntity>> _visualVoxelEntities;
        private readonly VisualWorldParameters _visualWorldParameters;
        private readonly CameraManager<ICameraFocused> _cameraManager;
        private readonly FastRandom _rnd = new FastRandom();
        private readonly D3DEngine _d3DEngine;
        private readonly WorldFocusManager _worldFocusManager;
        private readonly WorldChunks _worldChunkManager;
        private readonly VoxelModelManager _voxelModelManager;
        private readonly IChunkEntityImpactManager _chunkEntityImpactManager;
        private readonly Dictionary<TreeBpSeed,VisualVoxelModel> _cachedTrees;

        private Range3I _cubeRange;
        
        #endregion

        #region Public properties/Variable

        public Double DistanceFromPlayer { get; set; }
        public Vector3D ChunkCenter { get; set; } 
        public Vector2I ChunkPositionBlockUnit { get; private set; } // Gets or sets current chunk position in Block Unit
        
        public ChunkState State;
        
        public bool IsOutsideLightSourcePropagated { get; set; }
        public ThreadsManager.ThreadStatus ThreadStatus { get; set; }        // Thread status of the chunk, used for sync.
        public string ThreadLockedBy { get; set; }

        public int UpdateOrder { get; set; }              // Variable for sync drawing at rebuild time.
        public bool IsBorderChunk { get; set; }               // Set to true if the chunk is located at the border of the visible world !
        

        private bool _isServerRequested;
        private bool _isServerResyncMode;
        private DateTime _serverRequestTime;
        
        /// <summary>
        /// Contains chunk graphics properties for rendering
        /// </summary>
        public ChunkGraphics Graphics { get; private set; }

        public VisualChunk[] EightSurroundingChunks;
        public VisualChunk[] FourSurroundingChunks;


        public static Int64 ComputeChunkId(int PosiX, int PosiY)
        {
            var hashLong = new MathHelper.IntsToLong();
            hashLong.LeftInt32 = PosiX;
            hashLong.RightInt32 = PosiY;
            return hashLong.LongValue;
        }

        /// <summary>
        /// Gets or sets the value of chunk opaque. Allows to create slowly appearing effect
        /// </summary>
        public FTSValue<float> PopUpValue = new FTSValue<float>(0.0f);
        
        public Matrix World;                                  // The chunk World matrix ==> Not a property, to be sure it will be direct variables acces !!
        public BoundingBox ChunkWorldBoundingBox;             // The chunk World BoundingBox ==> Not a property, to be sure it will be direct variables acces !!

        public bool IsServerRequested
        {
            get { return _isServerRequested; }
            set { _isServerRequested = value; if (value) _serverRequestTime = DateTime.Now; }
        }

        public bool IsServerResyncMode
        {
            get { return _isServerResyncMode; }
            set { _isServerResyncMode = value; }
        }

        public DateTime ServerRequestTime
        {
            get { return _serverRequestTime; }
        }

        public List<ILightEmitterEntity> OutOfChunkLightSourceStaticEntities;
        public List<EntityMetaData> EmitterStaticEntities;
        public List<IItem> SoundStaticEntities;

        public int StorageRequestTicket { get; set; }

        public Range3I CubeRange
        {
            get { return _cubeRange; }
            set
            {
                _cubeRange = value;
                RangeChanged();
            }
        }
        
        //Use to display bounding box around chunk in debug mode only (Quite slow and not optimized)
#if DEBUG
        public BoundingBox3D ChunkBoundingBoxDisplay;
        private HLSLVertexPositionColor _blockpickedUPEffect;
#endif

        #endregion

        /// <summary>
        /// Occurs when chunk mesh updated
        /// </summary>
        public event EventHandler ChunkMeshUpdated;
        internal void OnChunkMeshUpdated()
        {
            var handler = ChunkMeshUpdated;
            if (handler != null) handler(this, EventArgs.Empty);
        }

        /// <summary>
        /// Occurs when chunk is ready to draw ;)
        /// </summary>
        public event EventHandler ReadyToDraw;
        internal void OnReadyToDraw()
        {
            var handler = ReadyToDraw;
            if (handler != null) handler(this, EventArgs.Empty);
        }

        public VisualChunkBase(
                            D3DEngine d3DEngine, 
                            WorldFocusManager worldFocusManager, 
                            VisualWorldParameters visualWorldParameter, 
                            Range3I cubeRange, 
                            CameraManager<ICameraFocused> cameraManager,
                            WorldChunks worldChunkManager,
                            VoxelModelManager voxelModelManager,
                            IChunkEntityImpactManager chunkEntityImpactManager, 
                            ChunkDataProvider provider = null)
            : base(provider)
        {
            _cachedTrees = new Dictionary<TreeBpSeed, VisualVoxelModel>();

            Graphics = new ChunkGraphics(this, d3DEngine);

            _d3DEngine = d3DEngine;
            _worldFocusManager = worldFocusManager;
            _worldChunkManager = worldChunkManager;
            _chunkEntityImpactManager = chunkEntityImpactManager;
#if DEBUG
            _blockpickedUPEffect = new HLSLVertexPositionColor(_d3DEngine.Device);
#endif
            
            _visualWorldParameters = visualWorldParameter;
            
            _cameraManager = cameraManager;
            _voxelModelManager = voxelModelManager;
            _visualVoxelEntities = new Dictionary<string, List<VisualVoxelEntity>>();
            EmitterStaticEntities = new List<EntityMetaData>();
            OutOfChunkLightSourceStaticEntities = new List<ILightEmitterEntity>();
            SoundStaticEntities = new List<IItem>();
            CubeRange = cubeRange;
            State = ChunkState.Empty;
            Entities.EntityAdded += EntitiesEntityAdded;
            Entities.EntityRemoved += EntitiesEntityRemoved;
            Entities.CollectionCleared += EntitiesCollectionCleared;
        }

        #region Public methods

        public void CreateVisualEntities()
        {
            foreach (var entity in Entities.EnumerateFast())
            {
                EntitiesEntityAdded(null,
                                     new Shared.Entities.Events.EntityCollectionEventArgs
                                     {
                                         Chunk = this,
                                         Entity = entity
                                     });
            }
        }

        public void RefreshBorderChunk()
        {
            IsBorderChunk = isBorderChunk(ChunkPositionBlockUnit.X, ChunkPositionBlockUnit.Y);

            //Get the surrounding chunks if BorderChunk is null
            if (IsBorderChunk == false)
            {
                EightSurroundingChunks = _worldChunkManager.GetEightSurroundingChunkFromChunkCoord(Position.X, Position.Z);
                FourSurroundingChunks = _worldChunkManager.GetFourSurroundingChunkFromChunkCoord(Position.X, Position.Z);
            }
            else
            {
                EightSurroundingChunks = new VisualChunk[0];
                FourSurroundingChunks = new VisualChunk[0];
            }
        }

        public bool SurroundingChunksMinimumState(ChunkState minimumState)
        {
            foreach (var chunk in EightSurroundingChunks)
            {
                if (chunk.State < minimumState) return false;
            }
            return true;
        }


#if DEBUG
        public void DrawDebugBoundingBox(DeviceContext context)
        {
            ChunkBoundingBoxDisplay.Draw(context, _cameraManager.ActiveCamera);
        }
#endif

        void EntitiesCollectionCleared(object sender, EventArgs e)
        {
            lock (_syncRoot)
            {
                foreach (var entityList in _visualVoxelEntities.Values)
                {
                    foreach (IDisposable i in entityList)
                    {
                        i.Dispose();
                    }
                }

                _visualVoxelEntities.Clear();
            }

            OutOfChunkLightSourceStaticEntities.Clear();
            EmitterStaticEntities.Clear();
            SoundStaticEntities.Clear();
        }

        private void EntitiesEntityRemoved(object sender, EntityCollectionEventArgs e)
        {
            RemoveVoxelEntity(e);
            RemoveParticuleEmitterEntity(e);
            RemoveSoundEntity(e);
        }

        /// <summary>
        /// New Static Entity added to the chunk
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void EntitiesEntityAdded(object sender, Shared.Entities.Events.EntityCollectionEventArgs e)
        {
            AddVoxelEntity(e);
            AddParticuleEmitterEntity(e);
            AddSoundEntity(e);
            AddOutOfChunkLightSourceStaticEntity(e);
        }

        public abstract TerraCubeResult GetCube(Vector3I internalPosition);

        private void AddVoxelEntity(EntityCollectionEventArgs e)
        {
            var voxelEntity = e.Entity as IVoxelEntity;
            if (voxelEntity == null) 
                return; //My static entity is not a Voxel Entity => Not possible to render it so !!!

            //Create the Voxel Model Instance for the Item
            VisualVoxelModel model = null;
            if (!string.IsNullOrEmpty(voxelEntity.ModelName)) 
                model = _voxelModelManager.GetModel(voxelEntity.ModelName, false);
            if (model != null && voxelEntity.ModelInstance == null) //The model blueprint is existing, and I need to create an instance of it !
            {
                var treeGrowing = e.Entity as TreeGrowingEntity;
                if (treeGrowing != null)
                {
                    if (treeGrowing.Scale > 0)
                    {
                        // we need to use generated voxel model
                        TreeBpSeed key;
                        key.TreeBlueprint = treeGrowing.TreeTypeId;
                        key.TreeSeed = treeGrowing.TreeRndSeed;

                        VisualVoxelModel treeModel;

                        if (_cachedTrees.TryGetValue(key, out treeModel))
                        {
                            model = treeModel;
                        }
                        else
                        {
                            var voxelModel = VoxelModel.GenerateTreeModel(treeGrowing.TreeRndSeed,
                                _visualWorldParameters.WorldParameters.Configuration.TreeBluePrintsDico[
                                    treeGrowing.TreeTypeId]);

                            model = new VisualVoxelModel(voxelModel, _voxelModelManager.VoxelMeshFactory);
                            model.BuildMesh();

                            _cachedTrees.Add(key, model);
                        }
                    }
                }

                voxelEntity.ModelInstance = new VoxelModelInstance(model.VoxelModel);

                //Assign state in case of growing entity !
                var growingEntity = e.Entity as PlantGrowingEntity;
                if (growingEntity != null)
                {
                    voxelEntity.ModelInstance.SetState(growingEntity.GrowLevels[growingEntity.CurrentGrowLevelIndex].ModelState);
                }

                var visualVoxelEntity = new VisualVoxelEntity(voxelEntity, model, _voxelModelManager);
                //Get default world translation
                Matrix instanceTranslation = Matrix.Translation(voxelEntity.Position.AsVector3());

                //Apply special rotation to the creation instance
                Quaternion instanceRotation = Quaternion.Identity;
                if (voxelEntity is IRndYRotation && ((IRndYRotation)voxelEntity).RndRotationAroundY)
                {
                    instanceRotation = Quaternion.RotationAxis(Vector3.UnitY, (float)(_rnd.NextDouble() * MathHelper.TwoPi));
                }
                else if (voxelEntity is IItem)
                {
                    var item = voxelEntity as IItem;
                    instanceRotation = item.Rotation;
                }

                //Apply special scaling to created entity (By default all blue print are 16 times too big.
                Matrix instanceScaling = Matrix.Scaling(1.0f / 16.0f);

                if (treeGrowing != null && treeGrowing.Scale > 0)
                {
                    instanceScaling = Matrix.Scaling(treeGrowing.Scale);
                }

                //Create the World transformation matrix for the instance.
                //We take the Model instance world matrix where we add a Rotation and scaling proper to the instance
                visualVoxelEntity.VoxelEntity.ModelInstance.World = instanceScaling * instanceTranslation;
                visualVoxelEntity.VoxelEntity.ModelInstance.Rotation = instanceRotation;

                var result = GetCube(visualVoxelEntity.VoxelEntity.Position.ToCubePosition());


                if (result.IsValid)
                {
                    visualVoxelEntity.BlockLight = result.Cube.EmissiveColor;
                }
                else
                {
                    visualVoxelEntity.BlockLight = new ByteColor(255, 255, 255, 255);
                }


                if (visualVoxelEntity.VisualVoxelModel.Initialized == false)
                {
                    visualVoxelEntity.VisualVoxelModel.BuildMesh();
                }

                if (voxelEntity.ModelInstance.CanPlay("Idle"))
                {
                    voxelEntity.ModelInstance.Play("Idle", true);
                }

                lock (_syncRoot)
                {
                    List<VisualVoxelEntity> list;
                    if (_visualVoxelEntities.TryGetValue(voxelEntity.ModelName, out list))
                    {
                        list.Add(visualVoxelEntity);
                    }
                    else
                    {
                        _visualVoxelEntities.Add(voxelEntity.ModelName, new List<VisualVoxelEntity> { visualVoxelEntity });
                    }
                }

                var lightEntity = e.Entity as ILightEmitterEntity;
                if (e.AtChunkCreationTime == false && lightEntity != null)
                {
                    //Get the Cube where is located the entity
                    var entityWorldPosition = lightEntity.Position;
                    var entityBlockPosition = new Vector3I(MathHelper.Floor(entityWorldPosition.X),
                                                                MathHelper.Floor(entityWorldPosition.Y),
                                                                MathHelper.Floor(entityWorldPosition.Z));
                    //new TerraCubeWithPosition(entityBlockPosition, WorldConfiguration.CubeId.Air, _visualWorldParameters.WorldParameters.Configuration), 
                    this.UpdateOrder = 1;
                    var cubeRange = new Range3I
                    {
                        Position = new Vector3I(entityBlockPosition.X, 0, entityBlockPosition.Z),
                        Size = Vector3I.One
                    };
                    _chunkEntityImpactManager.CheckImpact(this, cubeRange);
                }
            }
        }

        private void RemoveVoxelEntity(EntityCollectionEventArgs e)
        {
            //Remove the entity from Visual Model
            lock (_syncRoot)
            {
                foreach (var pair in _visualVoxelEntities)
                {
                    pair.Value.RemoveAll(x => x.Entity == e.Entity);
                }
            }

            var lightEntity = e.Entity as ILightEmitterEntity;
            if (e.AtChunkCreationTime == false && lightEntity != null)
            {
                //Get the Cube where is located the entity
                var entityWorldPosition = lightEntity.Position;
                var entityBlockPosition = new Vector3I(MathHelper.Floor(entityWorldPosition.X),
                                                            MathHelper.Floor(entityWorldPosition.Y),
                                                            MathHelper.Floor(entityWorldPosition.Z));

                this.UpdateOrder = 1;
                //Compute the Range impacted by the cube change
                var cubeRange = new Range3I
                {
                    Position = new Vector3I(entityBlockPosition.X, 0, entityBlockPosition.Z),
                    Size = Vector3I.One
                };
                _chunkEntityImpactManager.CheckImpact(this, cubeRange);
            }
        }

        private void AddSoundEntity(EntityCollectionEventArgs e)
        {
            var item = e.Entity as IItem;
            if (item == null || item.EmittedSound == null || item.EmittedSound.FilePath == null) return;
            SoundStaticEntities.Add(item);
        }
        
        private void RemoveSoundEntity(EntityCollectionEventArgs e)
        {
            var item = e.Entity as IItem;
            if (item == null || item.EmittedSound == null || item.EmittedSound.FilePath == null) return;
            SoundStaticEntities.Remove(item);
        }

        private void AddOutOfChunkLightSourceStaticEntity(EntityCollectionEventArgs e)
        {
            var item = e.Entity as ILightEmitterEntity;
            if (item == null) return;
            if (this.CubeRange.Contains(item.Position.ToCubePosition()) == false)
            {
                OutOfChunkLightSourceStaticEntities.Add(item);
            }
        }

        private void RemoveOutOfChunkLightSourceStaticEntity(EntityCollectionEventArgs e)
        {
            var item = e.Entity as ILightEmitterEntity;
            if (item == null) return;
            OutOfChunkLightSourceStaticEntities.Remove(item);
        }

        private void AddParticuleEmitterEntity(EntityCollectionEventArgs e)
        {
            if (e.Entity.Particules == null) return;
            foreach (var entityParticules in e.Entity.Particules)
            {
                EmitterStaticEntities.Add(new EntityMetaData() { Entity = e.Entity, Particule = entityParticules, EntityLastEmitTime = DateTime.Now });
            }
        }

        private void RemoveParticuleEmitterEntity(EntityCollectionEventArgs e)
        {
            if (e.Entity.Particules == null) return;
            EmitterStaticEntities.RemoveAll(x => x.Entity == e.Entity);
        }

        /// <summary>
        /// Allows to enumerate entities in threadsafe way
        /// </summary>
        /// <returns></returns>
        public IEnumerable<VisualVoxelEntity> AllEntities()
        {
            lock (_syncRoot)
            {
                foreach (var pair in _visualVoxelEntities)
                {
                    foreach (var entity in pair.Value)
                    {
                        yield return entity;
                    }
                }
            }
        }

        /// <summary>
        /// Allows to enumerate a group of entity with the given model name in a threadsafe way
        /// </summary>
        /// <param name="modelName"></param>
        /// <returns></returns>
        public IEnumerable<VisualVoxelEntity> AllEntities(string modelName)
        {
            lock (_syncRoot)
            {
                foreach (var entity in _visualVoxelEntities[modelName])
                {
                    yield return entity;
                }
            }
        }

        /// <summary>
        /// Allows to enumerate entities grouped by model in a threadsafe way
        /// </summary>
        /// <returns></returns>
        public IEnumerable<KeyValuePair<string, List<VisualVoxelEntity>>> AllPairs()
        {
            lock (_syncRoot)
            {
                foreach (var visualVoxelEntity in _visualVoxelEntities)
                {
                    yield return visualVoxelEntity;
                }
            }
        }

        #endregion

        #region Privates Methods

        /// <summary>
        /// Is my chunk at the edge of the visible world ?
        /// </summary>
        /// <param name="X">Chunk world X position</param>
        /// <param name="Z">Chunk world Z position</param>
        /// <returns>True if the chunk is at border</returns>
        private bool isBorderChunk(int X, int Z)
        {
            if (X == _visualWorldParameters.WorldRange.Position.X ||
               Z == _visualWorldParameters.WorldRange.Position.Z ||
               X == _visualWorldParameters.WorldRange.Max.X - AbstractChunk.ChunkSize.X ||
               Z == _visualWorldParameters.WorldRange.Max.Z - AbstractChunk.ChunkSize.Z)
            {
                return true;
            }
            return false;
        }

        private void RefreshWorldMatrix()
        {
            Matrix.Translation(_cubeRange.Position.X, _cubeRange.Position.Y, _cubeRange.Position.Z, out World); //Create a matrix for world translation

            //Refresh the bounding Box to make it in world coord.
            ChunkWorldBoundingBox.Minimum = new Vector3(_cubeRange.Position.X, _cubeRange.Position.Y, _cubeRange.Position.Z);
            ChunkWorldBoundingBox.Maximum = new Vector3(_cubeRange.Max.X, _cubeRange.Max.Y, _cubeRange.Max.Z);
        }

        private void RangeChanged() // Start it also if the World offset Change !!!
        {
            ChunkPositionBlockUnit = new Vector2I() { X = _cubeRange.Position.X, Y = _cubeRange.Position.Z };

            Position = new Vector3I() { X = _cubeRange.Position.X / AbstractChunk.ChunkSize.X, Y = 0, Z = _cubeRange.Position.Z / AbstractChunk.ChunkSize.Z };
            
            ChunkCenter = new Vector3D(_cubeRange.Position.X + (_cubeRange.Max.X - _cubeRange.Position.X) / 2.0,
                           _cubeRange.Position.Y + (_cubeRange.Max.Y - _cubeRange.Position.Y) / 2.0,
                           _cubeRange.Position.Z + (_cubeRange.Max.Z - _cubeRange.Position.Z) / 2.0);

#if DEBUG
            ChunkBoundingBoxDisplay = new BoundingBox3D(_d3DEngine, _worldFocusManager, new Vector3((float)(CubeRange.Max.X - CubeRange.Position.X), (float)(CubeRange.Max.Y - CubeRange.Position.Y), (float)(CubeRange.Max.Z - CubeRange.Position.Z)), _blockpickedUPEffect, Color.Tomato);
            ChunkBoundingBoxDisplay.Update(ChunkCenter.AsVector3(), Vector3.One, 0);
#endif

            RefreshWorldMatrix();

            OutOfChunkLightSourceStaticEntities.Clear();
            SoundStaticEntities.Clear();
            lock (_syncRoot)
                _visualVoxelEntities.Clear();
            EmitterStaticEntities.Clear();
        }

        #endregion

        public void Dispose()
        {
            Graphics.Dispose();

            Entities.EntityAdded -= EntitiesEntityAdded;
            Entities.EntityRemoved -= EntitiesEntityRemoved;
#if DEBUG
            _blockpickedUPEffect.Dispose();
            ChunkBoundingBoxDisplay.Dispose();
#endif

        }
    }
}
