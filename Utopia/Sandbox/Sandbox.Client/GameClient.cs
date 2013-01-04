using System;
using System.Reflection;
using Sandbox.Client.States;
using Utopia.Components;
using Ninject;
using S33M3DXEngine.Main;
using S33M3CoreComponents.States;
using S33M3DXEngine.Threading;
using S33M3CoreComponents.Inputs;
using S33M3DXEngine;
using Sandbox.Client.Components.GUI.Settings;
using Utopia.Shared.Entities;
using Utopia.Shared.Settings;
using S33M3CoreComponents.Config;

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
            EntityFactory.InitializeProtobufInheritanceHierarchy();

            //Load Client config XML file
            LoadClientsSettings();

            //Bings all components
            IocBinding("Utopia Sandbox v" + Assembly.GetExecutingAssembly().GetName().Version, Program.StartUpResolution);

            //Set Windows Icon
            _d3dEngine.GameWindow.Icon = Sandbox.Client.Properties.Resources.Utopia;

            System.Net.ServicePointManager.Expect100Continue = false;

            // Create the Rendering Main LOOP
            var game = CreateNewGameEngine(_iocContainer, ClientSettings.Current.Settings.GraphicalParameters.VSync); 
            _iocContainer.Bind<Game>().ToConstant(game);

            //Everytime a Key binding is change, we need to refresh the Bindings from InputManagers
            var settings = _iocContainer.Get<SettingsComponent>();
            settings.KeyBindingChanged += (sender, e) =>
            {
                this.BindActions(_iocContainer.Get<InputsManager>(), true);
            };

            //filling stages
            var stateManager = _iocContainer.Get<GameStatesManager>();
            GameScope.StateManager = stateManager;

            var fade = _iocContainer.Get<FadeSwitchComponent>();
            fade.Color = new SharpDX.Color4(0,0,0,1);
            stateManager.RegisterState(_iocContainer.Get<LoginState>());
            stateManager.RegisterState(_iocContainer.Get<CreditsState>());
            stateManager.RegisterState(_iocContainer.Get<SettingsState>());
            stateManager.RegisterState(_iocContainer.Get<MainMenuState>());
            stateManager.RegisterState(_iocContainer.Get<LoadingGameState>());
            stateManager.RegisterState(_iocContainer.Get<GamePlayState>());
            stateManager.RegisterState(_iocContainer.Get<SelectServerGameState>());
            stateManager.RegisterState(_iocContainer.Get<EditorState>());
            stateManager.RegisterState(_iocContainer.Get<SinglePlayerMenuState>());
            stateManager.RegisterState(_iocContainer.Get<StartUpState>());
            stateManager.RegisterState(_iocContainer.Get<SystemComponentsState>());
            stateManager.RegisterState(_iocContainer.Get<InGameMenuState>());

            stateManager.SwitchComponent = fade;
            game.GameComponents.Add(stateManager);

            stateManager.ActivateGameStateAsync("StartUp");

            game.MenuRequested += game_MenuRequested;
            game.GameStateManager = stateManager;

            ApplySystemSettings();

            game.Run(); //Start the Main render loop

            _iocContainer.Dispose();

            GC.Collect();
            GC.WaitForPendingFinalizers();
        }
        #endregion

        #region Private methods
        private void LoadClientsSettings()
        {
            ClientSettings.Current = new XmlSettingsManager<ClientConfig>(@"Client\client.config", SettingsStorage.ApplicationData, null);
            ClientSettings.Current.Load();

            if (ValidateSettings()) 
                ClientSettings.Current.Save();

            //Load the Actif texture pack config
            TexturePackConfig.Current = new XmlSettingsManager<TexturePackSetting>(@"TexturePackConfig.xml", SettingsStorage.CustomPath, @"TexturesPacks\" + ClientSettings.Current.Settings.GraphicalParameters.TexturePack + @"\");
            TexturePackConfig.Current.Load();
        }

        private void ApplySystemSettings()
        {
            var game = _iocContainer.Get<Game>();

            if (ClientSettings.Current.Settings.FrameLimiter > 0)
            {
                game.FramelimiterTime = (long)(1.0 / ClientSettings.Current.Settings.FrameLimiter * 1000.0);
                game.VSync = false;
            }
            else
            {
                game.FramelimiterTime = 0;
            }
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
                ClientSettings.Current.Settings.DefaultAllocatedThreads = ThreadsManager.SetOptimumNbrThread(0);
                needSave = true;
            }
            else
            {
                ThreadsManager.SetOptimumNbrThread(ClientSettings.Current.Settings.DefaultAllocatedThreads + ClientSettings.Current.Settings.EngineParameters.AllocatedThreadsModifier, true);
            }

            if (ClientSettings.Current.Settings.GraphicalParameters.StaticEntityViewSize == 0)
            {
                ClientSettings.Current.Settings.GraphicalParameters.StaticEntityViewSize = ClientSettings.Current.Settings.GraphicalParameters.WorldSize - 5;
                needSave = true;
            }

            if (string.IsNullOrEmpty(ClientSettings.Current.Settings.EngineParameters.EffectPack))
                ClientSettings.Current.Settings.EngineParameters.EffectPack = "Default";
            if (string.IsNullOrEmpty(ClientSettings.Current.Settings.GraphicalParameters.TexturePack))
                ClientSettings.Current.Settings.GraphicalParameters.TexturePack = "Default";

            if (ClientSettings.Current.Settings.GraphicalParameters.MSAA == null || ClientSettings.Current.Settings.GraphicalParameters.MSAA.SampleDescription.Count == 0)
            {
                ClientSettings.Current.Settings.GraphicalParameters.MSAA = new SampleDescriptionSetting() { SampleDescription = new SharpDX.DXGI.SampleDescription(1, 0) };
                needSave = true;
            }

                        //Set Default Threads - initializing the thread Engine component
            if (ClientSettings.Current.Settings.GraphicalParameters.LandscapeFog == null)
            {
                ClientSettings.Current.Settings.GraphicalParameters.LandscapeFog = "SkyFog";
            }

            return needSave;
        }


        private void game_MenuRequested(object sender, EventArgs e)
        {
            //Only if GamePlayState is activate
            if (_iocContainer.Get<GameStatesManager>().CurrentState is GamePlayState)
            {
                _iocContainer.Get<GameStatesManager>().ActivateGameStateAsync(_iocContainer.Get<InGameMenuState>(), true);
            }
        }
        #endregion

        public void Dispose()
        {
            if(_iocContainer != null && !_iocContainer.IsDisposed) _iocContainer.Dispose(); // Will also disposed all singleton objects that have been registered !
        }

    }
    
}
