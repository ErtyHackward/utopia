using System;
using S33M3CoreComponents.GUI.Nuclex.Controls;
using S33M3Resources.Structs;
using S33M3CoreComponents.GUI.Nuclex;
using S33M3CoreComponents.GUI.Nuclex.Controls.Desktop;
using SharpDX;
using System.IO;
using Utopia.Shared.Settings;
using System.Collections.Generic;
using System.Linq;
using Utopia.Shared.Configuration;

namespace Realms.Client.Components.GUI.SinglePlayer
{
    public partial class NewGamePanel
    {
        private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        #region Private variables
        #endregion

        #region Public variable/properties
        protected LabelControl _panelLabel;
        protected LabelControl _inputWorldNameLabel;
        protected InputControl _inputWorldName;
        protected LabelControl _inputSeedNameLabel;
        protected InputControl _inputSeedName;
        protected LabelControl _configurationsFilesLabel;
        protected ListControl _configurationsFiles;
        protected ButtonControl _btCreate;

        public ButtonControl BtCreate
        {
            get { return _btCreate; }
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
                Text = "New Game",
                Color = new ByteColor(255, 255, 255),
                CustomFont = _commonResources.FontBebasNeue25
            });

            _inputWorldName = ToDispose(new InputControl()
            {
                //CustomFont = _commonResources.FontBebasNeue17,
                Color = SharpDX.Color.Black
            });

            _inputSeedName = ToDispose(new InputControl()
            {
                //CustomFont = _commonResources.FontBebasNeue17,
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

            _configurationsFilesLabel = ToDispose(new LabelControl()
            {
                Text = "Configurations : ",
                Color = new ByteColor(255, 255, 255),
                CustomFont = _commonResources.FontBebasNeue17
            });

            _configurationsFiles = ToDispose(new ListControl());
            _configurationsFiles.IsClickTransparent = false;
            _configurationsFiles.SelectionMode = ListSelectionMode.Single;
            _configurationsFiles.SelectionChanged += _configurationsFiles_SelectionChanged;

            foreach (var configurationFile in GetConfigurationsList())
            {
                _configurationsFiles.Items.Add(configurationFile);
            }
            _configurationsFiles.SelectItem(0);

            _btCreate = ToDispose(new ButtonControl()
            {
                Text = "Create",
                TextFontId = 1
            });
            BtCreate.Pressed += BtCreate_Pressed;

        }

        void _configurationsFiles_SelectionChanged(object sender, EventArgs e)
        {
        }

        private IEnumerable<string> GetConfigurationsList()
        {
            var path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "Utopia");

            if (!Directory.Exists(path)) Directory.CreateDirectory(path);

            //Yield Default Configurations files
            foreach (var defaultFile in Directory.GetFiles("Config", "*.realm"))
            {
                yield return "System config : " +  Path.GetFileNameWithoutExtension(defaultFile);
            }

            foreach (var file in Directory.GetFiles(path).Where(x => x.EndsWith(".realm")))
            {
                yield return Path.GetFileNameWithoutExtension(file);
            }
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
                    //Check if this world is not existing
                    if (Directory.Exists(Path.Combine(LocalWorlds.GetSinglePlayerServerRootPath(_vars.ApplicationDataPath), _inputWorldName.Text)) == false)
                    {
                        //Assign to currentWorldParameters the news parameters
                        _currentWorldParameter.WorldName = _inputWorldName.Text;
                        _currentWorldParameter.SeedName = _inputSeedName.Text;
                        _currentWorldParameter.Configuration = GetConfigurationObject();

                        //Reset field values
                        _inputWorldName.Text = string.Empty;
                        _inputSeedName.Text = string.Empty;
                    }
                    else
                    {
                        _guiManager.MessageBox("World's forlder exists. Select different name.", "Error");
                        return;
                    }
                }
                catch (Exception error)
                {
                    logger.Error("Error while loading the configuration file : {0}", error);
                    _guiManager.MessageBox("Unable to load the configuration: " + error.Message);
                    return;
                }
            }

            if (_currentWorldParameter.WorldName == null)
            {
                _guiManager.MessageBox("World parameter(s) incorrect", "Error");
            }

        }

        private WorldConfiguration GetConfigurationObject()
        {
            WorldConfiguration config = null;
            if (_configurationsFiles.SelectedItem.ToString().StartsWith("System config : "))
            {
                //Create new default RealmConfiguration
                var path = Directory.GetFiles("Config", _configurationsFiles.SelectedItem.ToString().Replace("System config : ", "") + ".realm")[0];

                config = WorldConfiguration.LoadFromFile(path);
            }
            else
            {
                var path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "Utopia", _configurationsFiles.SelectedItem.ToString() + ".realm");
                if (File.Exists(path))
                {
                    config = WorldConfiguration.LoadFromFile(path);
                }
                else
                {
                    _guiManager.MessageBox("Configuration file missing !", "Error");
                }
            }

            return config;
        }

        private void BindComponents()
        {
            this.Children.Add(BtCreate);
            this.Children.Add(_inputSeedName);
            this.Children.Add(_inputSeedNameLabel);
            this.Children.Add(_inputWorldName);
            this.Children.Add(_inputWorldNameLabel);
            this.Children.Add(_panelLabel);
            this.Children.Add(_configurationsFiles);
            this.Children.Add(_configurationsFilesLabel);
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

            _configurationsFilesLabel.Bounds = new UniRectangle(BorderMargin, Yposi + 5, 130, 0);
            _configurationsFiles.Bounds = new UniRectangle(_configurationsFilesLabel.Bounds.Location.X.Offset + _configurationsFilesLabel.Bounds.Size.X.Offset + 10, Yposi, 400, 200);

            Yposi += 240;
            BtCreate.Bounds = new UniRectangle(BorderMargin, Yposi, 100, 40);
        }
        #endregion
    }
}
