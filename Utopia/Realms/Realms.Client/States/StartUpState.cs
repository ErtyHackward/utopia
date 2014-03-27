#define SINGLEPLAYERSTART

using System;
using System.Collections.Generic;
using System.Threading;
using S33M3CoreComponents.States;
using Ninject;
using Utopia.Components;
using System.IO;
using S33M3CoreComponents.Inputs;
using Utopia.Shared.Net.Web;
using Utopia.Shared.Settings;
using Utopia.Shared.GraphicManagers;

namespace Realms.Client.States
{
    public class StartUpState : GameState
    {
        private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        #region Private variables
        private IKernel _iocContainer;
        private bool _systemComponentInitialized, _slideShowFinished;
        private ClientWebApi _webApi;

        // indicates if we could switch to next state
        private string _nextState;
        #endregion

        #region Public variables/Properties
        public override string Name
        {
            get { return "StartUp"; }
        }
        #endregion

        public StartUpState(GameStatesManager stateManager, IKernel iocContainer)
            :base(stateManager)
        {
            _iocContainer = iocContainer;
            AllowMouseCaptureChange = false;
            _slideShowFinished = false;
            _systemComponentInitialized = false;
        }

        #region Public methods
        public override void Initialize(SharpDX.Direct3D11.DeviceContext context)
        {
            var startUpComponent = _iocContainer.Get<StartUpComponent>();
            var inputsManager = _iocContainer.Get<InputsManager>();
            _webApi = _iocContainer.Get<ClientWebApi>();

            inputsManager.KeyboardManager.IsRunning = true;

            //Get the list of Slides
            List<FileInfo> slides = new List<FileInfo>();
            foreach (var slide in Directory.GetFiles(@"Images\StartUpSlides\", "StartUpSlide*.*"))
            {
                slides.Add(new FileInfo(slide));
            }

            startUpComponent.SetSlideShows(slides.ToArray(), 3000);

            startUpComponent.SlideShowFinished += StartUpComponentSlideShowFinished;

            //Prepare Async SystemComponentState
            GameState systemComponentState = StatesManager.GetByName("SystemComponents");
            systemComponentState.StateInitialized += SystemComponentStateStateInitialized;

            StatesManager.PrepareStateAsync(systemComponentState);

            _webApi.TokenVerified += WebApiTokenVerified;
            if (!string.IsNullOrEmpty(ClientSettings.Current.Settings.Token))
            {
                _webApi.OauthVerifyTokenAsync(ClientSettings.Current.Settings.Token);
            }
            else
            {
                _nextState = "Login";
            }

            AddComponent(startUpComponent);
            AddComponent(inputsManager);
            base.Initialize(context);
        }

        void WebApiTokenVerified(object sender, Utopia.Shared.Net.Web.Responses.VerifyResponse e)
        {
            if (e.Error == 0 && e.Exception == null && !string.IsNullOrEmpty(e.DisplayName))
            {
                var vars = _iocContainer.Get<RealmRuntimeVariables>();

                vars.Login = ClientSettings.Current.Settings.Login;
                vars.PasswordHash = ClientSettings.Current.Settings.PasswordHash;
                vars.DisplayName = e.DisplayName;

                _nextState = "MainMenu";
                return;
            }

            _nextState = "Login";
        }
        #endregion

        #region Private methods
        private void SystemComponentStateStateInitialized(object sender, EventArgs e)
        {
            StatesManager.GetByName("SystemComponents").StateInitialized -= SystemComponentStateStateInitialized;
            _systemComponentInitialized = true;
            CheckForLoginStartUp();
        }

        private void StartUpComponentSlideShowFinished(object sender, EventArgs e)
        {
            var startUpComponent = _iocContainer.Get<StartUpComponent>();
            startUpComponent.SlideShowFinished -= StartUpComponentSlideShowFinished;

            _slideShowFinished = true;
            CheckForLoginStartUp();
        }

        private void CheckForLoginStartUp()
        {
            if (_systemComponentInitialized && _slideShowFinished)
            {
                StatesManager.DeactivateSwitchComponent = true;
                if (StatesManager.ActivateGameState("SystemComponents", true))
                {
                    StatesManager.ForceCurrentState(this);

                    StatesManager.DeactivateSwitchComponent = false;

                    while (string.IsNullOrEmpty(_nextState))
                    {
                        Thread.Sleep(100);
                    }

                    StatesManager.ActivateGameStateAsync(_nextState);
                }
                else
                {
                    logger.Error("Initialization of startUp synchro problem !");
                }
            }
        }
        #endregion
    }
}
