using System;
using System.Windows.Forms;
using Ninject;
using Realms.Client.Components.GUI;
using S33M3CoreComponents.Inputs;
using S33M3CoreComponents.Sound;
using S33M3CoreComponents.States;
using S33M3CoreComponents.GUI;
using Utopia.Shared.Net.Web;
using Utopia.Shared.Settings;

namespace Realms.Client.States
{
    /// <summary>
    /// Main menu state handling
    /// </summary>
    public class MainMenuState : GameState
    {
        private readonly IKernel _ioc;
        private RealmRuntimeVariables _vars;

        public override string Name
        {
            get { return "MainMenu"; }
        }

        public MainMenuState(GameStatesManager stateManager, IKernel iocContainer)
            :base(stateManager)
        {
            _ioc = iocContainer;
            AllowMouseCaptureChange = false;
        }

        public override void Initialize(SharpDX.Direct3D11.DeviceContext context)
        {
            var bg = _ioc.Get<BlackBgComponent>();
            var gui = _ioc.Get<GuiManager>();
            var menu = _ioc.Get<MainMenuComponent>();
            _vars = _ioc.Get<RealmRuntimeVariables>();

            AddComponent(bg);
            AddComponent(gui);
            AddComponent(menu);

            base.Initialize(context);
        }

        public override void OnEnabled(GameState previousState)
        {
            var menu = _ioc.Get<MainMenuComponent>();
            menu.CreditsPressed += MenuCreditsPressed;
            menu.SinglePlayerPressed += MenuSinglePlayerPressed;
            menu.MultiplayerPressed += MenuMultiplayerPressed;
            menu.EditorPressed += MenuEditorPressed;
            menu.SettingsButtonPressed += MenuSettingsButtonPressed;
            menu.LogoutPressed += MenuLogoutPressed;
            menu.ExitPressed += MenuExitPressed;

            var guiManager = _ioc.Get<GuiManager>();
            if (_vars.DisposeGameComponents)
            {
                _vars.DisposeGameComponents = false;

                var inputManager = _ioc.Get<InputsManager>();
                inputManager.MouseManager.MouseCapture = false;

                if (!string.IsNullOrEmpty(_vars.MessageOnExit))
                {
                    guiManager.MessageBox(_vars.MessageOnExit, "Information");
                    _vars.MessageOnExit = null;
                }

                var soundEngine = _ioc.Get<ISoundEngine>();
                soundEngine.StopAllSounds();

                //Dispose all components related to the Game scope
                GameScope.CurrentGameScope.Dispose();
                //Create a new Scope
                GameScope.CreateNewScope();
            }

            base.OnEnabled(previousState);
        }
        

        public override void OnDisabled(GameState nextState)
        {
            var menu = _ioc.Get<MainMenuComponent>();
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
            var guiManager = _ioc.Get<GuiManager>();
            var webApi = _ioc.Get<ClientWebApi>();

            if (string.IsNullOrEmpty(webApi.Token))
            {
                guiManager.MessageBox("Server authorization was failed. Did you confirm your email? (check the spam folder). You will unable to play multiplayer without confirmation.", "error");
                return;
            }

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
