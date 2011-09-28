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
using Utopia.GUI;
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
using Utopia.Entities.Managers;
using Utopia.Entities.Voxel;
using Utopia.Shared.Chunks.Entities;
using Utopia.Net.Connections;
using Utopia.Worlds.Chunks.ChunkEntityImpacts;
using Utopia.Entities.Renderer;
using Utopia.Entities.Managers.Interfaces;
using S33M3Engines.Timers;

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
        private Server _server;
        private ActionsManager _actions;
        private D3DEngine _engine;
        private IDynamicEntityManager _dynamicEntityManager;
        private IStaticEntityManager _staticEntityManager;
        private IChunkEntityImpactManager _chunkEntityImpactManager;
        private IPickingRenderer _pickingRenderer;
        //Debug tools
        private FPS _fps; //FPS computing object

        //Game components
        //private Entities.Living.ILivingEntity _player;
       
        private GameStatesManager _gameStateManagers;

        //Debug Tool
        private DebugInfo _debugInfo;

        private IKernel _iocContainer;

        private TimerManager _timerManager;

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
            _server = IoCContainer.Get<Server>();

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

            Seed = _server.WorldSeed;
            SeaLevel = _server.SeaLevel;

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
                                                     new ConstructorArgument("windowCaption", "Utopia"),
                                                     new ConstructorArgument("MaxNbrThreads", WorkQueue.ThreadPool.Concurrency));

            _d3dEngine.Initialize(); //Init the 3d Engine
            _d3dEngine.GameWindow.Closed += GameWindow_Closed;
            _d3dEngine.HideMouseCursor();   //Hide the mouse by default !

            _actionManager = IoCContainer.Get<ActionsManager>();
            DXStates.CreateStates(_d3dEngine);  //Create all States that could by used by the game.

            _timerManager = IoCContainer.Get<TimerManager>();
            GameComponents.Add(_timerManager);

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
                                                                          new ConstructorArgument("UserName", _server.ServerConnection.Login));

            GameComponents.Add(IoCContainer.Get<InputsManager>());

            //Attached the Player to the camera =+> The player will be used as Camera Holder !
            //camera.CameraPlugin = _player;
            _camManager.UpdateOrder = 1;
            GameComponents.Add(_camManager); //The camera is using the _player to get it's world positions and parameters, so the _player updates must be done BEFORE the camera !

            _pickingRenderer = IoCContainer.Get<IPickingRenderer>();
            GameComponents.Add(_pickingRenderer);

            //Create the Player manager
            PlayerEntityManager Player = IoCContainer.Get<PlayerEntityManager>(new ConstructorArgument("visualEntity", new VisualEntity(IoCContainer.Get<VoxelMeshFactory>(), IoCContainer.Get<PlayerCharacter>())));
            Player.UpdateOrder = 0;
            camera.CameraPlugin = Player;
            GameComponents.Add(Player);

            _dynamicEntityManager = IoCContainer.Get<IDynamicEntityManager>();
            GameComponents.Add(_dynamicEntityManager);
            _staticEntityManager = IoCContainer.Get<IStaticEntityManager>();
            GameComponents.Add(_staticEntityManager);

            _actions = IoCContainer.Get<ActionsManager>();

            //-- Clock --
            _worldClock = IoCContainer.Get<IClock>();
            //-- Weather --
            _weather = IoCContainer.Get<IWeather>();

            //-- SkyDome --
            _skyDome = IoCContainer.Get<ISkyDome>();
            //-- Chunks -- Get chunks manager.

            //Get Processor Config by giving world specification
            _chunks = IoCContainer.Get<IWorldChunks>(new ConstructorArgument("worldStartUpPosition", worldStartUp));

            //Attach a "Flat world generator"
            _chunks.LandscapeManager.WorldGenerator = new WorldGenerator(IoCContainer.Get<WorldParameters>(), IoCContainer.Get<IWorldProcessorConfig>("s33m3World"));

            //Get the chunkEntityImpact
            _chunkEntityImpactManager = IoCContainer.Get<IChunkEntityImpactManager>();

            //Create the World Components wrapper -----------------------
            _currentWorld = IoCContainer.Get<IWorld>();

            //Send the world to render
            _worldRenderer = IoCContainer.Get<WorldRenderer>();
            _worldRenderer.UpdateOrder = 11;
            GameComponents.Add(_worldRenderer);             //Bind worldRendered to main loop.

            //GUI components
            _fps = new FPS();
            GameComponents.Add(_fps);

            // chat
            GameComponents.Add(IoCContainer.Get<ChatComponent>());

            GameConsole.Initialize(_d3dEngine);

            //Create the EntityMessageTRanslator
            _entityMessageTranslator = IoCContainer.Get<EntityMessageTranslator>();

            GameComponents.Add(_server);

            GameComponents.Add(IoCContainer.Get<DebugComponent>());

            GameComponents.Add(IoCContainer.Get<Hud>());

            GameComponents.Add(IoCContainer.Get<GuiManager>());

            //this one is disabled by default, can be enabled with F12 UI 
            GameComponents.Add(IoCContainer.Get<EntityEditor>());


            _debugInfo = new DebugInfo(_d3dEngine);
            _debugInfo.Activated = true;
            _debugInfo.SetComponants(_fps, IoCContainer.Get<IClock>(), IoCContainer.Get<IWorldChunks>(), IoCContainer.Get<PlayerEntityManager>(), _server);
            GameComponents.Add(_debugInfo);

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
        void ServerConnection_ConnectionStatusChanged(object sender, Net.Connections.ConnectionStatusEventArgs e)
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
                Action = Actions.Use_Left,
                TriggerType = MouseTriggerMode.ButtonUpDown,
                Binding = MouseButton.LeftButton
            });

            _actionManager.AddActions(new MouseTriggeredAction()
            {
                Action = Actions.Use_Right,
                TriggerType = MouseTriggerMode.ButtonUpDown,
                Binding = MouseButton.RightButton
            });

            _actionManager.AddActions(new MouseTriggeredAction()
            {
                Action = Actions.Use_LeftWhileCursorLocked,
                TriggerType = MouseTriggerMode.ButtonUpDown,
                Binding = MouseButton.LeftButton,
                WithCursorLocked = true
            });

            _actionManager.AddActions(new MouseTriggeredAction()
            {
                Action = Actions.Use_RightWhileCursorLocked,
                TriggerType = MouseTriggerMode.ButtonUpDown,
                Binding = MouseButton.RightButton,
                WithCursorLocked = true
            });

            _actionManager.AddActions(new MouseTriggeredAction()
            {
                Action = Actions.Use_LeftWhileCursorNotLocked,
                TriggerType = MouseTriggerMode.ButtonUpDown,
                Binding = MouseButton.LeftButton,
                WithCursorLocked = false
            });

            _actionManager.AddActions(new MouseTriggeredAction()
            {
                Action = Actions.Use_RightWhileCursorNotLocked,
                TriggerType = MouseTriggerMode.ButtonUpDown,
                Binding = MouseButton.RightButton,
                WithCursorLocked = false
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

            _actionManager.AddActions(new KeyboardTriggeredAction
            {
                Action = Actions.Toggle_Chat,
                TriggerType = KeyboardTriggerMode.KeyDownUp,
                Binding = ClientSettings.Current.Settings.KeyboardMapping.Chat
            });

            _actionManager.AddActions(new KeyboardTriggeredAction
            {
                Action = Actions.EntityUse,
                TriggerType = KeyboardTriggerMode.KeyDownUp,
                Binding = ClientSettings.Current.Settings.KeyboardMapping.Use
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
            if (_actions.isTriggered(Actions.Engine_Exit))
            {
                GameExitReasonMessage msg = new GameExitReasonMessage()
                {
                    GameExitReason = ExitReason.UserRequest,
                    MainMessage = "User Requested exit"
                };
            
                Exit(msg);
            }
            if (_actions.isTriggered(Actions.Engine_LockMouseCursor)) _engine.UnlockedMouse = !_engine.UnlockedMouse;
            if (_actions.isTriggered(Actions.Engine_FullScreen)) _engine.isFullScreen = !_engine.isFullScreen;
        }

        public override void Dispose()
        {
#if DEBUG
            DebugEffect.Dispose();
#endif
            _d3dEngine.GameWindow.Closed -= GameWindow_Closed; //Subscribe to Close event

            _server.ServerConnection.ConnectionStatusChanged -= ServerConnection_ConnectionStatusChanged;
            VisualCubeProfile.CleanUp();
            GameConsole.CleanUp();
            base.Dispose();
        }
    }
}
