#define SINGLEPLAYERSTART

using System;
using System.IO;
using Sandbox.Client.GUI.Forms;
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

namespace Sandbox.Client
{
    public partial class GameClient : IDisposable
    {
        private static WelcomeScreen _welcomeForm;
        private ServerComponent _server;
        private IKernel _iocContainer;
        private SandboxEntityFactory _clientFactory;
        
        public GameClient()
        {
        }

        #region Public Methods
        public void Run()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            
            LoadClientsSettings();

            IocBinding("Utopia Sandbox mode", new System.Drawing.Size(1024, 600));

            _clientFactory = new SandboxEntityFactory(_iocContainer.Get<IChunkEntityImpactManager>());
            _iocContainer.Bind<EntityFactory>().ToConstant(_clientFactory).InSingletonScope().Named("Client");

            System.Net.ServicePointManager.Expect100Continue = false;

            var vars = _iocContainer.Get<RuntimeVariables>();

            vars.ApplicationDataPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "Utopia.Sandbox");

            NetworkMessageFactory.Instance.EntityFactory = _clientFactory;

            var game = CreateNewGameEngine(_iocContainer); // Create the Rendering

            _iocContainer.Bind<Game>().ToConstant(game);
            _iocContainer.Rebind<IVoxelModelStorage>().To<ModelSQLiteStorage>().InSingletonScope().WithConstructorArgument("fileName", Path.Combine(vars.ApplicationDataPath, "Common", "models.db"));

            SmartThread.SetOptimumNbrThread(0);

            //filling stages
            var stateManager = _iocContainer.Get<GameStatesManager>();

            var fade = _iocContainer.Get<FadeSwitchComponent>();

            fade.Color = new SharpDX.Color4(0,0,0,1);

            stateManager.SwitchComponent = fade;

            stateManager.RegisterState(_iocContainer.Get<LoginState>());
            stateManager.RegisterState(_iocContainer.Get<CreditsState>());
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

            //ClientSettings.Current.Settings.Login = vars.Login;
            //ClientSettings.Current.Save();
            stateManager.ActivateGameStateAsync("LoadingGame");
#else
            stateManager.ActivateGameStateAsync("Login");
#endif

            game.Run();

            //Get windows Exit reason
            game.Dispose();

            _iocContainer.Dispose();

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
                ClientSettings.Current.Settings = ClientConfig.DefaultQwerty;
                needSave = true;
            }

            if (ClientSettings.Current.Settings.GraphicalParameters.LightPropagateSteps == 0)
            {
                ClientSettings.Current.Settings.GraphicalParameters.LightPropagateSteps = 8;
                needSave = true;
            }

            return needSave;
        }
        #endregion

        public void Dispose()
        {
            if(_iocContainer != null && !_iocContainer.IsDisposed) _iocContainer.Dispose(); // Will also disposed all singleton objects that have been registered !
        }

    }
    
}
