using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using S33M3CoreComponents.GUI.Nuclex.Controls;
using S33M3Resources.Structs;
using S33M3CoreComponents.GUI.Nuclex.Visuals.Flat;
using S33M3CoreComponents.GUI.Nuclex;
using SharpDX.Direct3D11;
using Utopia.Settings;
using System.Reflection;
using S33M3CoreComponents.GUI.Nuclex.Controls.Desktop;
using S33M3CoreComponents.Tools;

namespace Sandbox.Client.Components.GUI.Settings
{
    partial class GraphicSettingsPanel
    {
        private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        #region Private Variables
        protected LabelControl _panelLabel;

        protected List<ParamRow> Parameters;
        #endregion

        #region Public Variables
        #endregion

        private void InitializeComponent()
        {
            CreateComponents();
            Resize();
            BindComponents();
        }

        #region Public Methods
        #endregion

        #region Private Methods
        private void CreateComponents()
        {
            _panelLabel = new LabelControl()
            {
                Text = "Graphical settings",
                Color = new ByteColor(255, 255, 255),
                CustomFont = SandboxMenuComponent.FontBebasNeue25
            };

            Parameters = new List<ParamRow>(Settings2Components.CreateComponentsRows(ClientSettings.Current.Settings.GraphicalParameters));            
        }

        private void BindComponents()
        {
            this.Children.Add(_panelLabel);
            foreach (ParamRow row in Parameters)
            {
                this.Children.Add(row.ParamName);
                this.Children.Add(ToDispose(row.InputingComp));
            }
        }

        public void Resize()
        {
            if(this.Parent != null) this.Bounds = new UniRectangle(0,0,this.Parent.Bounds.Size.X.Offset,this.Parent.Bounds.Size.Y.Offset);

            float BorderMargin = 15;
            _panelLabel.Bounds = new UniRectangle(BorderMargin, BorderMargin, 0, 0);

            float lineHeight = 50;

            foreach (ParamRow row in Parameters)
            {
                row.ParamName.Bounds = new UniRectangle(BorderMargin, lineHeight, 1 , 0);
                row.InputingComp.Bounds = new UniRectangle(BorderMargin + 200, lineHeight - 5, 50, 25);
                lineHeight += 30;
            }
        }
        #endregion
    }
}
