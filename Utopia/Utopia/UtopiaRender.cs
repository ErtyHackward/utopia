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
using Utopia.Planets.Terran;
using S33M3Engines.Struct;
using Utopia.Planets.Terran.World;
using Utopia.Planets.Terran.Cube;
using S33M3Engines.Maths;
using S33M3Engines.D3D.Effects.Basics;
using Utopia.PlugIn;
using Utopia.GameDXStates;
using Utopia.USM;
using Utopia.Shared.Structs;
using Utopia.Shared.Structs.Landscape;
using Utopia.Shared.Landscaping;
using Utopia.Shared;
using Utopia.Settings;
using Utopia.Shared.Config;
using Utopia.Entities.Living;
using S33M3Engines;
using Ninject;
using S33M3Engines.GameStates;
using S33M3Engines.WorldFocus;

namespace Utopia
{

    public partial class UtopiaRender : Game
    {
        //Debug tools
        private FPS _fps; //FPS computing object

        //Manage pluggins
        private Plugins _plugins;
        private bool _loadPluggin = false;
        private bool _newClientStructure = false;

        //Game componants
        private GameClock.Clock _clock;
        private Entities.Living.ILivingEntity _player;
        private GUI.D3D.GUI _gui;
        private Univers.Universe _universe;

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

        public UtopiaRender(bool newClientStructure = false)
        {
             _newClientStructure = newClientStructure;
             S33M3Engines.Threading.WorkQueue.ThreadingActif = true;    // Activate the threading Mode (Default : true, false used mainly to debug purpose)
             S33M3Engines.D3DEngine.FULLDEBUGMODE = true;
             VSync = true;                                              // Vsync ON (default)

            //Load the pluggin ! ===================
             _plugins = new Plugins();
             if (_loadPluggin && !newClientStructure)
             {
                 _plugins.LoadPlugins();
             }
             else
             {
                 _plugins.WorldPlugins = new IUniversePlugin[0];
             }

             WorldPlugins.Plugins = _plugins;
            //// =====================================

            //Load the config
            //Config_old.LoadConfig();

            LandscapeBuilder = new LandscapeBuilder();
             //LandscapeBuilder = new FlatLandscape();

            LandscapeBuilder.Initialize(ClientSettings.Current.Settings.GraphicalParameters.WorldSize);
            RenderCubeProfile.InitCubeProfiles();           // Init the render cube profiles
            CubeProfile.InitCubeProfiles();                 // Init the cube profiles
        }

        public override void Initialize()
        {
            //Initialize the Thread Pool manager
            S33M3Engines.Threading.WorkQueue.Initialize();

            _iocContainer = new StandardKernel(new NinjectSettings { UseReflectionBasedInjection = true });

            if (_newClientStructure) DebugInit(_iocContainer);
            else Init();

            //Display the pluggins that have been loaded ! =========================
            for (int i = 0; i < WorldPlugins.Plugins.WorldPlugins.Length; i++)
            {
                GameConsole.Write("Pluggin Loaded : " + WorldPlugins.Plugins.WorldPlugins[i].PluginName + " v" + WorldPlugins.Plugins.WorldPlugins[i].PluginVersion);
                WorldPlugins.Plugins.WorldPlugins[i].Initialize(_d3dEngine, _camManager, _worldFocusManager, null, _gameStateManagers);
            }
            //======================================================================
#if DEBUG
            GameConsole.Write("DX11 main engine started and initialized with feature level : " + _d3dEngine.Device.FeatureLevel);
#endif
            base.Initialize();

        }

        //Default Utopia Init method.
        private void Init()
        {
            _gameStateManagers = new GameStatesManager() { DebugActif = false, DebugDisplay = 0 };

            //Initialize the Thread Pool manager
            S33M3Engines.Threading.WorkQueue.Initialize();

            _d3dEngine = new D3DEngine(new System.Drawing.Size(W, H), "Powered By S33m3 Engine ! Rulezzz", S33M3Engines.Threading.WorkQueue.ThreadPool.Concurrency);

            //Init the 3d Engine
            _d3dEngine.Initialize();

            _d3dEngine.GameWindow.Closed += (o, args) =>
            {
                _isFormClosed = true;
            };

            if (!_d3dEngine.UnlockedMouse) System.Windows.Forms.Cursor.Hide(); //Hide the mouse by default !


            _inputHandler = new InputHandlerManager();

            //Create all States that could by used by the game.
            DXStates.CreateStates(_d3dEngine);
            //UtopiaSaveManager.Start("s33m3's World");

            WorldFocusManager _worldFocusManager = new WorldFocusManager();

            _clock = new GameClock.Clock(120, GameClock.GameTimeMode.Automatic, (float)Math.PI * 1f, _inputHandler);     // Clock creation, manage Utopia time
            GameComponents.Add(_clock);

            ICamera camera = new FirstPersonCamera(_d3dEngine, _worldFocusManager);  // Create a firstPersonCAmera viewer

            _worldFocusManager.WorldFocus = (IWorldFocus)camera; //Set the World Focus on my Camera

            _camManager = new CameraManager(camera);

            //Create an entity, link to camera to it.
            _player = new Entities.Living.Player(_d3dEngine, _camManager, _worldFocusManager, "s33m3", camera, _inputHandler,
                                                 new DVector3((LandscapeBuilder.Worldsize.X / 2.0) + LandscapeBuilder.WorldStartUpX, 90, (LandscapeBuilder.Worldsize.Z / 2.0f) + LandscapeBuilder.WorldStartUpZ),
                                                 new Vector3(0.5f, 1.9f, 0.5f),
                                                 5f, 30f, 10f)
                {
                    Mode = Entities.Living.LivingEntityMode.FreeFirstPerson //Defaulted to "Flying" mode
                };
            GameComponents.Add(_player);

            //Attached the Player to the camera =+> The player will be used as Camera Holder !
            camera.CameraPlugin = _player; //The camera is using the _player to get it's world positions and parameters, so the _player updates must be done BEFORE the camera !
            GameComponents.Add(_camManager);

            _universe = new Univers.Universe(_d3dEngine, _camManager, _worldFocusManager, _gameStateManagers, LandscapeBuilder, _clock, _player, "S33m3's World");
            GameComponents.Add(_universe);

            _fps = new FPS();
            GameComponents.Add(_fps);

            _gui = new GUI.D3D.GUI(GameComponents,_d3dEngine, ((Player)_player).Inventory);
            GameComponents.Add(_gui);

            _debugInfo = new DebugInfo(_d3dEngine);
            _debugInfo.Activated = true;
            _debugInfo.SetComponants(_fps, _clock, _universe, _player);
            GameComponents.Add(_debugInfo);

            GameConsole.Initialize(_d3dEngine);

            #region Debug Components
#if DEBUG
            _axis = new Axis(_d3dEngine, _camManager,  10, _gameStateManagers);         // Use to display the X,Y,Z axis
            GameComponents.Add(_axis);
            DebugEffect.Init(_d3dEngine);             // Default Effect used by debug componant (will be shared)
#endif
            #endregion
        }

        public override void LoadContent()
        {

            base.LoadContent();
            WorldPlugins.LoadContent();
        }

        public override void UnloadContent()
        {
            base.UnloadContent();
            WorldPlugins.UnloadContent();

            DXStates.Dispose();
            //UtopiaSaveManager.StopUSM();
        }

        public override void Update(ref GameTime TimeSpend)
        {
            KeyboardStateHandling();
            //Update Internal Components
            base.Update(ref TimeSpend);
            WorldPlugins.Update(ref TimeSpend);
        }

        public override void Interpolation(ref double interpolation_hd, ref float interpolation_ld)
        {
            KeyboardStateHandling();
            base.Interpolation(ref interpolation_hd, ref interpolation_ld);
            WorldPlugins.Interpolation(ref interpolation_hd, ref interpolation_ld);
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
            WorldPlugins.Draw();
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
