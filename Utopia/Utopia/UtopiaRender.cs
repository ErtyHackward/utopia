using System;
using SharpDX.Direct3D11;
using Utopia.Shared.Settings;
using S33M3DXEngine.Main;
using S33M3CoreComponents.Inputs.Actions;
using S33M3DXEngine;
using System.Drawing;
using S33M3CoreComponents.Inputs;
using Utopia.Shared.GameDXStates;
using SharpDX.DXGI;
using S33M3CoreComponents.States;

namespace Utopia
{
    public partial class UtopiaRender : Game
    {
        /// <summary>
        /// Defined several lvl of LoadContant Deffered Mode
        /// 0 = Default configuration
        /// 1 = Deffered LoadContent for IconFactory disabled
        /// </summary>
        public static int LCDefferedModeLvl = 0;

        /// <summary>
        /// Gets an action manager
        /// </summary>
        private InputsManager _inputManager;

        public GameStatesManager GameStateManager;

        //Not Engine injected constructor
        public UtopiaRender(InputsManager inputManager, Size startingWindowsSize, string WindowsCaption, SampleDescription sampleDescription, Size ResolutionSize = default(Size), bool withComObjectDisposeTracking = false)
            : base(startingWindowsSize, WindowsCaption, sampleDescription, ResolutionSize, withComObjectDisposeTracking)
        {
            _inputManager = inputManager;

            VSync = true;                                              // Vsync ON (default)
        }

        public UtopiaRender(D3DEngine engine, InputsManager inputManager, bool withComObjectDisposeTracking)
            : base(engine, withComObjectDisposeTracking)
        {
            _inputManager = inputManager;

            VSync = true;                                              // Vsync ON (default)
        }

        public override void Initialize()
        {
            _inputManager.MouseManager.IsRunning = true; //Start Mosue pooling
            _inputManager.EnableComponent();

            DXStates.CreateStates(Engine);

            //Init the TexturePack State value, after the Sampling states have been initialized
            TexturePackConfig.Current.Settings.ParseSamplingFilter(TexturePackConfig.Current.Settings.SamplingFilter);

            base.Initialize();
        }

        protected override void GameWindow_FormClosing(object sender, System.Windows.Forms.FormClosingEventArgs e)
        {
            if (Engine.IsShuttingDownSafe == false)
            {
                e.Cancel = true;
                Engine.IsShuttingDownRequested = true; //Trigger an engine shutdown
            }
        }

        public override void FTSUpdate(GameTime TimeSpend)
        {
            base.FTSUpdate(TimeSpend);

            //After all update are done, Check against "System" keys like Exit, ...
            InputHandling();
        }

        public override void VTSUpdate(double interpolation_hd, float interpolation_ld, float elapsedTime)
        {
            base.VTSUpdate(interpolation_hd, interpolation_ld, elapsedTime);
        }

        private void InputHandling()
        {
            if (_inputManager.ActionsManager.isTriggered(Actions.EngineVSync))
            {
                this.VSync = !this.VSync;
            }

            //Switch full screen state
            if (_inputManager.ActionsManager.isTriggered(Actions.EngineFullScreen))
            {
                Engine.IsFullScreen = !Engine.IsFullScreen;
            }

            //Mouse capture mode
            if (_inputManager.ActionsManager.isTriggered(Actions.MouseCapture))
            {
                if (GameStateManager != null && GameStateManager.CurrentState.AllowMouseCaptureChange)
                {
                    _inputManager.MouseManager.MouseCapture = !_inputManager.MouseManager.MouseCapture;
                }
            }
        }

        public override void BeforeDispose()
        {
            DXStates.Dispose();
        }
    }
}
