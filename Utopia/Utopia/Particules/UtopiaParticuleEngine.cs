using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using S33M3CoreComponents.Cameras;
using S33M3CoreComponents.Cameras.Interfaces;
using S33M3CoreComponents.Particules;
using S33M3DXEngine;
using Utopia.Components;
using S33M3CoreComponents.Inputs;
using Utopia.Action;
using SharpDX.Direct3D11;
using S33M3CoreComponents.Particules.Interfaces;
using S33M3DXEngine.Textures;
using Utopia.Shared.Interfaces;
using Utopia.Shared.Settings;
using SharpDX;
using S33M3Resources.Structs;
using S33M3DXEngine.RenderStates;
using Utopia.Shared.GameDXStates;
using S33M3CoreComponents.Particules.Emitters;
using Utopia.Shared.World;
using Utopia.Worlds.Chunks.ChunkEntityImpacts;
using Utopia.Shared.Configuration;
using Utopia.Worlds.Chunks;
using Utopia.Entities.Managers.Interfaces;
using Utopia.Entities.Voxel;
using Utopia.Shared.Entities;
using Utopia.Worlds.Weather;
using Utopia.Shared.Entities.Events;

namespace Utopia.Particules
{
    /// <summary>
    /// Base class to handle Particule on the Client
    /// Will contains all "utopia system" emitters.
    /// </summary>
    public class UtopiaParticuleEngine : ParticuleEngine
    {
        private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        public enum DynamicEntityParticuleType
        {
            Blood = 0
        }

        #region Private Variables
        private SharedFrameCB _sharedFrameCB;
        private CameraManager<ICameraFocused> _cameraManager;
        private InputsManager _inputsManager;
        private VisualWorldParameters _worldParameters;

        private ShaderResourceView _particulesSpritesResource;

        //Small Cube emitter, will emit on cube Change ! 
        private IChunkEntityImpactManager _chunkEntityImpactManager;
        private CubeEmitter _cubeEmitter;
        private IWorldChunks2D _worldChunks;
        private readonly ILandscapeManager _landscapeManager;

        private SpriteStaticEmitter _staticEntityEmitter;
        private SpriteDynamicEmitter _dynamicEntityEmitter;
        private IWeather _weather;

        private List<DynamicEntityParticule> DynamicEntityParticules;
        #endregion

        #region Public Properties
        public override Vector3D CameraPosition
        {
            get { return _cameraManager.ActiveCamera.WorldPosition.ValueInterp; }
        }
        #endregion

        public UtopiaParticuleEngine(D3DEngine d3dEngine,
                             SharedFrameCB sharedFrameCB,
                             CameraManager<ICameraFocused> cameraManager,
                             InputsManager inputsManager,
                             VisualWorldParameters worldParameters,
                             IChunkEntityImpactManager chunkEntityImpactManager,
                             IWorldChunks2D worldChunks,
                             ILandscapeManager landscapeManager,
                             IWeather weather)
            : base(d3dEngine, sharedFrameCB.CBPerFrame)
        {
            _sharedFrameCB = sharedFrameCB;
            _cameraManager = cameraManager;
            _inputsManager = inputsManager;
            _worldParameters = worldParameters;
            _chunkEntityImpactManager = chunkEntityImpactManager;
            _worldChunks = worldChunks;
            _landscapeManager = landscapeManager;
            _weather = weather;

            _chunkEntityImpactManager.BlockReplaced += _chunkEntityImpactManager_BlockReplaced;

            this.IsDefferedLoadContent = true;
        }

        public override void BeforeDispose()
        {
            _chunkEntityImpactManager.BlockReplaced -= _chunkEntityImpactManager_BlockReplaced;
        }

        #region Public Methods
        public override void Initialize()
        {
            //Create the Cube Emitter
            _cubeEmitter = ToDispose(new CubeEmitter(ClientSettings.TexturePack + @"Terran/", @"*.png", ClientSettings.TexturePack + @"BiomesColors/", 5, 0.1f, _worldParameters, _worldChunks, _landscapeManager, 32, _weather));
            AddEmitter(_d3dEngine.ImmediateContext, _cubeEmitter);

            //Create the various dynamic Particules type
            DynamicEntityParticules = new List<DynamicEntityParticule>();
            //Blood
            DynamicEntityParticule blood = new DynamicEntityParticule();
            blood.ParticuleType = EntityParticuleType.Billboard;
            blood.ParticuleId = 1; //Circle
            blood.ApplyWindForce = false;
            blood.Size = new Vector2(0.1f, 0.1f);
            blood.ParticuleColor = SharpDX.Color.Red;
            blood.ParticuleLifeTime = 0.4f;
            blood.ParticuleLifeTimeRandomness = 0.1f;
            blood.EmitVelocityRandomness = new Vector3(0.4f, 0.2f, 0.4f);
            blood.AccelerationForce = new Vector3(-1.5f, -2.1f, -1.5f);
            DynamicEntityParticules.Add(blood);

            base.Initialize();
        }

        public override void LoadContent(DeviceContext context)
        {
            //Create the Static Entity billboard Particules Emitter
            ArrayTexture.CreateTexture2DFromFiles(_d3dEngine.Device, context, ClientSettings.TexturePack + @"Particules/", @"*.png", FilterFlags.Point, "ArrayTexture_Particules", out _particulesSpritesResource);
            ToDispose(_particulesSpritesResource);

            _staticEntityEmitter = new SpriteStaticEmitter(this, DXStates.Samplers.UVWrap_MinMagMipLinear, _particulesSpritesResource, DXStates.Rasters.Default, DXStates.Blenders.Enabled, DXStates.DepthStencils.DepthReadEnabled, _weather);
            AddEmitter(context, _staticEntityEmitter);

            _dynamicEntityEmitter = new SpriteDynamicEmitter(this, DXStates.Samplers.UVWrap_MinMagMipLinear, _particulesSpritesResource, DXStates.Rasters.Default, DXStates.Blenders.Enabled, DXStates.DepthStencils.DepthReadEnabled, _weather);
            AddEmitter(context, _dynamicEntityEmitter);

            base.LoadContent(context);
        }

        public override void FTSUpdate(S33M3DXEngine.Main.GameTime timeSpent)
        {
            StaticEntityEmiters();
            base.FTSUpdate(timeSpent);
        }
        #endregion

        #region Private Methods

        //Adding cube particule on cube destroyed !
        private void _chunkEntityImpactManager_BlockReplaced(object sender, LandscapeBlockReplacedEventArgs e)
        {
            //Cube has been destroyed
            if (e.NewBlockType == WorldConfiguration.CubeId.Air)
            {
                //Emit Colored particules
                _cubeEmitter.EmitParticuleForCubeDestruction(40, e.PreviousBlock, e.Position, ref _cameraManager.ActiveCamera.WorldPosition.Value);
            }
        }

        //Look inside all Surrending chunks for statis entities that are emitting particules
        private void StaticEntityEmiters()
        {
            if (_worldChunks.Chunks == null) return;
            foreach (VisualChunk chunk in _worldChunks.Chunks.Where(x => x.Graphics.IsFrustumCulled == false && x.DistanceFromPlayer < _worldChunks.StaticEntityViewRange && x.State == ChunkState.DisplayInSyncWithMeshes && x.ThreadStatus == S33M3DXEngine.Threading.ThreadsManager.ThreadStatus.Idle))
            {
                foreach (var entityWithMeta in chunk.EmitterStaticEntities)
                {
                    switch (entityWithMeta.Particule.ParticuleType)
                    {
                        case EntityParticuleType.Billboard:
                            if ((DateTime.Now - entityWithMeta.EntityLastEmitTime).TotalSeconds > entityWithMeta.Particule.EmittedParticuleRate)
                            {
                                _staticEntityEmitter.EmitParticule(
                                                                   entityWithMeta.Particule,
                                                                   entityWithMeta.Entity.Position
                                                                   );
                                entityWithMeta.EntityLastEmitTime = DateTime.Now;
                            }
                            break;
                        default:
                            break;
                    }
                }
            }
        }

        public void AddDynamicEntityParticules(Vector3 StartUpPosition, Vector3 StartUpPositionNormal, DynamicEntityParticuleType type)
        {
            AddDynamicEntityParticules(new Vector3D(StartUpPosition), StartUpPositionNormal, type);
        }

        public void AddDynamicEntityParticules(Vector3D StartUpPosition, Vector3 StartUpPositionNormal, DynamicEntityParticuleType type)
        {
            if (StartUpPosition == default(Vector3D)) return;

            Vector3 velocity = StartUpPositionNormal;
            velocity *= 1.5f;

            _dynamicEntityEmitter.EmitParticule(DynamicEntityParticules[0], StartUpPosition, 15, velocity);
        }


        #endregion
    }
}
