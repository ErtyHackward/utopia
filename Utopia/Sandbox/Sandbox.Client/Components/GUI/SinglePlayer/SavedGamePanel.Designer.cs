using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using S33M3CoreComponents.GUI.Nuclex.Controls;
using S33M3Resources.Structs;
using S33M3CoreComponents.GUI.Nuclex;
using S33M3CoreComponents.GUI.Nuclex.Controls.Desktop;

namespace Sandbox.Client.Components.GUI.SinglePlayer
{
    public partial class SavedGamePanel
    {
        #region Private variables
        #endregion

        #region Public variable/properties
        protected LabelControl _panelLabel;
        protected ListControl _savedGameList;
        #endregion

        #region Public methods
        private void InitializeComponent()
        {
            CreateComponents();
            Resize();
            BindComponents();
        }
        #endregion

        #region Private methods
        private void CreateComponents()
        {
            _panelLabel = ToDispose(new LabelControl()
            {
                Text = "Saved Games",
                Color = new ByteColor(255, 255, 255),
                CustomFont = _commonResources.FontBebasNeue25
            });

            _savedGameList = ToDispose(new ListControl());
            _savedGameList.IsClickTransparent = false;
            _savedGameList.SelectionMode = ListSelectionMode.Single;
            _savedGameList.Items.Add("Row 1");
            _savedGameList.Items.Add("Row 2");
            _savedGameList.Items.Add("Row 3");
            _savedGameList.Items.Add("Row 4");
        }

        private void BindComponents()
        {
            this.Children.Add(_panelLabel);
            this.Children.Add(_savedGameList);
        }

        public void Resize()
        {
            if (this.Parent != null) this.Bounds = new UniRectangle(0, 0, this.Parent.Bounds.Size.X.Offset, this.Parent.Bounds.Size.Y.Offset);

            float BorderMargin = 15;
            _panelLabel.Bounds = new UniRectangle(BorderMargin, BorderMargin, 0, 0);

            float YPosi = BorderMargin + 35;

            _savedGameList.Bounds = new UniRectangle(BorderMargin, YPosi,  500, 400);
        }
        #endregion
    }
}
