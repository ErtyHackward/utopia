#define SINGLEPLAYERSTART

using System;
using System.IO;
using Sandbox.Client.States;
using Sandbox.Shared;
using Utopia;
using Utopia.Components;
using Utopia.Entities;
using Utopia.Network;
using Ninject;
using System.Windows.Forms;
using Utopia.Settings;
using Utopia.Shared.Config;
using Utopia.Shared.Entities;
using Utopia.Shared.Interfaces;
using Utopia.Shared.Net.Messages;
using Utopia.Worlds.Chunks.ChunkEntityImpacts;
using S33M3DXEngine.Main;
using S33M3CoreComponents.States;
using S33M3DXEngine.Threading;
using S33M3CoreComponents.Inputs;
using S33M3CoreComponents.GUI;
using S33M3CoreComponents.Debug;
using Ninject.Parameters;
using SharpDX;
using S33M3DXEngine;
using Sandbox.Client.Components;
using Sandbox.Client.Components.GUI;
using Sandbox.Client.Components.GUI.Settings;
using Utopia.Shared.Settings;

namespace Sandbox.Client
{
    public partial class GameClient : IDisposable
    {
        private IKernel _iocContainer;
        private D3DEngine _d3dEngine;
        
        public GameClient()
        {
        }

        #region Public Methods
        public void Run()
        {
            //Application.EnableVisualStyles();
            //Application.SetCompatibleTextRenderingDefault(false);
            
            //Load Client config XML file
            LoadClientsSettings();
            //Bings all components
            IocBinding("Utopia Sandbox mode", new System.Drawing.Size(1024, 640));

            _d3dEngine.GameWindow.Icon = Sandbox.Client.Properties.Resources.Utopia;

            System.Net.ServicePointManager.Expect100Continue = false;

            var vars = _iocContainer.Get<RuntimeVariables>();

            vars.ApplicationDataPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "Utopia.Sandbox");

            NetworkMessageFactory.Instance.EntityFactory = _iocContainer.Get<EntityFactory>();

            var game = CreateNewGameEngine(_iocContainer, ClientSettings.Current.Settings.GraphicalParameters.VSync); // Create the Rendering
            _iocContainer.Bind<Game>().ToConstant(game);

            var settings = _iocContainer.Get<SettingsComponent>();
            settings.KeyBindingChanged += (sender, e) =>
            {
                this.BindActions(_iocContainer.Get<InputsManager>(), true);
            };

            _iocContainer.Rebind<IVoxelModelStorage>().To<ModelSQLiteStorage>().InSingletonScope().WithConstructorArgument("fileName", Path.Combine(vars.ApplicationDataPath, "Common", "models.db"));

            SandboxMenuComponent.LoadCommonImages(_iocContainer.Get<D3DEngine>());

            //filling stages
            var stateManager = _iocContainer.Get<GameStatesManager>();

            var fade = _iocContainer.Get<FadeSwitchComponent>();

            fade.Color = new SharpDX.Color4(0,0,0,1);

            stateManager.SwitchComponent = fade;

            stateManager.RegisterState(_iocContainer.Get<LoginState>());
            stateManager.RegisterState(_iocContainer.Get<CreditsState>());
            stateManager.RegisterState(_iocContainer.Get<SettingsState>());
            stateManager.RegisterState(_iocContainer.Get<MainMenuState>());
            stateManager.RegisterState(_iocContainer.Get<LoadingGameState>());
            stateManager.RegisterState(_iocContainer.Get<GamePlayState>());
            stateManager.RegisterState(_iocContainer.Get<SelectServerGameState>());
            stateManager.RegisterState(_iocContainer.Get<EditorState>());

            //Add system components that will be share with all possible states !
            InputsManager inputManager = _iocContainer.Get<InputsManager>();
            inputManager.MouseManager.IsRunning = true;
            game.GameComponents.Add(inputManager);
            var debugComponents = _iocContainer.Get<DebugComponent>(new ConstructorArgument("withDisplayInfoActivated", true));
            debugComponents.EnableComponent();
            game.GameComponents.Add(debugComponents);
            //Add the StateManager to the main loop
            game.GameComponents.Add(stateManager);

#if SINGLEPLAYERSTART
            // first state will be the login state
            vars.SinglePlayer = true;
            vars.Login = "test";
            vars.PasswordHash = "";
            vars.DisplayName = "s33m3";

            //stateManager.ActivateGameStateAsync("LoadingGame");
            stateManager.ActivateGameStateAsync("MainMenu");
#else
            stateManager.ActivateGameStateAsync("Login");
#endif

            //game.MenuRequested += new EventHandler(game_MenuRequested);

            game.Run(); //Start the Main render loop

            _iocContainer.Dispose();

            game.Dispose();

            GC.Collect();
            GC.WaitForPendingFinalizers();

        }
        #endregion

        #region Private methods
        private void LoadClientsSettings()
        {
            ClientSettings.Current = new XmlSettingsManager<ClientConfig>("UtopiaClient.config", SettingsStorage.ApplicationData);
            ClientSettings.Current.Load();

            if (ValidateSettings()) 
                ClientSettings.Current.Save();

            //Load the Actif texture pack config
            TexturePackConfig.Current = new XmlSettingsManager<TexturePackSetting>(@"TexturePackConfig.xml", SettingsStorage.CustomPath) { CustomSettingsFolderPath = @"TexturesPacks\" + ClientSettings.Current.Settings.GraphicalParameters.TexturePack + @"\" };
            TexturePackConfig.Current.Load();
        }

        /// <summary>
        /// Do validation on the settings values
        /// </summary>
        /// <returns></returns>
        private bool ValidateSettings()
        {
            var needSave = false;

            //If file was not present create a new one with the Qwerty Default mapping !
            if (ClientSettings.Current.Settings.KeyboardMapping == null)
            {
                var keyboardType = System.Globalization.CultureInfo.CurrentCulture.KeyboardLayoutId;

                if (keyboardType == 2060 || keyboardType == 1036)
                {
                    ClientSettings.Current.Settings = ClientConfig.DefaultAzerty;
                }
                else
                {
                    ClientSettings.Current.Settings = ClientConfig.DefaultQwerty;
                }
                needSave = true;
            }

            //Set Default Threads - initializing the thread Engine component
            if (ClientSettings.Current.Settings.DefaultAllocatedThreads == 0)
            {
                ClientSettings.Current.Settings.DefaultAllocatedThreads = SmartThread.SetOptimumNbrThread(0);
                needSave = true;
            }
            else
            {
                SmartThread.SetOptimumNbrThread(ClientSettings.Current.Settings.DefaultAllocatedThreads + ClientSettings.Current.Settings.EngineParameters.AllocatedThreadsModifier, true);
            }

            if (string.IsNullOrEmpty(ClientSettings.Current.Settings.EngineParameters.EffectPack))
                ClientSettings.Current.Settings.EngineParameters.EffectPack = "Default";
            if (string.IsNullOrEmpty(ClientSettings.Current.Settings.GraphicalParameters.TexturePack))
                ClientSettings.Current.Settings.GraphicalParameters.TexturePack = "Default";

            return needSave;
        }


        private void game_MenuRequested(object sender, EventArgs e)
        {
            InputsManager inputManager = _iocContainer.Get<InputsManager>();
            inputManager.MouseManager.MouseCapture = false;
            _iocContainer.Get<GameStatesManager>().ActivateGameStateAsync(_iocContainer.Get<MainMenuState>(), true);
        }
        #endregion

        public void Dispose()
        {
            if(_iocContainer != null && !_iocContainer.IsDisposed) _iocContainer.Dispose(); // Will also disposed all singleton objects that have been registered !
        }

    }
    
}
