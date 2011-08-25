using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using S33M3Engines.D3D;
using S33M3Engines.D3D.DebugTools;
using S33M3Engines.InputHandler;
using S33M3Engines.InputHandler.KeyboardHelper;
using System.Windows.Forms;
using SharpDX.Direct3D11;
using S33M3Engines.Cameras;
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
using Utopia.Entities.Living;
using S33M3Engines;
using Ninject;
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
using Ninject.Parameters;
using S33M3Engines.Threading;
using Size = System.Drawing.Size;
using Utopia.Shared.World;
using Utopia.Shared.Interfaces;
using Utopia.Worlds.Chunks.ChunkLighting;

namespace Utopia
{

    public partial class UtopiaRender : Game
    {
        private WorldRenderer _worldRenderer;
        private IWorld _currentWorld;
        private IClock _worldClock;
        private ISkyDome _skyDome;
        private IWorldChunks _chunks;
        private IWeather _weather;
        private CameraManager _camManager;
        private WorldFocusManager _worldFocusManager;
        private EntityRenderer _entityRender;

        //Debug tools
        private FPS _fps; //FPS computing object

        //Game componants
        private Entities.Living.ILivingEntity _player;
        private GUI.D3D.GUI _gui;

        private GameStatesManager _gameStateManagers;

        //Debug Tool
        private DebugInfo _debugInfo;

        //TODO validate the way to access player properties from other components / eventually remove casts
        public Player Player {get {return (Player) _player;}}

        private IKernel _iocContainer;

#if STEALTH
        const int W = 48;
        const int H = 32;
#else
        const int W = 1024;
        const int H = 600;
#endif

#if DEBUG
        private Axis _axis;
#endif

        public UtopiaRender()
        {
             S33M3Engines.Threading.WorkQueue.ThreadingActif = true;    // Activate the threading Mode (Default : true, false used mainly to debug purpose)
             S33M3Engines.D3DEngine.FULLDEBUGMODE = true;
             VSync = true;                                              // Vsync ON (default)

            CubeProfile.InitCubeProfiles();                 // Init the cube profiles use by shared application (Similar than VisualCubeProfile, but without visual char.)
        }

        public override void Initialize()
        {
            //Initialize the Thread Pool manager
            S33M3Engines.Threading.WorkQueue.Initialize(ClientSettings.Current.Settings.GraphicalParameters.AllocatedThreadsModifier);

            _iocContainer = new StandardKernel(new NinjectSettings { UseReflectionBasedInjection = true });

            // DebugInit(_iocContainer); //To use for testing Debug initializer
            Init(_iocContainer);

#if DEBUG
            GameConsole.Write("DX11 main engine started and initialized with feature level : " + _d3dEngine.Device.FeatureLevel);
#endif
            base.Initialize();

        }

        //Default Utopia Init method.
        private void Init(IKernel IoCContainer)
        {
            //=======================================================================================
            //New Class structure Acces =============================================================
            //=======================================================================================
            //Create the world components ===========================================================

            //Variables initialisation ==================================================================
            Utopia.Shared.World.WorldParameters worldParam = new Shared.World.WorldParameters()
            {
                IsInfinite = true,
                Seed = 0,
                SeaLevel = AbstractChunk.ChunkSize.Y / 2,
                WorldChunkSize = new Location2<int>(ClientSettings.Current.Settings.GraphicalParameters.WorldSize,
                                                ClientSettings.Current.Settings.GraphicalParameters.WorldSize)
            };
            Location2<int> worldStartUp = new Location2<int>(0 * AbstractChunk.ChunkSize.X, 0 * AbstractChunk.ChunkSize.Z);

            //===========================================================================================

            //Creating the IoC Bindings
            ContainersBindings(IoCContainer, worldParam);

            //Init Block Profiles
            VisualCubeProfile.InitCubeProfiles(IoCContainer.Get<ICubeMeshFactory>("SolidCubeMeshFactory"),
                                               IoCContainer.Get<ICubeMeshFactory>("LiquidCubeMeshFactory"));

            //-- Get the Main D3dEngine --
            _d3dEngine = IoCContainer.Get<D3DEngine>(new ConstructorArgument("startingSize", new Size(W, H)),
                                                     new ConstructorArgument("windowCaption", "Powered By S33m3 Engine ! Rulezzz"),
                                                     new ConstructorArgument("MaxNbrThreads", WorkQueue.ThreadPool.Concurrency));

            _d3dEngine.Initialize(); //Init the 3d Engine
            _d3dEngine.GameWindow.Closed += (o, args) => { _isFormClosed = true; }; //Subscribe to Close event
            if (!_d3dEngine.UnlockedMouse) System.Windows.Forms.Cursor.Hide();      //Hide the mouse by default !

            _inputHandler = IoCContainer.Get<InputHandlerManager>();
            DXStates.CreateStates(_d3dEngine);  //Create all States that could by used by the game.

            //-- Get Camera --
            ICamera camera = IoCContainer.Get<ICamera>(); // Create a firstPersonCamera viewer

            //-- Get World focus --
            _worldFocusManager = IoCContainer.Get<WorldFocusManager>();
            _worldFocusManager.WorldFocus = (IWorldFocus)camera; // Use the camera as a the world focus

            //-- Get StateManager --
            _gameStateManagers = IoCContainer.Get<GameStatesManager>();
            _gameStateManagers.DebugActif = false;
            _gameStateManagers.DebugDisplay = 0;

            //-- Get Camera manager --
            _camManager = IoCContainer.Get<CameraManager>();

            //-- Create Entity Player --
            //TODO : Create an entity manager that will be responsible to render the various entities instead of leaving each entity to render itself.
            _player = IoCContainer.Get<ILivingEntity>(new ConstructorArgument("Name", ClientSettings.Current.Settings.GameParameters.NickName),
                                                      new ConstructorArgument("startUpWorldPosition", new DVector3((worldParam.WorldChunkSize.X * AbstractChunk.ChunkSize.X / 2.0) + worldStartUp.X, 90, (worldParam.WorldChunkSize.Z * AbstractChunk.ChunkSize.Z / 2.0f) + worldStartUp.Z)),
                                                      new ConstructorArgument("size", new Vector3(0.5f, 1.9f, 0.5f)),
                                                      new ConstructorArgument("walkingSpeed", 5f),
                                                      new ConstructorArgument("flyingSpeed", 30f),
                                                      new ConstructorArgument("headRotationSpeed", 10f));
            ((Player)_player).Mode = LivingEntityMode.FreeFirstPerson;

            //Create the Entity Renderer
            //A simple object wrapping a collectin of Entities, and wiring them for update/draw/...
            _entityRender = IoCContainer.Get<EntityRenderer>();
            _entityRender.Entities.Add(_player); //Add the main player to Entities
            GameComponents.Add(_entityRender);


            //Attached the Player to the camera =+> The player will be used as Camera Holder !
            camera.CameraPlugin = _player;
            GameComponents.Add(_camManager); //The camera is using the _player to get it's world positions and parameters, so the _player updates must be done BEFORE the camera !


            //-- Clock --
            _worldClock = IoCContainer.Get<IClock>(new ConstructorArgument("input", _inputHandler),
                                                   new ConstructorArgument("clockSpeed", 1f),
                                                   new ConstructorArgument("startTime", (float)Math.PI * 1f));
            //-- Weather --
            _weather = IoCContainer.Get<IWeather>();
            //-- SkyDome --
            _skyDome = IoCContainer.Get<ISkyDome>();
            //-- Chunks -- Get chunks manager.

            //Get Processor Config by giving world specification
            _chunks = IoCContainer.Get<IWorldChunks>(new ConstructorArgument("worldStartUpPosition", worldStartUp));

            //Attach a "Flat world generator"
            _chunks.LandscapeManager.WorldGenerator = new WorldGenerator(IoCContainer.Get<WorldParameters>(), IoCContainer.Get<IWorldProcessorConfig>("s33m3World"));

            //Create the World Components wrapper -----------------------
            _currentWorld = IoCContainer.Get<IWorld>();

            //Send the world to render
            _worldRenderer = IoCContainer.Get<WorldRenderer>();
            GameComponents.Add(_worldRenderer);             //Bind worldRendered to main loop.

            //TODO Incoroporate EntityImpect inside Enitty framework as a single class ==> Not static !
            EntityImpact.Init(IoCContainer.Get<SingleArrayChunkContainer>(), IoCContainer.Get<ILightingManager>(), IoCContainer.Get<IWorldChunks>());

            //GUI components
            _fps = new FPS();
            GameComponents.Add(_fps);

            _gui = new GUI.D3D.GUI(GameComponents,_d3dEngine /*,((Player)_player).Inventory*/);
            GameComponents.Add(_gui);

            _debugInfo = new DebugInfo(_d3dEngine);
            _debugInfo.Activated = true;
            _debugInfo.SetComponants(_fps, IoCContainer.Get<IClock>(), _player);
            GameComponents.Add(_debugInfo);

            GameConsole.Initialize(_d3dEngine);

        }

        public override void LoadContent()
        {

            base.LoadContent();
        }

        public override void UnloadContent()
        {
            base.UnloadContent();

            DXStates.Dispose();
            //UtopiaSaveManager.StopUSM();
        }

        public override void Update(ref GameTime TimeSpend)
        {
            KeyboardStateHandling();
            //Update Internal Components
            base.Update(ref TimeSpend);
        }

        public override void Interpolation(ref double interpolation_hd, ref float interpolation_ld)
        {
            KeyboardStateHandling();
            base.Interpolation(ref interpolation_hd, ref interpolation_ld);
        }

        private void KeyboardStateHandling()
        {

            if (_inputHandler.PrevKeyboardState.IsKeyDown(ClientSettings.Current.Settings.KeyboardMapping.DebugMode) && _inputHandler.PrevKeyboardState.IsKeyDown(Keys.LControlKey) && _inputHandler.CurKeyboardState.IsKeyUp(ClientSettings.Current.Settings.KeyboardMapping.DebugMode) && _inputHandler.CurKeyboardState.IsKeyDown(Keys.LControlKey))
            {
                FixedTimeSteps = !FixedTimeSteps;
                GameConsole.Write("FixeTimeStep Mode : " + FixedTimeSteps.ToString());
            }

            if (_inputHandler.IsKeyPressed(ClientSettings.Current.Settings.KeyboardMapping.FullScreen)) _d3dEngine.isFullScreen = !_d3dEngine.isFullScreen; //Go full screen !

            if (_inputHandler.PrevKeyboardState.IsKeyDown(ClientSettings.Current.Settings.KeyboardMapping.DebugMode) && !_inputHandler.PrevKeyboardState.IsKeyDown(Keys.LControlKey) && _inputHandler.CurKeyboardState.IsKeyUp(ClientSettings.Current.Settings.KeyboardMapping.DebugMode) && !_inputHandler.CurKeyboardState.IsKeyDown(Keys.LControlKey))
            {
                _gameStateManagers.DebugActif = !_gameStateManagers.DebugActif;
                if (!DebugActif)
                {
                    _gameStateManagers.DebugDisplay = 0;
                }
            }
            if (_inputHandler.IsKeyPressed(Keys.Up))
            {
                if (!_gameStateManagers.DebugActif) return;
                _gameStateManagers.DebugDisplay++;
                if (_gameStateManagers.DebugDisplay > 2) _gameStateManagers.DebugDisplay = 2;
            }
            if (_inputHandler.IsKeyPressed(Keys.Down))
            {
                if (!_gameStateManagers.DebugActif) return;
                _gameStateManagers.DebugDisplay--;
                if (_gameStateManagers.DebugDisplay < 0) _gameStateManagers.DebugDisplay = 0;
            }

            if (_inputHandler.IsKeyPressed(ClientSettings.Current.Settings.KeyboardMapping.LockMouseCursor))
            {
                _d3dEngine.UnlockedMouse = !_d3dEngine.UnlockedMouse;
            }

            if (_inputHandler.IsKeyPressed(ClientSettings.Current.Settings.KeyboardMapping.VSync))
            {
                VSync = !VSync;
            }

            if (_inputHandler.IsKeyPressed(ClientSettings.Current.Settings.KeyboardMapping.DebugInfo))
            {
                _debugInfo.Activated = !_debugInfo.Activated;
            }

            if (_inputHandler.IsKeyPressed(ClientSettings.Current.Settings.KeyboardMapping.Console)) GameConsole.Show = !GameConsole.Show;

            //Exit application
            if (_inputHandler.IsKeyPressed(Keys.Escape)) Exit();
        }

        public override void Draw()
        {
            _d3dEngine.Context.ClearRenderTargetView(_d3dEngine.RenderTarget, BackBufferColor);
            _d3dEngine.Context.ClearDepthStencilView(_d3dEngine.DepthStencilTarget, DepthStencilClearFlags.Depth, 1.0f, 0);

            base.Draw();
            base.DrawInterfaces();

            base.Present();
        }

        public override void Dispose()
        {
            _iocContainer.Dispose(); // Will also disposed all singleton objects that have been registered !
#if DEBUG
            DebugEffect.Dispose();
#endif
            GameConsole.CleanUp();
            base.Dispose();
        }
    }
}
