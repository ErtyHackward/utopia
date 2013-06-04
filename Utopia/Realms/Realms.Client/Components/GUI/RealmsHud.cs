using System;
using Ninject;
using Realms.Client.Components.GUI.Settings;
using S33M3CoreComponents.Cameras;
using S33M3CoreComponents.Cameras.Interfaces;
using S33M3CoreComponents.GUI.Nuclex;
using S33M3CoreComponents.GUI.Nuclex.Controls;
using S33M3CoreComponents.Inputs;
using S33M3CoreComponents.Sprites2D;
using S33M3DXEngine;
using Utopia.GUI;
using Utopia.GUI.Inventory;
using Utopia.Shared.Settings;

namespace Realms.Client.Components.GUI
{
    public class RealmsHud : Hud
    {
        private readonly MainScreen _screen;
        private readonly D3DEngine _d3DEngine;

        [Inject]
        public SettingsComponent SettignsComponent
        {
            set
            {
                value.KeyBindingChanged += delegate { UpdateLabels(); };
            }
        }

        public RealmsHud(MainScreen screen, D3DEngine d3DEngine, ToolBarUi toolbar, InputsManager inputManager, CameraManager<ICameraFocused> camManager) : 
            base(screen, d3DEngine, toolbar, inputManager, camManager)
        {
            _screen = screen;
            _d3DEngine = d3DEngine;

            _d3DEngine.ViewPort_Updated += UpdateLayout;
        }

        private void UpdateLabels()
        {

        }

        public override void Initialize()
        {



            UpdateLabels();

            UpdateLayout(_d3DEngine.ViewPort, _d3DEngine.BackBufferTex.Description);
            
            base.Initialize();
        }

        public override void EnableComponent(bool forced)
        {


            base.EnableComponent(forced);
        }

        public override void DisableComponent()
        {


            base.DisableComponent();
        }

        void UpdateLayout(SharpDX.ViewportF viewport, SharpDX.Direct3D11.Texture2DDescription newBackBuffer)
        {

        }
    }
}
