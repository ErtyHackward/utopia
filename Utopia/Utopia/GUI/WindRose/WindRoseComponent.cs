using S33M3CoreComponents.GUI.Nuclex;
using S33M3CoreComponents.GUI.Nuclex.Controls.Arcade;
using S33M3DXEngine.Main;
using S33M3Resources.Structs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Utopia.GUI.WindRose
{
    public class WindRoseComponent : GameComponent
    {
        #region Private Variables
        private readonly MainScreen _guiScreen;

        private PanelControl _compassPanel;
        #endregion

        #region Public Properties
        #endregion

        public WindRoseComponent(MainScreen guiScreen)
        {
            _guiScreen = guiScreen;
        }

        public override void Initialize()
        {            
        }

        public override void LoadContent(SharpDX.Direct3D11.DeviceContext context)
        {
            _compassPanel = ToDispose(new PanelControl() { FrameName = "WindRose", Bounds = new UniRectangle(new UniScalar(1.0f, -200), 0, 200, 200)});
            _guiScreen.Desktop.Children.Add(_compassPanel);
        }

        #region Public Methods
        #endregion

        #region Private Methods
        #endregion
    }
}
