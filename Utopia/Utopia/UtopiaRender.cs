using System;
using S33M3Engines;
using S33M3Engines.D3D;
using SharpDX.Direct3D11;
using Utopia.GameDXStates;
using Utopia.Settings;
using Utopia.Action;
using Utopia.Shared.Settings;
using S33M3Engines.D3D.Effects.Basics;

namespace Utopia
{
    public partial class UtopiaRender : Game
    {
        /// <summary>
        /// Gets an action manager
        /// </summary>
        public ActionsManager ActionsManager { get; protected set; }
        
        public UtopiaRender(D3DEngine engine)
            : base(engine)
        {
            ActionsManager = new ActionsManager(engine);

            VSync = true;                                              // Vsync ON (default)
        }

        public override void Initialize()
        {
            DXStates.CreateStates(_d3dEngine);
            base.Initialize();
        }

        void GameWindow_Closed(object sender, EventArgs e)
        {
            _isFormClosed = true; //Subscribe to Close event
        }

        //State management

        /// <summary>
        /// Check server connection change state !!
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        //void ServerConnection_ConnectionStatusChanged(object sender, ConnectionStatusEventArgs e)
        //{
        //    if (e.Status == ConnectionStatus.Disconnected && e.Exception != null)
        //    {
        //        GameExitReasonMessage msg = new GameExitReasonMessage()
        //        {
        //            GameExitReason = ExitReason.Error,
        //            MainMessage = "Server connection lost",
        //            DetailedMessage = "Reason : " + e.Reason.ToString() + Environment.NewLine + "Detail : " + e.Exception.Message
        //        };
        //        Exit(msg);
        //    }
        //}

        

        public override void LoadContent()
        {
            base.LoadContent();
        }

        public override void UnloadContent()
        {
            base.UnloadContent();
            DXStates.Dispose();
        }

        public override void Update(ref GameTime TimeSpend)
        {
            ActionsManager.FetchInputs();
            ActionsManager.Update();
            base.Update(ref TimeSpend);

            //After all update are done, Check against "System" keys like Exit, ...
            InputHandling();
        }

        public override void Interpolation(ref double interpolation_hd, ref float interpolation_ld)
        {
            ActionsManager.FetchInputs();
            base.Interpolation(ref interpolation_hd, ref interpolation_ld);
        }

        public override void Draw()
        {
            _d3dEngine.Context.ClearRenderTargetView(_d3dEngine.RenderTarget, BackBufferColor);
            _d3dEngine.Context.ClearDepthStencilView(_d3dEngine.DepthStencilTarget, DepthStencilClearFlags.Depth, 1.0f, 0);
            base.Draw();
            base.Present();
        }


        private void InputHandling()
        {
            //Exit application
            if (ActionsManager.isTriggered(Actions.Engine_Exit))
            {
                GameExitReasonMessage msg = new GameExitReasonMessage()
                {
                    GameExitReason = ExitReason.UserRequest,
                    MainMessage = "User Requested exit"
                };
            
                Exit(msg);
            }
            if (ActionsManager.isTriggered(Actions.Engine_LockMouseCursor)) _d3dEngine.MouseCapture = !_d3dEngine.MouseCapture;
            if (ActionsManager.isTriggered(Actions.Engine_FullScreen)) _d3dEngine.isFullScreen = !_d3dEngine.isFullScreen;
        }

        public override void Dispose()
        {
#if DEBUG
            DebugEffect.Dispose();
#endif
            _d3dEngine.GameWindow.Closed -= GameWindow_Closed; //Subscribe to Close event

            GameSystemSettings.Current.Settings.CleanUp();
            base.Dispose();
        }
    }
}
