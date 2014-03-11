using System;
using S33M3DXEngine.Main;
using S33M3CoreComponents.GUI.Nuclex;
using S33M3DXEngine;
using SharpDX.Direct3D11;

namespace Realms.Client.Components.GUI
{
    public partial class MenuTemplate1Component : GameComponent
    {
        protected static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        #region Private variables
        protected readonly D3DEngine _engine;
        protected readonly MainScreen _screen;
        protected readonly Game _game;
        #endregion

        #region Public variables/Properties
        public readonly SandboxCommonResources CommonResources;
        #endregion

        public MenuTemplate1Component(Game game, D3DEngine engine, MainScreen screen, SandboxCommonResources commonResources)
        {
            _engine = engine;
            _screen = screen;
            CommonResources = commonResources;
            _game = game;

            _engine.ScreenSize_Updated += UpdateLayoutInternal;
        }

        public override void BeforeDispose()
        {
            if (BackPressed != null)
            {
                //Remove all Events associated to this control (That haven't been unsubscribed !)
                foreach (Delegate d in BackPressed.GetInvocationList())
                {
                    BackPressed -= (EventHandler)d;
                }
            }

            _engine.ScreenSize_Updated -= UpdateLayoutInternal;
        }

        #region Private methods
        #endregion

        #region Public methods
        public override void Initialize()
        {
            InitializeComponentInternal();
        }

        public override void LoadContent(DeviceContext context)
        {
            RefreshComponentsVisibility();
        }

        protected override void OnUpdatableChanged(object sender, EventArgs args)
        {
            if (!IsInitialized) return;

            RefreshComponentsVisibility();

            base.OnUpdatableChanged(sender, args);
        }
        #endregion
    }
}
