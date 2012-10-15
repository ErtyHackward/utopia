﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using S33M3CoreComponents.GUI.Nuclex.Controls;
using S33M3Resources.Structs;
using S33M3CoreComponents.GUI.Nuclex;
using S33M3CoreComponents.GUI.Nuclex.Controls.Desktop;
using SharpDX;
using System.IO;
using System.Diagnostics;
using Utopia.Shared.Settings;

namespace Sandbox.Client.Components.GUI.SinglePlayer
{
    public partial class NewGamePanel
    {
        #region Private variables
        #endregion

        #region Public variable/properties
        protected LabelControl _panelLabel;
        protected LabelControl _inputWorldNameLabel;
        protected InputControl _inputWorldName;
        protected LabelControl _inputSeedNameLabel;
        protected InputControl _inputSeedName;
        public ButtonControl BtCreate;
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
                Text = "New Game",
                Color = new ByteColor(255, 255, 255),
                CustomFont = _commonResources.FontBebasNeue25
            });

            _inputWorldName = ToDispose(new InputControl()
            {
                CustomFont = _commonResources.FontBebasNeue17,
                Color = SharpDX.Color.Black
            });

            _inputSeedName = ToDispose(new InputControl()
            {
                CustomFont = _commonResources.FontBebasNeue17,
                Color = SharpDX.Color.Black
            });

            _inputWorldNameLabel = ToDispose(new LabelControl()
            {
                Text = "World Name : ",
                Color = new ByteColor(255, 255, 255),
                CustomFont = _commonResources.FontBebasNeue17
            });

            _inputSeedNameLabel = ToDispose(new LabelControl()
            {
                Text = "Seed Name : ",
                Color = new ByteColor(255, 255, 255),
                CustomFont = _commonResources.FontBebasNeue17
            });

            BtCreate = ToDispose(new ButtonControl()
            {
                Text = "Create",
                TextFontId = 1
            });
            BtCreate.Pressed += BtCreate_Pressed;
        }

        //[DebuggerStepThrough]
        private void BtCreate_Pressed(object sender, EventArgs e)
        {
            _currentWorldParameter.Clear();
            //Do parameters validation check.
            if ((string.IsNullOrEmpty(_inputWorldName.Text) ||
                string.IsNullOrWhiteSpace(_inputWorldName.Text) ||
                string.IsNullOrEmpty(_inputSeedName.Text) ||
                string.IsNullOrEmpty(_inputSeedName.Text)) == false)
            {
                //Validate the Name as Directory
                try
                {
                    //Will fall in error if not right correct file name
                    var result = Path.GetFullPath(@"c:\" + _inputWorldName.Text);

                    //Check if this world is not existing
                    if (Directory.Exists(Path.Combine(LocalWorlds.GetSinglePlayerServerRootPath(_vars.ApplicationDataPath), _inputWorldName.Text)) == false)
                    {
                        //Assign to currentWorldParameters the news parameters
                        _currentWorldParameter.WorldName = _inputWorldName.Text;
                        _currentWorldParameter.SeedName = _inputSeedName.Text;

                        //Reset field values
                        _inputWorldName.Text = string.Empty;
                        _inputSeedName.Text = string.Empty;
                    }
                }
                catch (Exception)
                {
                }
            }

            if (_currentWorldParameter.WorldName == null)
            {
                _guiManager.MessageBox("World parameter(s) incorrect", "Error");
            }

        }

        private void BindComponents()
        {
            this.Children.Add(BtCreate);
            this.Children.Add(_inputSeedName);
            this.Children.Add(_inputSeedNameLabel);
            this.Children.Add(_inputWorldName);
            this.Children.Add(_inputWorldNameLabel);
            this.Children.Add(_panelLabel);
        }

        public void Resize()
        {
            if (this.Parent != null) this.Bounds = new UniRectangle(0, 0, this.Parent.Bounds.Size.X.Offset, this.Parent.Bounds.Size.Y.Offset);

            float BorderMargin = 15;
            _panelLabel.Bounds = new UniRectangle(BorderMargin, BorderMargin, 0, 0);

            float Yposi = BorderMargin + 40;

            _inputWorldNameLabel.Bounds = new UniRectangle(BorderMargin, Yposi + 5, 130, 0);
            _inputWorldName.Bounds = new UniRectangle(_inputWorldNameLabel.Bounds.Location.X.Offset + _inputWorldNameLabel.Bounds.Size.X.Offset + 10, Yposi, 300, 30);

            Yposi+= 40;

            _inputSeedNameLabel.Bounds = new UniRectangle(BorderMargin, Yposi + 5, 130, 0);
            _inputSeedName.Bounds = new UniRectangle(_inputSeedNameLabel.Bounds.Location.X.Offset + _inputSeedNameLabel.Bounds.Size.X.Offset + 10, Yposi, 300, 30);

            Yposi += 40;

            BtCreate.Bounds = new UniRectangle(BorderMargin, Yposi, 80, 30);
        }
        #endregion
    }
}
