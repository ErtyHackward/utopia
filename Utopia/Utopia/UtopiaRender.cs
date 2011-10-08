using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using S33M3Engines.D3D;
using S33M3Engines.D3D.DebugTools;
using S33M3Engines.InputHandler;
using S33M3Engines.InputHandler.KeyboardHelper;
using SharpDX.Direct3D11;
using S33M3Engines.Cameras;
using Utopia.Editor;
using Utopia.GUI;
using Utopia.GUI.D3D;
using Utopia.GUI.D3D.Map;
using Utopia.Shared.Net.Connections;
using UtopiaContent.ModelComp;
using SharpDX;
using S33M3Engines.Struct;
using S33M3Engines.Maths;
using S33M3Engines.D3D.Effects.Basics;
using Utopia.GameDXStates;
using Utopia.Shared.Structs;
using Utopia.Shared.Structs.Landscape;
using Utopia.Shared;
using Utopia.Settings;
using Utopia.Shared.Config;
using S33M3Engines;
using S33M3Engines.GameStates;
using S33M3Engines.WorldFocus;
using Utopia.Worlds.Cubes;
using Utopia.Worlds.Weather;
using Utopia.Worlds.Chunks;
using Utopia.Worlds.SkyDomes;
using Utopia.Worlds.GameClocks;
using Utopia.Worlds;
using Utopia.Entities;
using Utopia.Shared.Chunks;
using S33M3Engines.Threading;
using Size = System.Drawing.Size;
using Utopia.Shared.World;
using Utopia.Shared.Interfaces;
using Utopia.Worlds.Chunks.ChunkLighting;
using Utopia.Network;
using Utopia.Worlds.Storage;
using Utopia.Action;
using Utopia.InputManager;
using S33M3Engines.Shared.Math;
using Utopia.Shared.Chunks.Entities.Interfaces;
using Utopia.Shared.Chunks.Entities.Concrete;
using Utopia.Entities.Managers;
using Utopia.Entities.Voxel;
using Utopia.Shared.Chunks.Entities;
using Utopia.Worlds.Chunks.ChunkEntityImpacts;
using Utopia.Entities.Renderer;
using Utopia.Entities.Managers.Interfaces;
using S33M3Engines.Timers;
using Utopia.Entities.Renderer.Interfaces;
using Utopia.Worlds.Chunks.ChunkLandscape;
using Utopia.Worlds.Chunks.ChunkMesh;
using Utopia.Worlds.Chunks.ChunkWrapper;
using Nuclex.UserInterface;

namespace Utopia
{
    public partial class UtopiaRender : Game
    {
        #region Private Variables
        private D3DEngine _engine;
        private Server _server;
        private WorldFocusManager _worldFocusManager;
        private WorldParameters _worldParameters;
        private VisualWorldParameters _visualWorldParameters;
        private GameStatesManager _gameStatesManager;
        private ICamera _firstPersonCamera;
        private CameraManager _cameraManager;
        private TimerManager _timerManager;
        private EntityMessageTranslator _entityMessageTranslator;
        private ItemMessageTranslator _itemMessageTranslator;
        private InputsManager _inputsManager;
        private ActionsManager _actionsManager;
        private GuiManager _guiManager;
        private Screen _screen;
        private IconFactory _iconFactory;
        private DebugComponent _debugComponent;
        private FPS _fps;
        private IClock _gameClock;
        private ChatComponent _chatComponent;
        private MapComponent _mapComponent;
        private Hud _hud;
        private EntityEditor _entityEditor;
        private IDrawableComponent _stars;
        private ISkyDome _skydome;
        private IWeather _weather;
        private IDrawableComponent _clouds;
        private IChunkStorageManager _chunkStorageManager;
        private ICubeMeshFactory _solidCubeMeshFactory;
        private ICubeMeshFactory _liquidCubeMeshFactory;
        private SingleArrayChunkContainer _singleArrayChunkContainer;
        private ILandscapeManager _landscapeManager;
        private ILightingManager _lightingManager;
        private IChunkMeshManager _chunkMeshManager;
        private IWorldChunks _worldChunks;
        private IChunksWrapper _chunksWrapper;
        private WorldGenerator _worldGenerator;
        private IWorldProcessorConfig _worldProcessorConfig;
        private IPickingRenderer _pickingRenderer;
        private IChunkEntityImpactManager _chunkEntityImpactManager;
        private IEntityPickingManager _entityPickingManager;
        private IDynamicEntityManager _dynamicEntityManager;
        private PlayerEntityManager _playerEntityManager;
        private PlayerCharacter _playerCharacter;
        private IEntitiesRenderer _playerEntityRenderer;
        private IEntitiesRenderer _defaultEntityRenderer;
        private VoxelMeshFactory _voxelMeshFactory;

        #endregion

        public UtopiaRender(
                D3DEngine engine,
                Server server,
                WorldFocusManager worldFocusManager,
                WorldParameters worldParameters,
                VisualWorldParameters visualWorldParameters,
                GameStatesManager gameStatesManager,
                ICamera firstPersonCamera,
                CameraManager cameraManager,
                TimerManager timerManager,
                EntityMessageTranslator entityMessageTranslator,
                ItemMessageTranslator itemMessageTranslator,
                InputsManager inputsManager,
                ActionsManager actionsManager,
                GuiManager guiManager,
                Screen screen,
                IconFactory iconFactory,
                FPS fps,
                IClock gameClock,
                ChatComponent chatComponent,
                MapComponent mapComponent,
                Hud hud,
                EntityEditor entityEditor,
                IDrawableComponent stars,
                ISkyDome skydome,
                IWeather weather,
                IDrawableComponent clouds,
                IChunkStorageManager chunkStorageManager,
                ICubeMeshFactory solidCubeMeshFactory,
                ICubeMeshFactory liquidCubeMeshFactory,
                SingleArrayChunkContainer singleArrayChunkContainer,
                ILandscapeManager landscapeManager,
                ILightingManager lightingManager,
                IChunkMeshManager chunkMeshManager,
                IWorldChunks worldChunks,
                IChunksWrapper chunksWrapper,
                WorldGenerator worldGenerator,
                IWorldProcessorConfig worldProcessorConfig,
                IPickingRenderer pickingRenderer,
                IChunkEntityImpactManager chunkEntityImpactManager,
                IEntityPickingManager entityPickingManager,
                IDynamicEntityManager dynamicEntityManager,
                PlayerEntityManager playerEntityManager,
                PlayerCharacter playerCharacter,
                IEntitiesRenderer playerEntityRenderer,
                IEntitiesRenderer defaultEntityRenderer,
                VoxelMeshFactory voxelMeshFactory
            )
        {
            _engine = engine;
            _server = server;
            _worldFocusManager = worldFocusManager;
            _worldParameters = worldParameters;
            _visualWorldParameters = visualWorldParameters;
            _gameStatesManager = gameStatesManager;
            _firstPersonCamera = firstPersonCamera;
            _cameraManager = cameraManager;
            _timerManager = timerManager;
            _entityMessageTranslator = entityMessageTranslator;
            _itemMessageTranslator = itemMessageTranslator;
            _inputsManager = inputsManager;
            _actionsManager = actionsManager;
            _guiManager = guiManager;
            _screen = screen;
            _iconFactory = iconFactory;
            _fps = fps;
            _gameClock = gameClock;
            _chatComponent = chatComponent;
            _mapComponent = mapComponent;
            _hud = hud;
            _entityEditor = entityEditor;
            _stars = stars;
            _skydome = skydome;
            _weather = weather;
            _clouds = clouds;
            _chunkStorageManager = chunkStorageManager;
            _solidCubeMeshFactory = solidCubeMeshFactory;
            _liquidCubeMeshFactory = liquidCubeMeshFactory;
            _singleArrayChunkContainer = singleArrayChunkContainer;
            _landscapeManager = landscapeManager;
            _lightingManager = lightingManager;
            _chunkMeshManager = chunkMeshManager;
            _worldChunks = worldChunks;
            _chunksWrapper = chunksWrapper;
            _worldGenerator = worldGenerator;
            _worldProcessorConfig = worldProcessorConfig;
            _pickingRenderer = pickingRenderer;
            _chunkEntityImpactManager = chunkEntityImpactManager;
            _entityPickingManager = entityPickingManager;
            _dynamicEntityManager = dynamicEntityManager;
            _playerEntityManager = playerEntityManager;
            _playerCharacter = playerCharacter;
            _playerEntityRenderer = playerEntityRenderer;
            _defaultEntityRenderer = defaultEntityRenderer;
            _voxelMeshFactory = voxelMeshFactory;

            S33M3Engines.Threading.WorkQueue.ThreadingActif = true;    // Activate the threading Mode (Default : true, false used mainly to debug purpose)
            S33M3Engines.D3DEngine.FULLDEBUGMODE = false;
            VSync = true;                                              // Vsync ON (default)
        }

        public override void Initialize()
        {
            //Initialize the Thread Pool manager
            S33M3Engines.Threading.WorkQueue.Initialize(ClientSettings.Current.Settings.GraphicalParameters.AllocatedThreadsModifier);

            Init();

            base.Initialize();
        }

        //Default Utopia Init method.
        private void Init()
        {
            _server.ServerConnection.ConnectionStatusChanged += ServerConnection_ConnectionStatusChanged;
            
            if (AbstractChunk.ChunkSize != _server.ChunkSize)
            {
                throw new Exception("Client chunkSize is different from server !");
            }
            //Change Visible WorldSize if client parameter > Server !
            if (ClientSettings.Current.Settings.GraphicalParameters.WorldSize > _server.MaxServerViewRange)
            {
                ClientSettings.Current.Settings.GraphicalParameters.WorldSize = _server.MaxServerViewRange;
            }

            _d3dEngine = _engine;
            _d3dEngine.GameWindow.Closed += GameWindow_Closed;
            _d3dEngine.HideMouseCursor();   //Hide the mouse by default !
            DXStates.CreateStates(_d3dEngine);  //Create all States that could by used by the game.

            //-- Get Camera --
            _firstPersonCamera.CameraPlugin = _playerEntityManager;

            //-- Get World focus --
            _worldFocusManager.WorldFocus = (IWorldFocus)_firstPersonCamera; // Use the camera as a the world focus

            //Add Components to the main game Loop !
            GameComponents.Add(_server);
            GameComponents.Add(_inputsManager);
            GameComponents.Add(_iconFactory);
            GameComponents.Add(_timerManager);
            GameComponents.Add(_playerEntityManager);
            GameComponents.Add(_dynamicEntityManager);
            GameComponents.Add(_cameraManager);
            GameComponents.Add(_hud);
            GameComponents.Add(_guiManager);
            GameComponents.Add(_pickingRenderer);
            GameComponents.Add(_chatComponent);
            GameComponents.Add(_mapComponent);
            GameComponents.Add(new DebugComponent(this, _d3dEngine, _screen, _gameStatesManager, _actionsManager,_playerEntityManager));
            GameComponents.Add(_fps);
            GameComponents.Add(_entityEditor);
            GameComponents.Add(_skydome);
            GameComponents.Add(_gameClock);
            GameComponents.Add(_weather);
            GameComponents.Add(_worldChunks);

            #region Debug Components
#if DEBUG
            DebugEffect.Init(_d3dEngine);             // Default Effect used by debug componant (will be shared)
#endif
            #endregion

        }

        //Windows state management

        void GameWindow_Closed(object sender, EventArgs e)
        {
            _isFormClosed = true; //Subscribe to Close event
        }

        //State management

        /// <summary>
        /// Check server connection change state !!
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void ServerConnection_ConnectionStatusChanged(object sender, ConnectionStatusEventArgs e)
        {
            if (e.Status == ConnectionStatus.Disconnected && e.Exception != null)
            {
                GameExitReasonMessage msg = new GameExitReasonMessage()
                {
                    GameExitReason = ExitReason.Error,
                    MainMessage = "Server connection lost",
                    DetailedMessage = "Reason : " + e.Reason.ToString() + Environment.NewLine + "Detail : " + e.Exception.Message
                };
                Exit(msg);
            }
        }

        

        public override void LoadContent()
        {
            base.LoadContent();
        }

        public override void UnloadContent()
        {
            base.UnloadContent();
            DXStates.Dispose();
        }

        public override void Update(ref GameTime TimeSpend)
        {
            _actionsManager.FetchInputs();
            _actionsManager.Update();
            base.Update(ref TimeSpend);

            //After all update are done, Check against "System" keys like Exit, ...
            InputHandling();
        }

        public override void Interpolation(ref double interpolation_hd, ref float interpolation_ld)
        {
            _actionsManager.FetchInputs();
            base.Interpolation(ref interpolation_hd, ref interpolation_ld);
        }

        public override void Draw()
        {
            _d3dEngine.Context.ClearRenderTargetView(_d3dEngine.RenderTarget, BackBufferColor);
            _d3dEngine.Context.ClearDepthStencilView(_d3dEngine.DepthStencilTarget, DepthStencilClearFlags.Depth, 1.0f, 0);
            base.Draw();
            base.Present();
        }


        private void InputHandling()
        {
            //Exit application
            if (_actionsManager.isTriggered(Actions.Engine_Exit))
            {
                GameExitReasonMessage msg = new GameExitReasonMessage()
                {
                    GameExitReason = ExitReason.UserRequest,
                    MainMessage = "User Requested exit"
                };
            
                Exit(msg);
            }
            if (_actionsManager.isTriggered(Actions.Engine_LockMouseCursor)) _engine.UnlockedMouse = !_engine.UnlockedMouse;
            if (_actionsManager.isTriggered(Actions.Engine_FullScreen)) _engine.isFullScreen = !_engine.isFullScreen;
        }

        public override void Dispose()
        {
#if DEBUG
            DebugEffect.Dispose();
#endif
            _d3dEngine.GameWindow.Closed -= GameWindow_Closed; //Subscribe to Close event

            _server.ServerConnection.ConnectionStatusChanged -= ServerConnection_ConnectionStatusChanged;
            VisualCubeProfile.CleanUp();
            base.Dispose();
        }
    }
}
