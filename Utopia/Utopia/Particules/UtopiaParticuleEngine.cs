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

namespace Utopia.Particules
{
    /// <summary>
    /// Base class to handle Particule on the Client
    /// Will contains all "utopia system" emitters.
    /// </summary>
    public class UtopiaParticuleEngine : ParticuleEngine
    {
        #region Private Variables
        private SharedFrameCB _sharedFrameCB;
        private CameraManager<ICameraFocused> _cameraManager;
        private InputsManager _inputsManager;
        private VisualWorldParameters _worldParameters;

        private ShaderResourceView _particulesSpritesResource;

        //Small Cube emitter, will emit on cube Change ! 
        private IChunkEntityImpactManager _chunkEntityImpactManager;
        private CubeEmitter _cubeEmitter;
        private IWorldChunks _worldChunks;

        private SpriteEmitter _staticEntityEmitter;
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
                             IWorldChunks worldChunks)
            : base(d3dEngine, sharedFrameCB.CBPerFrame)
        {
            _sharedFrameCB = sharedFrameCB;
            _cameraManager = cameraManager;
            _inputsManager = inputsManager;
            _worldParameters = worldParameters;
            _chunkEntityImpactManager = chunkEntityImpactManager;
            _worldChunks = worldChunks;

            _chunkEntityImpactManager.BlockReplaced += _chunkEntityImpactManager_BlockReplaced;
        }

        public override void BeforeDispose()
        {
            _chunkEntityImpactManager.BlockReplaced -= _chunkEntityImpactManager_BlockReplaced;
        }

        #region Public Methods
        public override void Initialize()
        {
            //Create the Cube Emitter
            _cubeEmitter = ToDispose(new CubeEmitter(ClientSettings.TexturePack + @"Terran/", @"ct*.png", ClientSettings.TexturePack + @"BiomesColors/", 5, 0.1f, _worldParameters, _worldChunks, 32));
            AddEmitter(_cubeEmitter);

            base.Initialize();
        }

        public override void LoadContent(DeviceContext context)
        {
            //Create the Static Entity billboard Particules Emitter
            ArrayTexture.CreateTexture2DFromFiles(_d3dEngine.Device, context, ClientSettings.TexturePack + @"Particules/", @"*.png", FilterFlags.Point, "ArrayTexture_Particules", out _particulesSpritesResource);
            ToDispose(_particulesSpritesResource);

            _staticEntityEmitter = new SpriteEmitter(this, DXStates.Samplers.UVWrap_MinMagMipLinear, _particulesSpritesResource, DXStates.Rasters.Default, DXStates.Blenders.Enabled, DXStates.DepthStencils.DepthReadEnabled);
            AddEmitter(_staticEntityEmitter);

            base.LoadContent(context);
        }

        public override void Update(S33M3DXEngine.Main.GameTime timeSpent)
        {
            StaticEntityEmiters();
            base.Update(timeSpent);
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

        public void StaticEntityEmiters()
        {
            if (_worldChunks.Chunks == null) return;

            //foreach (var chunk in _worldChunks.Chunks)
            //{
            //    if (chunk.isFrustumCulled == false && chunk.DistanceFromPlayer < _worldChunks.StaticEntityViewRange)
            //    {
            //        foreach (var entityType in chunk.VisualVoxelEntities)
            //        {
            //            foreach (var visualEntity in entityType.Value.Where(x => x.Entity.Particules.Count > 0))
            //            {
            //                VisualVoxelEntity entity = visualEntity; //

            //                for (int i = 0; i < entity.Entity.Particules.Count; i++)
            //                {
            //                    var p = entity.Entity.Particules[i];
            //                    var totsecond = (DateTime.Now - p.LastEmitedParticuleTime).TotalSeconds;
            //                    if (totsecond > 2)
            //                    {
            //                        _staticEntityEmitter.EmitParticule(1, 5, p.EmitVelocity, p.AccelerationForces, entity.Entity.Position);
            //                        p.LastEmitedParticuleTime = DateTime.Now;
            //                    }

            //                }

            //            }
            //        }
            //    }
            //}

        }
        #endregion
    }
}
