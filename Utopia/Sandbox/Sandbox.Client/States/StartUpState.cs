using System;
using System.Collections.Generic;
using S33M3CoreComponents.States;
using Ninject;
using Utopia.Components;
using System.IO;
using S33M3CoreComponents.Inputs;

namespace Sandbox.Client.States
{
    public class StartUpState : GameState
    {
        private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        #region Private variables
        private IKernel _iocContainer;
        private bool _systemComponentInitialized, _slideShowFinished;
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

            inputsManager.KeyboardManager.IsRunning = true;

            //Get the list of Slides
            List<FileInfo> slides = new List<FileInfo>();
            foreach (var slide in Directory.GetFiles(@"Images\StartUpSlides\", "StartUpSlide*.*"))
            {
                slides.Add(new FileInfo(slide));
            }
            
            startUpComponent.SetSlideShows(slides.ToArray(), 3000);

            startUpComponent.SlideShowFinished += startUpComponent_SlideShowFinished;

            //Prepare Async SystemComponentState
            GameState systemComponentState = StatesManager.GetByName("SystemComponents");
            systemComponentState.StateInitialized += systemComponentState_StateInitialized;

            StatesManager.PrepareStateAsync(systemComponentState);

            AddComponent(startUpComponent);
            AddComponent(inputsManager);
            base.Initialize(context);
        }
        #endregion

        #region Private methods
        private void systemComponentState_StateInitialized(object sender, EventArgs e)
        {
            StatesManager.GetByName("SystemComponents").StateInitialized -= systemComponentState_StateInitialized;
            _systemComponentInitialized = true;
            CheckForLoginStartUp();
        }

        private void startUpComponent_SlideShowFinished(object sender, EventArgs e)
        {
            var startUpComponent = _iocContainer.Get<StartUpComponent>();
            startUpComponent.SlideShowFinished -= startUpComponent_SlideShowFinished;

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

#if !DEBUG
                    // first state will be the login state
                    var vars = _iocContainer.Get<RuntimeVariables>();
                    vars.SinglePlayer = true;
                    vars.Login = "test";
                    vars.PasswordHash = "";
                    vars.DisplayName = "s33m3";

                    //stateManager.ActivateGameStateAsync("LoadingGame");
                    StatesManager.ActivateGameStateAsync("MainMenu");
#else
                    StatesManager.ActivateGameStateAsync("Login");
#endif

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
