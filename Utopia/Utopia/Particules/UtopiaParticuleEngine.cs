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

        //Small Cube emitter, will emit on cube Change ! 
        private IChunkEntityImpactManager _chunkEntityImpactManager;
        private CubeEmitter _cubeEmitter;
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
                             IChunkEntityImpactManager chunkEntityImpactManager)
            : base(d3dEngine, sharedFrameCB.CBPerFrame)
        {
            _sharedFrameCB = sharedFrameCB;
            _cameraManager = cameraManager;
            _inputsManager = inputsManager;
            _worldParameters = worldParameters;
            _chunkEntityImpactManager = chunkEntityImpactManager;

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
            _cubeEmitter = new CubeEmitter(ClientSettings.TexturePack + @"Terran/", @"ct*.png", _worldParameters);
            AddEmitter(_cubeEmitter);

            base.Initialize();
        }

        public override void LoadContent(DeviceContext context)
        {

            base.LoadContent(context);
        }

        public override void Update(S33M3DXEngine.Main.GameTime timeSpent)
        {
            base.Update(timeSpent);
        }
        #endregion

        #region Private Methods
        private void _chunkEntityImpactManager_BlockReplaced(object sender, LandscapeBlockReplacedEventArgs e)
        {
            //Happening when a block is replaced !

            //Cube has been destroyed
            if (e.NewBlockType == WorldConfiguration.CubeId.Air)
            {
                //Emit Colored particules
                _cubeEmitter.EmitParticule(10, e.PreviousBlock, e.Position);
            }
        }
        #endregion
    }
}
