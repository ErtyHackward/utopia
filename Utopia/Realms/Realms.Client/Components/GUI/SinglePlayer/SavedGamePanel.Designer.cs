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
        public ButtonControl BtLoad;
        protected ButtonControl _btDelete;
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

            BtLoad = ToDispose(new ButtonControl()
            {
                Text = "Load",
                Enabled = false,
                TextFontId = 1
            });
            BtLoad.Pressed += _btLoad_Pressed;

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

            if (GameSystemSettings.LocalWorldsParams != null)
            {
                //Insert the various single world present on the computer
                foreach (LocalWorlds.LocalWorldsParam worldp in GameSystemSettings.LocalWorldsParams)
                {
                    _savedGameList.Items.Add(worldp);
                }
            }

            if (_savedGameList.Items.Count > 0)
            {
                _savedGameList.SelectItem = 0;
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
                BtLoad.Enabled = true;
                _btDelete.Enabled = true;
            }
            else
            {
                BtLoad.Enabled = false;
                _btDelete.Enabled = false;
            }
        }

        private void BindComponents()
        {
            this.Children.Add(_btDelete);
            this.Children.Add(BtLoad);
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
            BtLoad.Bounds = new UniRectangle(BorderMargin, YPosi, 70, 30);
            _btDelete.Bounds = new UniRectangle(BorderMargin + BtLoad.Bounds.Size.X.Offset + 10, YPosi, 70, 30);
        }

        public override void BeforeDispose()
        {
            _btDelete.Pressed -= _btDelete_Pressed;
            BtLoad.Pressed -= _btLoad_Pressed;
            _savedGameList.SelectionChanged -= _savedGameList_SelectionChanged;
        }
        #endregion
    }
}
