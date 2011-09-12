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
using Utopia.Editor;
using Utopia.GUI.D3D;
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
using Utopia.Network;
using Utopia.Worlds.Storage;
using Utopia.Action;
using Utopia.InputManager;
using S33M3Engines.Shared.Math;
using Utopia.Shared.Chunks.Entities.Interfaces;
using Utopia.Shared.Chunks.Entities.Concrete;
using Utopia.Shared.Chunks.Entities.Voxel;

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
        private IChunkStorageManager _chunkStorageManager;
        private ActionsManager _actionManager;
        private EntityMessageTranslator _entityMessageTranslator;
        private IDynamicEntity _player;
        private ActionsManager _actions;
        private D3DEngine _engine;
        private IEntityManager _entityManager;
        //Debug tools
        private FPS _fps; //FPS computing object

        //Game components
        //private Entities.Living.ILivingEntity _player;
       
        private GameStatesManager _gameStateManagers;

        //Debug Tool
        private DebugInfo _debugInfo;

        //TODO validate the way to access player properties from other components / eventually remove casts
        //public Player Player {get {return (Player) _player;}}

        private IKernel _iocContainer;

#if STEALTH
        const int W = 48;
        const int H = 32;
#else
        const int W = 1024;
        const int H = 600;
#endif

        public UtopiaRender(IKernel iocContainer)
        {
            _iocContainer = iocContainer;
            S33M3Engines.Threading.WorkQueue.ThreadingActif = true;    // Activate the threading Mode (Default : true, false used mainly to debug purpose)
            S33M3Engines.D3DEngine.FULLDEBUGMODE = false;
            VSync = true;                                              // Vsync ON (default)

            CubeProfile.InitCubeProfiles();                 // Init the cube profiles use by shared application (Similar than VisualCubeProfile, but without visual char.)
        }

        public override void Initialize()
        {
            //Initialize the Thread Pool manager
            S33M3Engines.Threading.WorkQueue.Initialize(ClientSettings.Current.Settings.GraphicalParameters.AllocatedThreadsModifier);

            //DebugInit(_iocContainer); //To use for testing Debug initializer
            Init(_iocContainer);

#if DEBUG
            GameConsole.Write("DX11 main engine started and initialized with feature level : " + _d3dEngine.Device.FeatureLevel);
#endif
            base.Initialize();

        }

        //Default Utopia Init method.
        private void Init(IKernel IoCContainer)
        {
            int Seed = 12695360;
            int SeaLevel = AbstractChunk.ChunkSize.Y / 2;

            //=======================================================================================
            //New Class structure Acces =============================================================
            //=======================================================================================
            //Create the world components ===========================================================

            //If Server mode set the chunk Side
            Server server = IoCContainer.Get<Server>();
            if (server.Connected)
            {
                if (AbstractChunk.ChunkSize != server.ChunkSize)
                {
                    throw new Exception("Client chunkSize is different from server !");
                }

                //Change Visible WorldSize if client parameter > Server !
                if (ClientSettings.Current.Settings.GraphicalParameters.WorldSize > server.MaxServerViewRange)
                {
                    ClientSettings.Current.Settings.GraphicalParameters.WorldSize = server.MaxServerViewRange;
                }

                Seed = server.WorldSeed;
                SeaLevel = server.SeaLevel;
            }

            //Variables initialisation ==================================================================
            Utopia.Shared.World.WorldParameters worldParam = new Shared.World.WorldParameters()
            {
                IsInfinite = true,
                Seed = Seed,
                SeaLevel = SeaLevel,
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

            _actionManager = IoCContainer.Get<ActionsManager>();
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

            _engine = IoCContainer.Get<D3DEngine>();
          
            //Storage Manager
            _chunkStorageManager = IoCContainer.Get<IChunkStorageManager>(new ConstructorArgument("forceNew", false),
                                                                          new ConstructorArgument("UserName", server.ServerConnection.Login));

            GameComponents.Add(IoCContainer.Get<InputsManager>());

            GameComponents.Add(IoCContainer.Get<ItemRenderer>());

            //Attached the Player to the camera =+> The player will be used as Camera Holder !
            //camera.CameraPlugin = _player;
            _camManager.UpdateOrder = 1;
            GameComponents.Add(_camManager); //The camera is using the _player to get it's world positions and parameters, so the _player updates must be done BEFORE the camera !

            //The Player !
            VisualPlayerCharacter Player = IoCContainer.Get<VisualPlayerCharacter>(new ConstructorArgument("voxelEntity", new PlayerCharacterBody()));
            Player.UpdateOrder = 0;
            Player.IsPlayerConstroled = true;
            camera.CameraPlugin = IoCContainer.Get<VisualPlayerCharacter>();

            _entityManager = IoCContainer.Get<IEntityManager>();
            GameComponents.Add(_entityManager);

            GameComponents.Add(Player);

            _actions = IoCContainer.Get<ActionsManager>();

            //-- Clock --
            _worldClock = IoCContainer.Get<IClock>(new ConstructorArgument("clockSpeed", 1f),
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
            _worldRenderer.UpdateOrder = 11;
            GameComponents.Add(_worldRenderer);             //Bind worldRendered to main loop.

            //TODO Incoroporate EntityImpect inside Enitty framework as a single class ==> Not static !
            EntityImpact.Init(IoCContainer.Get<SingleArrayChunkContainer>(), IoCContainer.Get<ILightingManager>(), IoCContainer.Get<IWorldChunks>(), IoCContainer.Get<IChunkStorageManager>(), server);

            //GUI components
            _fps = new FPS();
            GameComponents.Add(_fps);

         
            _debugInfo = new DebugInfo(_d3dEngine);
            _debugInfo.Activated = true;
            _debugInfo.SetComponants(_fps, IoCContainer.Get<IClock>(), IoCContainer.Get<IWorldChunks>());
            GameComponents.Add(_debugInfo);

            GameConsole.Initialize(_d3dEngine);

            //Add the server if multiplayer mode
            if (server.Connected)
            {
                server.ChunkContainer = IoCContainer.Get<SingleArrayChunkContainer>();

                //Create the EntityMessageTRanslator
                _entityMessageTranslator = IoCContainer.Get<EntityMessageTranslator>();

                GameComponents.Add(server);
            }

            GameComponents.Add(IoCContainer.Get<DebugComponent>());

            GameComponents.Add(IoCContainer.Get<Hud>());

            GameComponents.Add(IoCContainer.Get<GuiManager>());

            //this one is disabled by default, can be enabled with F12 UI 
            GameComponents.Add(IoCContainer.Get<EntityEditor>());

            //Bind Actions to inputs events
            BindActions();

            // TODO (Simon) wire all binded components in one shot with ninject : GameComponents.AddRange(IoCContainer.GetAll<IGameComponent>());
            // BUT we cant handle the add order ourselves: an updateOrder int + sorted components collection like in XNA would be good
            
            // TODO (Simon)  debug vs release is currently a mess : no debug text in debug mode, no UI in debug mode, is debug mode of any use fir us ?
            #region Debug Components
#if DEBUG
            DebugEffect.Init(_d3dEngine);             // Default Effect used by debug componant (will be shared)
#endif
            #endregion

        }

        private void BindActions()
        {
            _actionManager.AddActions(new KeyboardTriggeredAction()
            {
                Action = Actions.Move_Forward,
                TriggerType = KeyboardTriggerMode.KeyDown,
                Binding = ClientSettings.Current.Settings.KeyboardMapping.Move.Forward
            });

            _actionManager.AddActions(new MouseTriggeredAction()
            {
                Action = Actions.Move_Forward,
                TriggerType = MouseTriggerMode.ButtonDown,
                Binding = MouseButton.LeftAndRightButton
            });

            _actionManager.AddActions(new KeyboardTriggeredAction()
            {
                Action = Actions.Move_Backward,
                TriggerType = KeyboardTriggerMode.KeyDown,
                Binding = ClientSettings.Current.Settings.KeyboardMapping.Move.Backward
            });

            _actionManager.AddActions(new KeyboardTriggeredAction()
            {
                Action = Actions.Move_StrafeLeft,
                TriggerType = KeyboardTriggerMode.KeyDown,
                Binding = ClientSettings.Current.Settings.KeyboardMapping.Move.StrafeLeft
            });

            _actionManager.AddActions(new KeyboardTriggeredAction()
            {
                Action = Actions.Move_StrafeRight,
                TriggerType = KeyboardTriggerMode.KeyDown,
                Binding = ClientSettings.Current.Settings.KeyboardMapping.Move.StrafeRight
            });

            _actionManager.AddActions(new KeyboardTriggeredAction()
            {
                Action = Actions.Move_Down,
                TriggerType = KeyboardTriggerMode.KeyDown,
                Binding = ClientSettings.Current.Settings.KeyboardMapping.Move.Down
            });

            _actionManager.AddActions(new KeyboardTriggeredAction()
            {
                Action = Actions.Move_Up,
                TriggerType = KeyboardTriggerMode.KeyDown,
                Binding = ClientSettings.Current.Settings.KeyboardMapping.Move.Up
            });

            _actionManager.AddActions(new KeyboardTriggeredAction()
            {
                Action = Actions.Move_Jump,
                TriggerType = KeyboardTriggerMode.KeyDown,
                Binding = ClientSettings.Current.Settings.KeyboardMapping.Move.Jump
            });

            _actionManager.AddActions(new KeyboardTriggeredAction()
            {
                Action = Actions.Move_Mode,
                TriggerType = KeyboardTriggerMode.KeyDownUp,
                Binding = ClientSettings.Current.Settings.KeyboardMapping.Move.Mode
            });

            _actionManager.AddActions(new KeyboardTriggeredAction()
            {
                Action = Actions.Move_Run,
                TriggerType = KeyboardTriggerMode.KeyDown,
                Binding = ClientSettings.Current.Settings.KeyboardMapping.Move.Run
            });

            _actionManager.AddActions(new KeyboardTriggeredAction()
            {
                Action = Actions.Engine_FullScreen,
                TriggerType = KeyboardTriggerMode.KeyDownUp,
                Binding = ClientSettings.Current.Settings.KeyboardMapping.FullScreen
            });

            _actionManager.AddActions(new KeyboardTriggeredAction()
            {
                Action = Actions.Engine_LockMouseCursor,
                TriggerType = KeyboardTriggerMode.KeyDownUp,
                Binding = ClientSettings.Current.Settings.KeyboardMapping.LockMouseCursor
            });

            _actionManager.AddActions(new MouseTriggeredAction()
            {
                Action = Actions.Block_Add,
                TriggerType = MouseTriggerMode.ButtonUpDown,
                Binding = MouseButton.LeftButton
            });

            _actionManager.AddActions(new MouseTriggeredAction()
            {
                Action = Actions.Block_Remove,
                TriggerType = MouseTriggerMode.ButtonUpDown,
                Binding = MouseButton.RightButton
            });

            _actionManager.AddActions(new MouseTriggeredAction()
            {
                Action = Actions.Block_SelectNext,
                TriggerType = MouseTriggerMode.ScrollWheelForward,
                Binding = MouseButton.ScrollWheel
            });

            _actionManager.AddActions(new MouseTriggeredAction()
            {
                Action = Actions.Block_SelectPrevious,
                TriggerType = MouseTriggerMode.ScrollWheelBackWard,
                Binding = MouseButton.ScrollWheel
            });

            _actionManager.AddActions(new KeyboardTriggeredAction()
            {
                Action = Actions.World_FreezeTime,
                TriggerType = KeyboardTriggerMode.KeyDownUp,
                Binding = ClientSettings.Current.Settings.KeyboardMapping.FreezeTime
            });

            _actionManager.AddActions(new KeyboardTriggeredAction()
            {
                Action = Actions.Engine_VSync,
                TriggerType = KeyboardTriggerMode.KeyDownUp,
                Binding = ClientSettings.Current.Settings.KeyboardMapping.VSync
            });

            _actionManager.AddActions(new KeyboardTriggeredAction()
            {
                Action = Actions.Engine_ShowDebugUI,
                TriggerType = KeyboardTriggerMode.KeyDownUp,
                Binding = new KeyWithModifier() { MainKey = Keys.F12 }
            });

            _actionManager.AddActions(new KeyboardTriggeredAction()
            {
                Action = Actions.Engine_Exit,
                TriggerType = KeyboardTriggerMode.KeyDownUp,
                Binding = new KeyWithModifier() { MainKey = Keys.Escape }
            });

            _actionManager.AddActions(new KeyboardTriggeredAction()
            {
                Action = Actions.DebugUI_Insert,
                TriggerType = KeyboardTriggerMode.KeyDownUp,
                Binding = new KeyWithModifier() { MainKey = Keys.Insert }
            });
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
            _actionManager.FetchInputs();
            _actionManager.Update();
            base.Update(ref TimeSpend);

            //After all update are done, Check against "System" keys like Exit, ...
            InputHandling();
        }

        public override void Interpolation(ref double interpolation_hd, ref float interpolation_ld)
        {
            _actionManager.FetchInputs();
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
            if (_actions.isTriggered(Actions.Engine_Exit)) Exit();
            if (_actions.isTriggered(Actions.Engine_LockMouseCursor)) _engine.UnlockedMouse = !_engine.UnlockedMouse;
            if (_actions.isTriggered(Actions.Engine_FullScreen)) _engine.isFullScreen = !_engine.isFullScreen;
        }

        public override void Dispose()
        {
#if DEBUG
            DebugEffect.Dispose();
#endif
            GameConsole.CleanUp();
            base.Dispose();
        }
    }
}
