using S33M3CoreComponents.GUI.Nuclex.Controls;
using S33M3DXEngine;
using SharpDX.Direct3D11;
using S33M3CoreComponents.GUI.Nuclex;
using SharpDX;

namespace Realms.Client.Components.GUI.Settings
{
    public partial class KeyBindingSettingsPanel : Control
    {
        #region Private Variables
        private SettingsComponent _parent;
        private string _panelName;
        private D3DEngine _engine;
        #endregion

        #region Public Variables
        #endregion

        public KeyBindingSettingsPanel(SettingsComponent parent, D3DEngine engine, UniRectangle bound)
        {
            _engine = engine;
            _engine.ScreenSize_Updated += engine_ScreenSize_Updated;
            _panelName = "Key Bindings";
            _parent = parent;
            this.Bounds = bound;
            this.IsRendable = false;
            
            InitializeComponent();
        }

        void engine_ScreenSize_Updated(ViewportF viewport, Texture2DDescription newBackBuffer)
        {
            Resize();
        }

        #region Public Methods
        #endregion

        #region Private Methods
        #endregion
    }
}
