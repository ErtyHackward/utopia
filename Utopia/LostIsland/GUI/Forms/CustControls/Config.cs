using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Utopia.Settings;
using System.Reflection;
using Utopia.Shared.Config;
          
namespace LostIslandClient.GUI.Forms.CustControls
{
    public partial class Config : UserControl
    {
        //Binding lists that will by binded to the datagrid. They will contains the data that needs to be displayed from the Global static ClientSettings.ClientConfig
        private BindingList<SettingsBindingItem> _settingBindingGame;
        private BindingList<SettingsBindingItem> _settingBindingGraphic;
        private BindingList<KeyBoardBindingItemWithInfo> _KeybBindingItems;
        private BindingList<KeyBoardBindingItem> _KeybBindingItemsMove;

        public Config()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Create all the bindings lists from the Config data settings for datagrid binding
        /// </summary>
        /// <param name="Config">The Config parameters to display</param>
        public void RefreshAllBindingLists(ClientConfig Config)
        {
            RefreshGameSettingsBindingList(Config);
            RefreshGraphicSettingsBindingList(Config);
            RefreshKeyboardSettingsBindingList(Config);
        }

        /// <summary>
        /// Create the binding list for the game Setting from the Config data settings for datagrid binding
        /// </summary>
        /// <param name="Config">The Config parameters to display</param>
        private void RefreshGameSettingsBindingList(ClientConfig Config)
        {
            object value;
            Type reflectedType;
            PropertyInfo[] pi;
            //GAME SETTINGS BINDING =====================================================
            _settingBindingGame = new BindingList<SettingsBindingItem>();

            reflectedType = Config.GameParameters.GetType();
            pi = reflectedType.GetProperties();
            foreach (PropertyInfo field in pi)
            {
                value = field.GetValue(Config.GameParameters, null);
                _settingBindingGame.Add(new SettingsBindingItem() { Name = field.Name, Value = value });
            }
            dtGridGameParam.DataSource = _settingBindingGame;
            dtGridGameParam.Columns["Name"].ReadOnly = true;
        }

        /// <summary>
        /// Create the binding list for the parameter Setting from the Config data settings for datagrid binding
        /// </summary>
        /// <param name="Config">The Config parameters to display</param>
        private void RefreshGraphicSettingsBindingList(ClientConfig Config)
        {
            Type reflectedType;
            PropertyInfo[] pi;
            object value;

            _settingBindingGraphic = new BindingList<SettingsBindingItem>();

            reflectedType = Config.GraphicalParameters.GetType();
            pi = reflectedType.GetProperties();
            foreach (PropertyInfo field in pi)
            {
                value = field.GetValue(Config.GraphicalParameters, null);
                _settingBindingGraphic.Add(new SettingsBindingItem() { Name = field.Name, Value = value });
            }
            dtGridGraphParam.DataSource = _settingBindingGraphic;
            dtGridGraphParam.Columns["Name"].ReadOnly = true;
        }

        /// <summary>
        /// Create the binding list for the keyboard mapping setting from the Config data settings for datagrid binding
        /// </summary>
        /// <param name="Config">The Config parameters to display</param>
        private void RefreshKeyboardSettingsBindingList(ClientConfig Config)
        {
            Type reflectedType;
            FieldInfo[] fi;
            KeyWithModifier kwm;

            _KeybBindingItems = new BindingList<KeyBoardBindingItemWithInfo>();
            //The Various parameters
            reflectedType = Config.KeyboardMapping.GetType();
            fi = reflectedType.GetFields();
            foreach (FieldInfo field in fi)
            {
                kwm = ((KeyWithModifier)field.GetValue(Config.KeyboardMapping));
                _KeybBindingItems.Add(new KeyBoardBindingItemWithInfo() { Name = field.Name, Info = kwm.Info, Binding = kwm.MainKey.ToString() + (kwm.Modifier != Keys.None ? " + " + kwm.Modifier.ToString() : string.Empty) });
            }
            dtGridKeyboard.DataSource = _KeybBindingItems;
            dtGridKeyboard.Columns["Name"].ReadOnly = true;

            //The Move parameters
            _KeybBindingItemsMove = new BindingList<KeyBoardBindingItem>();
            reflectedType = Config.KeyboardMapping.Move.GetType();
            fi = reflectedType.GetFields();
            foreach (FieldInfo field in fi)
            {
                kwm = ((KeyWithModifier)field.GetValue(Config.KeyboardMapping.Move));
                _KeybBindingItemsMove.Add(new KeyBoardBindingItem() { Name = field.Name, Binding = kwm.MainKey.ToString() + (kwm.Modifier != Keys.None ? " + " + kwm.Modifier.ToString() : string.Empty) });
            }
            dtGridKeyboardMove.DataSource = _KeybBindingItemsMove;
            dtGridKeyboardMove.Columns["Name"].ReadOnly = true;
        }

        /// <summary>
        /// Save the BindingList collections value into the ClientSettings.Current class
        /// </summary>
        private void FromBindingListToSettings()
        {
            KeyWithModifier kwm;
            Type reflectedType = ClientSettings.Current.Settings.KeyboardMapping.GetType();
            FieldInfo[] fi;
            PropertyInfo[] pi;
            PropertyInfo piTmp;
            //GAME SETTINGS BINDING =====================================================
            reflectedType = ClientSettings.Current.Settings.GameParameters.GetType();
            pi = reflectedType.GetProperties();
            foreach (var item in _settingBindingGame)
            {
                piTmp = pi.First(x => x.Name == item.Name);

                if (item.Value.GetType() != piTmp.PropertyType)
                {
                    var castedValue = piTmp.PropertyType.InvokeMember("Parse", BindingFlags.InvokeMethod, null, piTmp.PropertyType, new object[] { item.Value });
                    piTmp.SetValue(ClientSettings.Current.Settings.GameParameters, castedValue, null);
                }
                else
                {
                    piTmp.SetValue(ClientSettings.Current.Settings.GameParameters, item.Value, null);
                }
            }

            //GRAPHICAL SETTINGS BINDING ================================================
            reflectedType = ClientSettings.Current.Settings.GraphicalParameters.GetType();
            pi = reflectedType.GetProperties();
            foreach (var item in _settingBindingGraphic)
            {
                piTmp = pi.First(x => x.Name == item.Name);
                if (item.Value.GetType() != piTmp.PropertyType)
                {
                    var castedValue = piTmp.PropertyType.InvokeMember("Parse", BindingFlags.InvokeMethod, null, piTmp.PropertyType, new object[] { item.Value });
                    piTmp.SetValue(ClientSettings.Current.Settings.GraphicalParameters, castedValue, null);
                }
                else
                {
                    piTmp.SetValue(ClientSettings.Current.Settings.GraphicalParameters, item.Value, null);
                }
            }

            //KEYBOARD BINDING ==========================================================
            reflectedType = ClientSettings.Current.Settings.KeyboardMapping.GetType();
            fi = reflectedType.GetFields();
            string[] Bindings;
            foreach (var item in _KeybBindingItems)
            {
                Bindings = item.Binding.Split(new string[] { " + " }, StringSplitOptions.RemoveEmptyEntries);
                kwm = new KeyWithModifier();
                kwm.MainKey = (Keys)Enum.Parse(typeof(Keys), Bindings[0]);
                kwm.Info = item.Info;
                if (Bindings.Length > 1)
                {
                    kwm.Modifier = (Keys)Enum.Parse(typeof(Keys), Bindings[1]);
                }
                fi.First(x => x.Name == item.Name).SetValue(ClientSettings.Current.Settings.KeyboardMapping, kwm); 
            }

            reflectedType = ClientSettings.Current.Settings.KeyboardMapping.Move.GetType();
            fi = reflectedType.GetFields();
            foreach (var item in _KeybBindingItemsMove)
            {
                Bindings = item.Binding.Split(new string[] { " + " }, StringSplitOptions.RemoveEmptyEntries);
                kwm = new KeyWithModifier();
                kwm.MainKey = (Keys)Enum.Parse(typeof(Keys), Bindings[0]);
                if (Bindings.Length > 1)
                {
                    kwm.Modifier = (Keys)Enum.Parse(typeof(Keys), Bindings[1]);
                }
                fi.First(x => x.Name == item.Name).SetValue(ClientSettings.Current.Settings.KeyboardMapping.Move, kwm);
            }

        }


        #region Form Event Managements 

        /// <summary>
        /// Handle Specific Key down for Keyboard mapping
        /// </summary>
        private void dtGridKeyboard_KeyDown(object sender, KeyEventArgs e)
        {
            //Get selected cell
            bool isLShift, isRShift, isLControl, isRControl;
            string ModifierStr = string.Empty;
            isLShift = (S33M3Engines.Windows.UnsafeNativeMethods.GetKeyState(0xA0) & 0x80) != 0; // VK_LSHIFT    
            isRShift = (S33M3Engines.Windows.UnsafeNativeMethods.GetKeyState(0xA1) & 0x80) != 0; // VK_RSHIFT        
            isLControl = (S33M3Engines.Windows.UnsafeNativeMethods.GetKeyState(162) & 0x80) != 0; // VK_LCONTROL              
            isRControl = (S33M3Engines.Windows.UnsafeNativeMethods.GetKeyState(0xA3) & 0x80) != 0; // VK_RCONTROL              

            if (isLShift) ModifierStr += ModifierStr.Length > 0 ? "+ LShiftKey" : "LShiftKey";
            if (isRShift) ModifierStr += ModifierStr.Length > 0 ? "+ RShiftKey" : "RShiftKey";
            if (isLControl) ModifierStr += ModifierStr.Length > 0 ? "+ LControlKey" : "LControlKey";
            if (isRControl) ModifierStr += ModifierStr.Length > 0 ? "+ RControlKey" : "RControlKey";

            string Keypressed = e.KeyData.ToString();

            //If it is only a Modifier key pressed
            if (Keypressed.Contains("Key"))
            {
                ((DataGridView)sender).SelectedCells[0].Value = ModifierStr;
            }
            else
            {
                Keypressed = Keypressed.Split(',')[0];
                if (ModifierStr.Length > 0) ModifierStr = " + " + ModifierStr;
                ((DataGridView)sender).SelectedCells[0].Value = Keypressed + ModifierStr;
            }

            if (e.KeyCode == Keys.Tab) e.Handled = true;
        }

        /// <summary>
        /// Request to load the default Qwerty keyboard configuration
        /// </summary>
        private void btDefaultQWERTY_Click(object sender, EventArgs e)
        {
            RefreshKeyboardSettingsBindingList(ClientConfig.DefaultQwerty);
        }

        /// <summary>
        /// Request to load the default Azerty keyboard configuration
        /// </summary>
        private void btDefaultAZERTY_Click(object sender, EventArgs e)
        {
            RefreshKeyboardSettingsBindingList(ClientConfig.DefaultAzerty);
        }

        /// <summary>
        /// Close the Configuration screen without saving
        /// </summary>
        private void btCancel_Click(object sender, EventArgs e)
        {
            ((Panel)this.Parent).BackColor = Color.FromArgb(0, 0, 0, 0);
            ((Panel)this.Parent).Controls.Clear();
        }

        /// <summary>
        /// Save the changes to Global static settings class + save into the xml file
        /// </summary>
        private void btSaveConfig_Click(object sender, EventArgs e)
        {
            FromBindingListToSettings();
            ClientSettings.Current.Save();

            ((Panel)this.Parent).BackColor = Color.FromArgb(0, 0, 0, 0);
            ((Panel)this.Parent).Controls.Clear();
        }
        #endregion

    }

    //Classes used for Datagrid Binding ======================================
    public class KeyBoardBindingItem
    {
        public string Name { get; set;}
        public string Binding { get; set; }
    }

    public class KeyBoardBindingItemWithInfo
    {
        public string Name { get; set; }
        public string Info { get; set; }
        public string Binding { get; set; }
    }

    public class SettingsBindingItem
    {
        public string Name { get; set; }
        public object Value { get; set; }
    }

}
