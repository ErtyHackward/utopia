﻿using System;
using LostIsland.Client.Components;
using LostIsland.Client.States;
using LostIsland.Shared;
using Utopia;
using Utopia.Components;
using Utopia.Network;
using Ninject;
using S33M3Engines.D3D;
using System.Windows.Forms;
using Utopia.Settings;
using Utopia.Shared.Config;
using LostIsland.Client.GUI.Forms;
using Utopia.Shared.Entities;
using Utopia.Worlds.Chunks.ChunkEntityImpacts;

namespace LostIsland.Client
{
    public partial class GameClient : IDisposable
    {
        private static WelcomeScreen _welcomeForm;
        private Server _server;
        private IKernel _iocContainer;
        private GameExitReasonMessage _exitRease;
        
        public GameClient()
        {
            _exitRease = new GameExitReasonMessage() { GameExitReason = ExitReason.UserRequest };
        }

        #region Public Methods
        public void Run()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            
            LoadClientsSettings();

            IocBinding();

            EntityFactory.Instance = new LostIslandEntityFactory(_iocContainer.Get<IChunkEntityImpactManager>());
            
            var game = CreateNewGameEngine(_iocContainer); // Create the Rendering

            _iocContainer.Bind<Game>().ToConstant(game);

            //filling stages

            var stateManager = _iocContainer.Get<StatesManager>();

            var fade = _iocContainer.Get<FadeComponent>();

            fade.Color = new SharpDX.Color4(0,0,0,1);

            stateManager.SwitchComponent = fade;

            stateManager.RegisterState(_iocContainer.Get<LoginState>());
            stateManager.RegisterState(_iocContainer.Get<CreditsState>());
            stateManager.RegisterState(_iocContainer.Get<MainMenuState>());
            stateManager.RegisterState(_iocContainer.Get<GamePlayState>());
            stateManager.RegisterState(_iocContainer.Get<GameLoadingState>());
            stateManager.RegisterState(_iocContainer.Get<GameInventoryState>());

            //stateManager.PrepareState("Login");
            // first state will be the login
            stateManager.SetGameState("Login");

            game.Run();

            //Get windows Exit reason
            _exitRease = game.GameExitReason;
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
