using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using S33M3DXEngine.Main;
using S33M3DXEngine;
using S33M3CoreComponents.GUI.Nuclex;
using S33M3CoreComponents.GUI.Nuclex.Controls;
using S33M3CoreComponents.GUI.Nuclex.Controls.Desktop;
using SharpDX.Direct3D11;
using SharpDX;
using S33M3CoreComponents.GUI.Nuclex.Controls.Arcade;
using System.Reflection;
using S33M3CoreComponents.Inputs.KeyboardHandler;
using System.Windows.Forms;
using S33M3DXEngine.Threading;
using Utopia.Shared.Settings;
using Ninject;
using S33M3DXEngine.Main.Interfaces;
using Utopia.Worlds.SkyDomes.SharedComp;
using Sandbox.Client.States;
using S33M3CoreComponents.Config;
using Utopia.Worlds.SkyDomes;
using Utopia.Components;
using S33M3CoreComponents.Sound;
using Utopia.Worlds.Chunks;

namespace Sandbox.Client.Components.GUI.Settings
{
    public partial class SettingsComponent : MenuTemplate1Component
    {
        #region Private variables
        private IKernel _iocContainer;
        private ISoundEngine _soundEngine;
        #endregion

        #region Public properties/methods
        public event EventHandler KeyBindingChanged;
        public bool isGameRunning { get; set; }
        #endregion

        public SettingsComponent(Game game, D3DEngine engine, MainScreen screen, SandboxCommonResources commonResources, IKernel iocContainer, ISoundEngine soundEngine)
            :base(game, engine, screen, commonResources)
        {
            _iocContainer = iocContainer;
            _soundEngine = soundEngine;
        }

        public override void BeforeDispose()
        {
            if (KeyBindingChanged != null)
            {
                //Remove all Events associated to this control (That haven't been unsubscribed !)
                foreach (Delegate d in KeyBindingChanged.GetInvocationList())
                {
                    KeyBindingChanged -= (EventHandler)d;
                }
            }

            base.BeforeDispose();
        }

        #region Public methods
        public override void LoadContent(DeviceContext context)
        {
            LoadContentComponent(context);
            base.LoadContent(context);
        }

        #endregion

        #region Private methods
        public void SaveChange()
        {
            List<ParamRow> Parameters = new List<ParamRow>();
            object Parameter;
            Type reflectedType;
            PropertyInfo[] pi;
            bool _restartNeeded = false;
            //Saving Graphical Parameters ========================================================
            if (_graphSettingsPanel != null)
            {
                Parameters = this._graphSettingsPanel.Parameters;

                //Saving the Graphical changes
                Parameter = ClientSettings.Current.Settings.GraphicalParameters;
                reflectedType = Parameter.GetType();
                pi = reflectedType.GetProperties();
                foreach (ParamRow row in Parameters)
                {
                    var piTmp = pi.First(x => x.Name == row.FieldData.Name);
                    var previousValue = piTmp.GetValue(Parameter, null);

                    ParameterAttribute attrib = (ParameterAttribute)piTmp.GetCustomAttributes(typeof(ParameterAttribute), true)[0];

                    if (row.FieldData.Value.GetType() != piTmp.PropertyType)
                    {
                        var castedValue = piTmp.PropertyType.InvokeMember("Parse", BindingFlags.InvokeMethod, null, piTmp.PropertyType, new object[] { row.FieldData.Value });
                        if (previousValue.ToString() != castedValue.ToString())
                        {
                            if (attrib.NeedRestartAfterChange) _restartNeeded = true;
                            piTmp.SetValue(Parameter, castedValue, null);
                        }
                    }
                    else
                    {
                        if (previousValue.ToString() != row.FieldData.Value.ToString())
                        {
                            piTmp.SetValue(Parameter, row.FieldData.Value, null);
                            if (attrib.NeedRestartAfterChange) _restartNeeded = true;
                        }

                        switch (row.FieldData.Name)
                        {
                            case "TexturePack":
                                //Refresh TexturePackConfig value
                                TexturePackConfig.Current = new XmlSettingsManager<TexturePackSetting>(@"TexturePackConfig.xml", SettingsStorage.CustomPath, @"TexturesPacks\" + ClientSettings.Current.Settings.GraphicalParameters.TexturePack + @"\");
                                TexturePackConfig.Current.Load();
                                break;
                            case "VSync":
                                ChangeVSync((bool)row.FieldData.Value);
                                break;
                            case "StaticEntityViewSize":
                                ChangeVisibleStaticEntities((int)row.FieldData.Value);
                                break;
                            case "LandscapeFog":
                                ChangeLandscapeFog((string)row.FieldData.Value);
                                break;
                            default:
                                break;
                        }
                    }
                }
            }

            //Saving Core Engine Parameters ========================================================
            if (_coreEngineSetingsPanel != null)
            {
                Parameters = this._coreEngineSetingsPanel.Parameters;

                //Saving the Graphical changes
                Parameter = ClientSettings.Current.Settings.EngineParameters;
                reflectedType = ClientSettings.Current.Settings.EngineParameters.GetType();
                pi = reflectedType.GetProperties();
                foreach (ParamRow row in Parameters)
                {
                    var piTmp = pi.First(x => x.Name == row.FieldData.Name);
                    var previousValue = piTmp.GetValue(Parameter, null);
                    ParameterAttribute attrib = (ParameterAttribute)piTmp.GetCustomAttributes(typeof(ParameterAttribute), true)[0];

                    if (row.FieldData.Value.GetType() != piTmp.PropertyType)
                    {
                        var castedValue = piTmp.PropertyType.InvokeMember("Parse", BindingFlags.InvokeMethod, null, piTmp.PropertyType, new object[] { row.FieldData.Value });
                        if (previousValue.ToString() != castedValue.ToString())
                        {
                            if (attrib.NeedRestartAfterChange) _restartNeeded = true;
                            piTmp.SetValue(Parameter, castedValue, null);
                        }
                    }
                    else
                    {
                        if (previousValue.ToString() != row.FieldData.Value.ToString())
                        {
                            if (attrib.NeedRestartAfterChange) _restartNeeded = true;
                            piTmp.SetValue(Parameter, row.FieldData.Value, null);

                            switch (row.FieldData.Name)
                            {
                                case "AllocatedThreadsModifier":
                                    ChangeAllocatedThreads((int)row.FieldData.Value);
                                    break;
                                default:
                                    break;
                            }

                        }
                    }
                }
            }

            //Saving Sound Engine Parameters ========================================================
            if (_soundSettingsPanel != null)
            {
                Parameters = this._soundSettingsPanel.Parameters;

                //Saving the Graphical changes
                Parameter = ClientSettings.Current.Settings.SoundParameters;
                reflectedType = ClientSettings.Current.Settings.SoundParameters.GetType();
                pi = reflectedType.GetProperties();
                foreach (ParamRow row in Parameters)
                {
                    var piTmp = pi.First(x => x.Name == row.FieldData.Name);
                    var previousValue = piTmp.GetValue(Parameter, null);

                    ParameterAttribute attrib = (ParameterAttribute)piTmp.GetCustomAttributes(typeof(ParameterAttribute), true)[0];

                    if (row.FieldData.Value.GetType() != piTmp.PropertyType)
                    {
                        var castedValue = piTmp.PropertyType.InvokeMember("Parse", BindingFlags.InvokeMethod, null, piTmp.PropertyType, new object[] { row.FieldData.Value });
                        if (previousValue.ToString() != castedValue.ToString())
                        {
                            if (attrib.NeedRestartAfterChange) _restartNeeded = true;
                            piTmp.SetValue(Parameter, castedValue, null);
                        }
                    }
                    else
                    {
                        if (previousValue.ToString() != row.FieldData.Value.ToString())
                        {
                            if (attrib.NeedRestartAfterChange) _restartNeeded = true;
                            piTmp.SetValue(Parameter, row.FieldData.Value, null);

                            switch (row.FieldData.Name)
                            {
                                case "GlobalFXVolume":
                                    SetGlobalFXVol((int)row.FieldData.Value);
                                    break;
                                case "GlobalMusicVolume":
                                    SetGlobalMusicVol((int)row.FieldData.Value);
                                    break;
                                default:
                                    break;
                            }
                        }
                    }
                }
            }

            //Saving Key binding Parameters ========================================================
            if (_keyBindingSettingsPanel != null)
            {
                bool isBindingChanged = false;
                string[] bindings;
                KeyWithModifier kwm;
                FieldInfo[] fi;

                //For each move key binded
                reflectedType = ClientSettings.Current.Settings.KeyboardMapping.Move.GetType();
                fi = reflectedType.GetFields();
                foreach (KeyBindingSettingsPanel.KeyBindComponent keyBinding in _keyBindingSettingsPanel.MoveKeys)
                {
                    bindings = keyBinding.input.Text.Split(new string[] { " + " }, StringSplitOptions.RemoveEmptyEntries);
                    kwm = new KeyWithModifier();
                    kwm.MainKey = (Keys)Enum.Parse(typeof(Keys), bindings[0]);
                    kwm.Info = keyBinding.Key.Info;
                    if (bindings.Length > 1)
                    {
                        kwm.Modifier = (Keys)Enum.Parse(typeof(Keys), bindings[1]);
                    }

                    FieldInfo field = fi.First(x => x.Name == keyBinding.Name.Text);
                    KeyWithModifier oldVvalue = (KeyWithModifier)field.GetValue(ClientSettings.Current.Settings.KeyboardMapping.Move);
                    if (oldVvalue != kwm)
                    {
                        field.SetValue(ClientSettings.Current.Settings.KeyboardMapping.Move, kwm);
                        isBindingChanged = true;
                    }
                }

                //For each Game key binded
                reflectedType = ClientSettings.Current.Settings.KeyboardMapping.Game.GetType();
                fi = reflectedType.GetFields();
                foreach (KeyBindingSettingsPanel.KeyBindComponent keyBinding in _keyBindingSettingsPanel.GameKeys)
                {
                    bindings = keyBinding.input.Text.Split(new string[] { " + " }, StringSplitOptions.RemoveEmptyEntries);
                    kwm = new KeyWithModifier();
                    kwm.MainKey = (Keys)Enum.Parse(typeof(Keys), bindings[0]);
                    kwm.Info = keyBinding.Key.Info;
                    if (bindings.Length > 1)
                    {
                        kwm.Modifier = (Keys)Enum.Parse(typeof(Keys), bindings[1]);
                    }

                    FieldInfo field = fi.First(x => x.Name == keyBinding.Name.Text);
                    KeyWithModifier oldVvalue = (KeyWithModifier)field.GetValue(ClientSettings.Current.Settings.KeyboardMapping.Game);
                    if (oldVvalue != kwm)
                    {
                        field.SetValue(ClientSettings.Current.Settings.KeyboardMapping.Game, kwm);
                        isBindingChanged = true;
                    }
                }

                //For each System key binded
                reflectedType = ClientSettings.Current.Settings.KeyboardMapping.System.GetType();
                fi = reflectedType.GetFields();
                foreach (KeyBindingSettingsPanel.KeyBindComponent keyBinding in _keyBindingSettingsPanel.SystemKeys)
                {
                    bindings = keyBinding.input.Text.Split(new string[] { " + " }, StringSplitOptions.RemoveEmptyEntries);
                    kwm = new KeyWithModifier();
                    kwm.MainKey = (Keys)Enum.Parse(typeof(Keys), bindings[0]);
                    kwm.Info = keyBinding.Key.Info;
                    if (bindings.Length > 1)
                    {
                        kwm.Modifier = (Keys)Enum.Parse(typeof(Keys), bindings[1]);
                    }

                    FieldInfo field = fi.First(x => x.Name == keyBinding.Name.Text);
                    KeyWithModifier oldVvalue = (KeyWithModifier)field.GetValue(ClientSettings.Current.Settings.KeyboardMapping.System);
                    if (oldVvalue != kwm)
                    {
                        field.SetValue(ClientSettings.Current.Settings.KeyboardMapping.System, kwm);
                        isBindingChanged = true;
                    }
                }

                //Raised the KeyBinding Event if a key has been changed
                if (isBindingChanged)
                {
                    if (KeyBindingChanged != null)
                    {
                        KeyBindingChanged(this, null);
                        logger.Info("Keyboard binding Saved, KeyBindingChanged event raised");
                    }
                }
            }

            if (_restartNeeded)
            {
                _settingsStateLabel.IsVisible = true;
            }

            ClientSettings.Current.Save();
        }

        private void ChangeAllocatedThreads(int newValue)
        {
            ThreadsManager.SetOptimumNbrThread(ClientSettings.Current.Settings.DefaultAllocatedThreads + newValue, true);
        }

        private void ChangeVSync(bool vsyncValue)
        {
            _game.VSync = vsyncValue;
        }

        private void ChangeVisibleStaticEntities(int distance)
        {
            if (isGameRunning)
            {
                var worldChunks = _iocContainer.Get<IWorldChunks>();

                if (distance > (ClientSettings.Current.Settings.GraphicalParameters.WorldSize / 2) - 2.5)
                {
                    worldChunks.StaticEntityViewRange = (int)((ClientSettings.Current.Settings.GraphicalParameters.WorldSize / 2) - 2.5) * 16;
                }
                else
                {
                    worldChunks.StaticEntityViewRange = ClientSettings.Current.Settings.GraphicalParameters.StaticEntityViewSize * 16;
                }
            }
        }

        private void ChangeLandscapeFog(string landscapeFogType)
        {
            if (isGameRunning)
            {
                var skyDome = _iocContainer.Get<ISkyDome>();
                if (landscapeFogType == "SkyFog")
                {
                    StaggingBackBuffer skyBackBuffer = _iocContainer.Get<StaggingBackBuffer>("SkyBuffer");
                    skyBackBuffer.EnableComponent(true);
                    skyDome.DrawOrders.UpdateIndex(0, 40);
                }
                else
                {
                    StaggingBackBuffer skyBackBuffer = _iocContainer.Get<StaggingBackBuffer>("SkyBuffer");
                    skyBackBuffer.DisableComponent();
                    skyDome.DrawOrders.UpdateIndex(0, 990);
                }
            }
        }

        private void SetGlobalMusicVol(int value)
        {
            _soundEngine.GlobalMusicVolume = (value / 100.0f);
        }

        private void SetGlobalFXVol(int value)
        {
            _soundEngine.GlobalFXVolume = (value / 100.0f);
        }

        //ButtonList Event management ==========================================
        public void ButtonList_Pressed(object sender, EventArgs e)
        {
            ButtonControl bt = (ButtonControl)sender;
            List<object> values = (List<object>)bt.Tag;

            //Dynamic collection value ???
            if (values.Count == 1 && (values[0]).ToString().Contains("CLIST_"))
            {
                values = ClientSettings.DynamicLists[values[0].ToString()];
            }

            ((ParamRow)bt.Tag2).FieldData.Value = values[0];
            try
            {
                int currentIndex = values.FindIndex(x => x.ToString() == bt.Text);
                currentIndex = currentIndex == values.Count - 1 ? 0 : currentIndex + 1;
                ((ParamRow)bt.Tag2).FieldData.Value = values[currentIndex];
                SaveChange();
            }
            catch (Exception)
            {
            }
            bt.Text = ((ParamRow)bt.Tag2).FieldData.Value.ToString();
        }
        //==================================================================

        //CheckBox Event management ==========================================
        public void SettingsPanel_Changed(object sender, EventArgs e)
        {
            OptionControl checkbox = (OptionControl)sender;
            ((ParamRow)checkbox.Tag2).FieldData.Value = checkbox.Selected;
            SaveChange();
        }
        //CheckBox Event management ==========================================

        //Slider Event management ==========================================
        public void SliderControl_Moved(object sender, EventArgs e)
        {
            SliderTextResfresh((SliderControl)sender);
        }

        public void SliderTextResfresh(SliderControl ctr)
        {
            LabelControl infoLabel = (LabelControl)ctr.Tag;
            infoLabel.Text = ctr.Value.ToString();

            if ((int)((ParamRow)ctr.Tag2).FieldData.Value != ctr.Value)
            {
                ((ParamRow)ctr.Tag2).FieldData.Value = ctr.Value;
                SaveChange();
            }
        }
        //==================================================================
        #endregion
    }
}
