using System;
using SharpDX.Direct3D11;
using Utopia.GameDXStates;
using Utopia.Settings;
using Utopia.Shared.Settings;
using S33M3_DXEngine.Main;
using S33M3_CoreComponents.Inputs.Actions;
using S33M3_DXEngine;
using System.Drawing;
using S33M3_CoreComponents.Inputs;

namespace Utopia
{
    public partial class UtopiaRender : Game
    {
        /// <summary>
        /// Gets an action manager
        /// </summary>
        private InputsManager _inputManager;

        //Not Engine injected constructor
        public UtopiaRender(InputsManager inputManager, Size startingWindowsSize, string WindowsCaption, Size ResolutionSize = default(Size), bool withComObjectDisposeTracking = false)
            : base(startingWindowsSize, WindowsCaption, ResolutionSize, withComObjectDisposeTracking)
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
            base.Initialize();
        }

        void GameWindow_Closed(object sender, EventArgs e)
        {
            _isFormClosed = true; //Subscribe to Close event
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
                this.Exit();
            }

            //Switch full screen state
            if (_inputManager.ActionsManager.isTriggered(Actions.EngineFullScreen))
            {
                Engine.isFullScreen = !Engine.isFullScreen;
            }

            //Mouse capture mode
            if (_inputManager.ActionsManager.isTriggered(Actions.MouseCapture))
            {
                _inputManager.MouseManager.MouseCapture = !_inputManager.MouseManager.MouseCapture;
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
