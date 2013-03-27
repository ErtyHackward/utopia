﻿using System;
using System.Windows.Forms;
using Ninject;
using Realms.Client.Components.GUI;
using S33M3CoreComponents.States;
using S33M3CoreComponents.GUI;
using Utopia.Shared.Settings;

namespace Realms.Client.States
{
    /// <summary>
    /// Main menu state handling
    /// </summary>
    public class MainMenuState : GameState
    {
        private readonly IKernel _iocContainer;
        private RealmRuntimeVariables _vars;

        public override string Name
        {
            get { return "MainMenu"; }
        }

        public MainMenuState(GameStatesManager stateManager, IKernel iocContainer)
            :base(stateManager)
        {
            _iocContainer = iocContainer;
            AllowMouseCaptureChange = false;
        }

        public override void Initialize(SharpDX.Direct3D11.DeviceContext context)
        {
            var bg = _iocContainer.Get<BlackBgComponent>();
            var gui = _iocContainer.Get<GuiManager>();
            var menu = _iocContainer.Get<MainMenuComponent>();
            _vars = _iocContainer.Get<RealmRuntimeVariables>();

            AddComponent(bg);
            AddComponent(gui);
            AddComponent(menu);

            base.Initialize(context);
        }

        public override void OnEnabled(GameState previousState)
        {
            var menu = _iocContainer.Get<MainMenuComponent>();
            menu.CreditsPressed += MenuCreditsPressed;
            menu.SinglePlayerPressed += MenuSinglePlayerPressed;
            menu.MultiplayerPressed += MenuMultiplayerPressed;
            menu.EditorPressed += MenuEditorPressed;
            menu.SettingsButtonPressed += MenuSettingsButtonPressed;
            menu.LogoutPressed += MenuLogoutPressed;
            menu.ExitPressed += MenuExitPressed;

            StatesManager.PrepareStateAsync("LoadingGame");
            base.OnEnabled(previousState);
        }



        public override void OnDisabled(GameState nextState)
        {
            var menu = _iocContainer.Get<MainMenuComponent>();
            menu.CreditsPressed -= MenuCreditsPressed;
            menu.SinglePlayerPressed -= MenuSinglePlayerPressed;
            menu.MultiplayerPressed -= MenuMultiplayerPressed;
            menu.EditorPressed -= MenuEditorPressed;
            menu.SettingsButtonPressed -= MenuSettingsButtonPressed;
            menu.LogoutPressed -= MenuLogoutPressed;
            menu.ExitPressed -= MenuExitPressed;
        }

        void MenuSettingsButtonPressed(object sender, EventArgs e)
        {
            var state = StatesManager.GetByName("Settings");
            state.StatesManager.ActivateGameStateAsync(state);
        }

        void MenuEditorPressed(object sender, EventArgs e)
        {
            StatesManager.ActivateGameStateAsync("Editor");
        }

        void MenuLogoutPressed(object sender, EventArgs e)
        {
            ClientSettings.Current.Settings.Token = null;
            ClientSettings.Current.Save();

            StatesManager.ActivateGameStateAsync("Login");
        }

        void MenuExitPressed(object sender, EventArgs e)
        {
            Application.ExitThread();
        }

        void MenuMultiplayerPressed(object sender, EventArgs e)
        {
            _vars.SinglePlayer = false;
            StatesManager.ActivateGameStateAsync("SelectServer");
        }

        void MenuSinglePlayerPressed(object sender, EventArgs e)
        {
            _vars.SinglePlayer = true;
            StatesManager.ActivateGameStateAsync("SinglePlayerMenu");
        }
        
        void MenuCreditsPressed(object sender, EventArgs e)
        {
            // showing the credits
            StatesManager.ActivateGameStateAsync("Credits");
        }
    }
}
