using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using S33M3CoreComponents.GUI.Nuclex.Controls;
using S33M3DXEngine;
using SharpDX.Direct3D11;
using S33M3CoreComponents.GUI.Nuclex;
using S33M3CoreComponents.Unsafe;

namespace Sandbox.Client.Components.GUI.Settings
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
            _engine.ViewPort_Updated += engine_ViewPort_Updated;
            this.CanBeRendered = false;
            _panelName = "Key Bindings";
            _parent = parent;
            this.Bounds = bound;

            _engine.GameWindow.KeyDown += new System.Windows.Forms.KeyEventHandler(GameWindow_KeyDown);

            InitializeComponent();
        }

        public override void Dispose()
        {
            _engine.GameWindow.KeyDown -= new System.Windows.Forms.KeyEventHandler(GameWindow_KeyDown);

            base.Dispose();
        }

        void GameWindow_KeyDown(object sender, System.Windows.Forms.KeyEventArgs e)
        {
            //Get selected cell
            bool isLShift, isRShift, isLControl, isRControl;
            string ModifierStr = string.Empty;
            isLShift = (UnsafeNativeMethods.GetKeyState(0xA0) & 0x80) != 0; // VK_LSHIFT    
            isRShift = (UnsafeNativeMethods.GetKeyState(0xA1) & 0x80) != 0; // VK_RSHIFT        
            isLControl = (UnsafeNativeMethods.GetKeyState(162) & 0x80) != 0; // VK_LCONTROL              
            isRControl = (UnsafeNativeMethods.GetKeyState(0xA3) & 0x80) != 0; // VK_RCONTROL              

            if (isLShift) ModifierStr += ModifierStr.Length > 0 ? "+ LShiftKey" : "LShiftKey";
            if (isRShift) ModifierStr += ModifierStr.Length > 0 ? "+ RShiftKey" : "RShiftKey";
            if (isLControl) ModifierStr += ModifierStr.Length > 0 ? "+ LControlKey" : "LControlKey";
            if (isRControl) ModifierStr += ModifierStr.Length > 0 ? "+ RControlKey" : "RControlKey";

            string Keypressed = e.KeyData.ToString();

            //If it is only a Modifier key pressed
            if (Keypressed.Contains("Key"))
            {
                Console.WriteLine(ModifierStr);
            }
            else
            {
                Keypressed = Keypressed.Split(',')[0];
                if (ModifierStr.Length > 0) ModifierStr = " + " + ModifierStr;
                Console.WriteLine(Keypressed + ModifierStr);
            }
        }

        void engine_ViewPort_Updated(Viewport viewport, Texture2DDescription newBackBuffer)
        {
            Resize();
        }

        #region Public Methods
        #endregion

        #region Private Methods
        #endregion
    }
}
