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

namespace Utopia
{

    public class UtopiaRender : Game
    {
        //Debug tools
        private FPS _fps; //FPS computing object

        //Manage pluggins
        private Plugins _plugins;
        private bool _loadPluggin = true;

        //Game componants
        private GameClock.Clock _clock;
        private Entities.Living.ILivingEntity _player;
        private GUI.D3D.GUI _gui;
        private Univers.Universe _universe;

        //Debug Tool
        private DebugInfo _debugInfo;

       
#if DEBUG
        private Axis _axis;
#endif

        public UtopiaRender()
            :base(new System.Drawing.Size(1024,600))                    // Windowed screen size resolution
        {
             S33M3Engines.Threading.WorkQueue.ThreadingActif = true;    // Activate the threading Mode (Default : true, false used mainly to debug purpose)
             S33M3Engines.D3DEngine.FULLDEBUGMODE = false;
             VSync = true;                                              // Vsync ON (default)

            ////Load the pluggin ! ===================
             _plugins = new Plugins();
             if (_loadPluggin)
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

        //private void LoadSettings(XmlSettingsManager<ClientSettings.ClientConfig> SettingsManager)
        //{
        //    ClientSettings.Settings = SettingsManager;
        //    ClientSettings.Settings = new XmlSettingsManager<ClientSettings.ClientConfig>("UtopiaClient.config", SettingsStorage.ApplicationData);
        //    ClientSettings.Settings.Load();
        //    //If file was not present create a new one with the Azerty Default mapping !
        //    if (SettingsManager.Settings.KeyboardMapping == null)
        //    {
        //        SettingsManager.Settings = ClientSettings.ClientConfig.DefaultQwerty;
        //        SettingsManager.Save();
        //    }
        //}

        public override void Initialize()
        {
            DXStates.CreateStates(this.D3dEngine);
            //UtopiaSaveManager.Start("s33m3's World");

            //DebugInit();
            NormalInit();

            //Display the pluggins that have been loaded ! =========================
            for (int i = 0; i < WorldPlugins.Plugins.WorldPlugins.Length; i++)
            {
                GameConsole.Write("Pluggin Loaded : " + WorldPlugins.Plugins.WorldPlugins[i].PluginName + " v" + WorldPlugins.Plugins.WorldPlugins[i].PluginVersion);
                WorldPlugins.Plugins.WorldPlugins[i].Initialize(_universe);
            }
            //======================================================================
#if DEBUG
            GameConsole.Write("DX11 main engine started and initialized with feature level : " + D3dEngine.GraphicsDevice.FeatureLevel);
#endif
            base.Initialize(); 
        }

        //Init phase used for testing purpose
        private void DebugInit()
        {
            //ICamera camera = new FirstPersonCamera(this);  // Create a firstPersonCAmera viewer
            
            //WorldFocus = (IWorldFocus)camera;
            
            //Entities.IEntity _wisp = new Entities.Admin.Wisp(this, "Wisp", camera, InputHandler,
            //                                     new DVector3(0, 0, 3)
            //                                     );
            //GameComponents.Add(_wisp);
            ////Attached the Player to the camera =+> The player will be used as Camera Holder !
            //camera.CameraPlugin = _wisp;
            //GameComponents.Add(camera);

            //Testing.NewEffect effect = new Testing.NewEffect(this);
            //GameComponents.Add(effect);

            //base.ActivCamera = camera;
        }

        //Default Utopia Init method.
        private void NormalInit()
        {
            #region Debug Components
#if DEBUG
            _axis = new Axis(this, 10);         // Use to display the X,Y,Z axis
            GameComponents.Add(_axis);          
            DebugEffect.Init(this);             // Default Effect used by debug componant (will be shared)
#endif
            #endregion


            _clock = new GameClock.Clock(this, 120, GameClock.GameTimeMode.Automatic, (float)Math.PI * 1f, InputHandler);     // Clock creation, manage Utopia time
            GameComponents.Add(_clock);

            ICamera camera = new FirstPersonCamera(this);  // Create a firstPersonCAmera viewer
            
            WorldFocus = (IWorldFocus)camera; //Set the World Focus on my Camera

            //Create an entity, link to camera to it.
            _player = new Entities.Living.Player(this, "s33m3", camera, InputHandler,
                                                 new DVector3((LandscapeBuilder.Worldsize.X / 2.0) + LandscapeBuilder.WorldStartUpX, 90, (LandscapeBuilder.Worldsize.Z / 2.0f) + LandscapeBuilder.WorldStartUpZ),
                                                 new Vector3(0.5f, 1.9f, 0.5f),
                                                 5f, 30f, 10f)
                {
                    Mode = Entities.Living.LivingEntityMode.FreeFirstPerson //Defaulted to "Flying" mode
                };
            GameComponents.Add(_player);

            //Attached the Player to the camera =+> The player will be used as Camera Holder !
            camera.CameraPlugin = _player; //The camera is using the _player to get it's world positions and parameters, so the _player updates must be done BEFORE the camera !
            GameComponents.Add(camera);

            _universe = new Univers.Universe(this, _clock, _player, "S33m3's World");
            GameComponents.Add(_universe);

            _fps = new FPS(this);
            GameComponents.Add(_fps);

            _gui = new GUI.D3D.GUI(this);
            GameComponents.Add(_gui);

            _debugInfo = new DebugInfo(this); 
            _debugInfo.Activated = true;
            _debugInfo.SetComponants(_fps, _clock, _universe, _player);
            GameComponents.Add(_debugInfo);

            GameConsole.Initialize(this);

            //Set Default Camera !
            base.ActivCamera = camera;
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

            if (InputHandler.PrevKeyboardState.IsKeyDown(ClientSettings.Current.Settings.KeyboardMapping.DebugMode) && InputHandler.PrevKeyboardState.IsKeyDown(Keys.LControlKey) && InputHandler.CurKeyboardState.IsKeyUp(ClientSettings.Current.Settings.KeyboardMapping.DebugMode) && InputHandler.CurKeyboardState.IsKeyDown(Keys.LControlKey))
            {
                FixedTimeSteps = !FixedTimeSteps;
                GameConsole.Write("FixeTimeStep Mode : " + FixedTimeSteps.ToString());
            }

            if (InputHandler.IsKeyPressed(ClientSettings.Current.Settings.KeyboardMapping.FullScreen)) isFullScreen = !isFullScreen; //Go full screen !

            if (InputHandler.PrevKeyboardState.IsKeyDown(ClientSettings.Current.Settings.KeyboardMapping.DebugMode) && !InputHandler.PrevKeyboardState.IsKeyDown(Keys.LControlKey) && InputHandler.CurKeyboardState.IsKeyUp(ClientSettings.Current.Settings.KeyboardMapping.DebugMode) && !InputHandler.CurKeyboardState.IsKeyDown(Keys.LControlKey))
            {
                DebugActif = !DebugActif;
                if (!DebugActif)
                {
                    DebugDisplay = 0;
                }
            }
            if (InputHandler.IsKeyPressed(Keys.Up))
            {
                if (!DebugActif) return;
                DebugDisplay++;
                if (DebugDisplay > 2) DebugDisplay = 2;
            }
            if (InputHandler.IsKeyPressed(Keys.Down))
            {
                if (!DebugActif) return;
                DebugDisplay--;
                if (DebugDisplay < 0) DebugDisplay = 0;
            }

            if (InputHandler.IsKeyPressed(ClientSettings.Current.Settings.KeyboardMapping.LockMouseCursor))
            {
                UnlockedMouse = !UnlockedMouse;
            }

            if (InputHandler.IsKeyPressed(ClientSettings.Current.Settings.KeyboardMapping.VSync))
            {
                VSync = !VSync;
            }

            if (InputHandler.IsKeyPressed(ClientSettings.Current.Settings.KeyboardMapping.DebugInfo))
            {
                _debugInfo.Activated = !_debugInfo.Activated;
            }

            if (InputHandler.IsKeyPressed(ClientSettings.Current.Settings.KeyboardMapping.Console)) GameConsole.Show = !GameConsole.Show;

            //Exit application
            if (InputHandler.IsKeyPressed(Keys.Escape)) Exit();
        }

        public override void Draw()
        {
            D3dEngine.Context.ClearRenderTargetView(RenderTarget, BackBufferColor);
            D3dEngine.Context.ClearDepthStencilView(DepthStencilTarget, DepthStencilClearFlags.Depth, 1.0f, 0);

            base.Draw();
            WorldPlugins.Draw();
            base.DrawInterfaces();

            base.Present();
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
