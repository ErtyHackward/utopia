using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using S33M3CoreComponents.GUI.Nuclex.Controls;
using S33M3Resources.Structs;
using S33M3CoreComponents.GUI.Nuclex;
using S33M3CoreComponents.GUI.Nuclex.Controls.Desktop;
using S33M3CoreComponents.Inputs.KeyboardHandler;
using System.Reflection;
using Utopia.Settings;

namespace Sandbox.Client.Components.GUI.Settings
{
    public partial class KeyBindingSettingsPanel
    {
        public class KeyBindComponent
        {
            public LabelControl Name;
            public InputKeyCatchControl input;
            public KeyWithModifier Key;

            public KeyBindComponent(string name, KeyWithModifier key)
            {
                Key = key;
                Name = new LabelControl()
                {
                    Text = name,
                    Color = new ByteColor(255, 255, 255),
                    FontStyle = System.Drawing.FontStyle.Bold
                };
                input = new InputKeyCatchControl() { Text = key.Modifier == System.Windows.Forms.Keys.None ? key.MainKey.ToString() : key.MainKey.ToString() + " + " + key.Modifier.ToString() };
            }
        }

        #region Private Variables
        private LabelControl _panelLabel;
        private LabelControl _moveSection, _gameSection, _systemSection;
        private List<KeyBindComponent> _moveKeys = new List<KeyBindComponent>();
        private List<KeyBindComponent> _gameKeys = new List<KeyBindComponent>();
        private List<KeyBindComponent> _systemKeys = new List<KeyBindComponent>();
        #endregion

        #region Public Variables
        public List<KeyBindComponent> SystemKeys
        {
            get { return _systemKeys; }
            set { _systemKeys = value; }
        }

        public List<KeyBindComponent> GameKeys
        {
            get { return _gameKeys; }
            set { _gameKeys = value; }
        }

        public List<KeyBindComponent> MoveKeys
        {
            get { return _moveKeys; }
            set { _moveKeys = value; }
        }
        #endregion

        #region Public Methods
        #endregion

        #region Private Methods
        private void InitializeComponent()
        {
            CreateComponents();
            Resize();
            BindComponents();
        }

        private void CreateComponents()
        {
            _panelLabel = new LabelControl()
            {
                Text = _panelName,
                Color = new ByteColor(255,255,255),
                CustomFont = SandboxMenuComponent.FontBebasNeue25
            };

            //Create the Game Component section
            CreateMoveSection();
            CreateGameSection();
            CreateSystemSection();
        }

        private void CreateMoveSection()
        {
            _moveSection = new LabelControl()
            {
                Text = "Move Bindings",
                Color = new ByteColor(146, 205, 67),
                CustomFont = SandboxMenuComponent.FontBebasNeue17
            };

            //Create a line per graphical components
            Type reflectedType;
            FieldInfo[] pi;

            reflectedType = ClientSettings.Current.Settings.KeyboardMapping.Move.GetType();
            pi = reflectedType.GetFields();
            foreach (FieldInfo field in pi)
            {
                KeyBindComponent binding = new KeyBindComponent(field.Name,(KeyWithModifier) field.GetValue(ClientSettings.Current.Settings.KeyboardMapping.Move));
                binding.input.KeyChanged += input_KeyChanged;
                _moveKeys.Add(binding);
            }
        }


        private void CreateGameSection()
        {
            _gameSection = new LabelControl()
            {
                Text = "Game Bindings",
                Color = new ByteColor(146, 205, 67),
                CustomFont = SandboxMenuComponent.FontBebasNeue17
            };

            //Create a line per graphical components
            Type reflectedType;
            FieldInfo[] pi;

            reflectedType = ClientSettings.Current.Settings.KeyboardMapping.Game.GetType();
            pi = reflectedType.GetFields();
            foreach (FieldInfo field in pi)
            {
                KeyBindComponent binding = new KeyBindComponent(field.Name, (KeyWithModifier)field.GetValue(ClientSettings.Current.Settings.KeyboardMapping.Game));
                binding.input.KeyChanged += input_KeyChanged;
                _gameKeys.Add(binding);
            }
        }

        private void CreateSystemSection()
        {
            _systemSection = new LabelControl()
            {
                Text = "System Bindings",
                Color = new ByteColor(146, 205, 67),
                CustomFont = SandboxMenuComponent.FontBebasNeue17
            };

            //Create a line per graphical components
            Type reflectedType;
            FieldInfo[] pi;

            reflectedType = ClientSettings.Current.Settings.KeyboardMapping.System.GetType();
            pi = reflectedType.GetFields();
            foreach (FieldInfo field in pi)
            {
                KeyBindComponent binding = new KeyBindComponent(field.Name, (KeyWithModifier)field.GetValue(ClientSettings.Current.Settings.KeyboardMapping.System));
                binding.input.KeyChanged += input_KeyChanged;
                _systemKeys.Add(binding);
            }
        }

        private void input_KeyChanged(object sender, EventArgs e)
        {
            _parent.SaveChange();
        }

        private void BindComponents()
        {
            this.Children.Add(_panelLabel);
            this.Children.Add(_moveSection);
            foreach (var item in _moveKeys)
            {
                this.Children.Add(item.Name);
                this.Children.Add(item.input);
            }

            this.Children.Add(_gameSection);
            foreach (var item in _gameKeys)
            {
                this.Children.Add(item.Name);
                this.Children.Add(item.input);
            }

            this.Children.Add(_systemSection);
            foreach (var item in _systemKeys)
            {
                this.Children.Add(item.Name);
                this.Children.Add(item.input);
            }
        }

        public void Resize()
        {
            if (this.Parent != null)
            {
                this.Bounds = new UniRectangle(0, 0, this.Parent.Bounds.Size.X.Offset, this.Parent.Bounds.Size.Y.Offset);
            }

            float BorderMargin = 15;
            _panelLabel.Bounds = new UniRectangle(BorderMargin, BorderMargin, 0, 0);

            //Move Section
            float yPos = BorderMargin + 30;
            _moveSection.Bounds = new UniRectangle(BorderMargin, yPos, 0, 0);
            int AccumulatedXSize = (int)BorderMargin;
            yPos += 30;
            foreach (var item in _moveKeys)
            {
                if (AccumulatedXSize + (int)(BorderMargin + 180) >= this.Bounds.Size.X.Offset)
                {
                    AccumulatedXSize = (int)BorderMargin;
                    yPos += 30;
                }
                item.Name.Bounds = new UniRectangle(AccumulatedXSize, yPos, 0, 0);
                item.input.Bounds = new UniRectangle(AccumulatedXSize + 100, yPos -10, 80, 20);

                AccumulatedXSize += (int)(BorderMargin + 180);
            }

            //Game Section
            yPos += 30;
            _gameSection.Bounds = new UniRectangle(BorderMargin, yPos, 0, 0);
            AccumulatedXSize = (int)BorderMargin;
            yPos += 30;
            foreach (var item in _gameKeys)
            {
                if (AccumulatedXSize + (int)(BorderMargin + 180) >= this.Bounds.Size.X.Offset)
                {
                    AccumulatedXSize = (int)BorderMargin;
                    yPos += 30;
                }
                item.Name.Bounds = new UniRectangle(AccumulatedXSize, yPos, 0, 0);
                item.input.Bounds = new UniRectangle(AccumulatedXSize + 100, yPos - 10, 80, 20);

                AccumulatedXSize += (int)(BorderMargin + 180);
            }

            //System Section
            yPos += 30;
            _systemSection.Bounds = new UniRectangle(BorderMargin, yPos, 0, 0);
            AccumulatedXSize = (int)BorderMargin;
            yPos += 30;
            foreach (var item in _systemKeys)
            {
                if (AccumulatedXSize + (int)(BorderMargin + 180) >= this.Bounds.Size.X.Offset)
                {
                    AccumulatedXSize = (int)BorderMargin;
                    yPos += 30;
                }
                item.Name.Bounds = new UniRectangle(AccumulatedXSize, yPos, 0, 0);
                item.input.Bounds = new UniRectangle(AccumulatedXSize + 100, yPos - 10, 80, 20);

                AccumulatedXSize += (int)(BorderMargin + 180);
            }

        }
        #endregion

    }
}
