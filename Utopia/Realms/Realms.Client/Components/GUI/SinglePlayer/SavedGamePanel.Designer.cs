using System;
using S33M3CoreComponents.GUI.Nuclex.Controls;
using S33M3Resources.Structs;
using S33M3CoreComponents.GUI.Nuclex;
using S33M3CoreComponents.GUI.Nuclex.Controls.Desktop;
using Utopia.Shared.Settings;

namespace Realms.Client.Components.GUI.SinglePlayer
{
    public partial class SavedGamePanel
    {
        #region Private variables
        #endregion

        #region Public variable/properties
        protected LabelControl _panelLabel;
        protected ListControl _savedGameList;
        protected ButtonControl _btLoad;
        protected ButtonControl _btDelete;

        public ButtonControl BtLoad
        {
            get { return _btLoad; }
        }

        public ButtonControl BtDelete
        {
            get { return _btDelete; }
        }

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

            _btLoad = ToDispose(new ButtonControl()
            {
                Text = "Load",
                Enabled = false,
                TextFontId = 1
            });
            _btLoad.Pressed += _btLoad_Pressed;

            _btDelete = ToDispose(new ButtonControl()
            {
                Text = "Delete",
                Enabled = false,
                TextFontId = 1
            });
            _btDelete.Pressed += _btDelete_Pressed;

            _savedGameList = ToDispose(new ListControl());
            _savedGameList.IsClickTransparent = false;
            _savedGameList.SelectionMode = ListSelectionMode.Single;
            _savedGameList.SelectionChanged += _savedGameList_SelectionChanged;

            if (LocalWorlds.LocalWorldsParams != null)
            {
                //Insert the various single world present on the computer
                foreach (LocalWorlds.LocalWorldsParam worldp in LocalWorlds.LocalWorldsParams)
                {
                    _savedGameList.Items.Add(worldp);
                }
            }

            if (_savedGameList.Items.Count > 0)
            {
                _savedGameList.SelectItem(0);
            }
        }

        void _btDelete_Pressed(object sender, EventArgs e)
        {
            if (_savedGameList.SelectedItems.Count == 1)
            {
                DeleteWorld((LocalWorlds.LocalWorldsParam)_savedGameList.SelectedItem);
            }
        }

        void _btLoad_Pressed(object sender, EventArgs e)
        {
            if (_savedGameList.SelectedItems.Count == 1)
            {
                LocalWorlds.LocalWorldsParam info = (LocalWorlds.LocalWorldsParam)_savedGameList.SelectedItem;

                //Assign the choosen World parameter to the Active WorldParameter
                _currentWorldParameter.WorldName = info.WorldParameters.WorldName;
                _currentWorldParameter.SeedName = info.WorldParameters.SeedName;
                _currentWorldParameter.Configuration = info.WorldParameters.Configuration;
            }
        }

        void _savedGameList_SelectionChanged(object sender, EventArgs e)
        {
            if (_savedGameList.SelectedItems.Count == 1 && _savedGameList.SelectedItem is LocalWorlds.LocalWorldsParam)
            {
                _btLoad.Enabled = true;
                _btDelete.Enabled = true;
            }
            else
            {
                _btLoad.Enabled = false;
                _btDelete.Enabled = false;
            }
        }

        private void BindComponents()
        {
            this.Children.Add(_btDelete);
            this.Children.Add(_btLoad);
            this.Children.Add(_panelLabel);
            this.Children.Add(_savedGameList);
        }

        public void Resize()
        {
            if (this.Parent != null) this.Bounds = new UniRectangle(0, 0, this.Parent.Bounds.Size.X.Offset, this.Parent.Bounds.Size.Y.Offset);

            float BorderMargin = 15;
            _panelLabel.Bounds = new UniRectangle(BorderMargin, BorderMargin, 0, 0);

            float YPosi = BorderMargin + 35;

            _savedGameList.Bounds = new UniRectangle(BorderMargin, YPosi,  500, 200);
            YPosi+= 220;
            _btLoad.Bounds = new UniRectangle(BorderMargin, YPosi, 100, 40);
            _btDelete.Bounds = new UniRectangle(BorderMargin + _btLoad.Bounds.Size.X.Offset + 10, YPosi, 100, 40);
        }

        public override void BeforeDispose()
        {
            _btDelete.Pressed -= _btDelete_Pressed;
            _btLoad.Pressed -= _btLoad_Pressed;
            _savedGameList.SelectionChanged -= _savedGameList_SelectionChanged;
        }
        #endregion
    }
}
