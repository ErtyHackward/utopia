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
        /// Gets an action manager
        /// </summary>
        private InputsManager _inputManager;

        public GameStatesManager _gameStateManager;
        public event EventHandler MenuRequested;

        //Not Engine injected constructor
        public UtopiaRender(InputsManager inputManager, Size startingWindowsSize, string WindowsCaption, SampleDescription sampleDescription, Size ResolutionSize = default(Size), bool withComObjectDisposeTracking = false)
            : base(startingWindowsSize, WindowsCaption,sampleDescription,  ResolutionSize, withComObjectDisposeTracking)
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
            _isFormClosed = true; //Subscribe to Close event
            //e.Cancel = true;
        }

        public override void Update(GameTime TimeSpend)
        {
            base.Update(TimeSpend);

            //After all update are done, Check against "System" keys like Exit, ...
            InputHandling();
        }

        public override void Interpolation(double interpolation_hd, float interpolation_ld, long timePassed)
        {
            base.Interpolation(interpolation_hd, interpolation_ld, timePassed);
        }

        private void InputHandling()
        {
            //Make the game exit
            if (_inputManager.ActionsManager.isTriggered(Actions.EngineExit))
            {
                if (MenuRequested != null)
                {
                    MenuRequested(this, null);
                }
                else
                {
                    this.Exit(false);
                }
            }

            //Switch full screen state
            if (_inputManager.ActionsManager.isTriggered(Actions.EngineFullScreen))
            {
                Engine.isFullScreen = !Engine.isFullScreen;
            }

            //Mouse capture mode
            if (_inputManager.ActionsManager.isTriggered(Actions.MouseCapture))
            {
                if (_gameStateManager != null && _gameStateManager.CurrentState.AllowMouseCaptureChange)
                {
                    _inputManager.MouseManager.MouseCapture = !_inputManager.MouseManager.MouseCapture;
                }
            }
        }

        public override void Dispose()
        {
            DXStates.Dispose();

            GameSystemSettings.Current.Settings.CleanUp();
            base.Dispose();
        }
    }
}
