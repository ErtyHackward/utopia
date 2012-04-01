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
using Utopia.Settings;

namespace Sandbox.Client.Components.GUI.Settings
{
    public partial class SettingsComponent : GameComponent
    {
        #region Private variables
        private readonly D3DEngine _engine;
        private readonly MainScreen _screen;

        #endregion

        #region Public properties/methods
        #endregion

        public SettingsComponent(D3DEngine engine, MainScreen screen)
        {
            _engine = engine;
            _screen = screen;

            _engine.ViewPort_Updated += UpdateLayout;
        }

        public override void Dispose()
        {
            _engine.ViewPort_Updated -= UpdateLayout;
            base.Dispose();
        }

        #region Public methods
        public override void Initialize()
        {
            InitializeComponent();
        }

        public override void LoadContent(DeviceContext context)
        {
            LoadContentComponent(context);
            RefreshComponentsVisibility();
        }

        protected override void OnUpdatableChanged(object sender, EventArgs args)
        {
            if (!IsInitialized) return;

            RefreshComponentsVisibility();

            base.OnUpdatableChanged(sender, args);
        }
        #endregion

        #region Private methods
        public void SaveChange()
        {
            List<ParamRow> Parameters = new List<ParamRow>();
            object Parameter;
            Type reflectedType;
            PropertyInfo[] pi;
            //Saving Graphical Parameters ========================================================
            if (_graphSettingsPanel != null) Parameters = this._graphSettingsPanel.Parameters;

            //Saving the Graphical changes
            Parameter = ClientSettings.Current.Settings.GraphicalParameters;
            reflectedType = Parameter.GetType();
            pi = reflectedType.GetProperties();
            foreach (ParamRow row in Parameters)
            {
                var piTmp = pi.First(x => x.Name == row.FieldData.Name);
                if (row.FieldData.Value.GetType() != piTmp.PropertyType)
                {
                    var castedValue = piTmp.PropertyType.InvokeMember("Parse", BindingFlags.InvokeMethod, null, piTmp.PropertyType, new object[] { row.FieldData.Value });
                    piTmp.SetValue(Parameter, castedValue, null);
                }
                else
                {
                    piTmp.SetValue(Parameter, row.FieldData.Value, null);
                }
            }

            //Saving Core Engine Parameters ========================================================
            if (_coreEngineSetingsPanel != null) Parameters = this._coreEngineSetingsPanel.Parameters;

            //Saving the Graphical changes
            Parameter = ClientSettings.Current.Settings.EngineParameters;
            reflectedType = ClientSettings.Current.Settings.EngineParameters.GetType();
            pi = reflectedType.GetProperties();
            foreach (ParamRow row in Parameters)
            {
                var piTmp = pi.First(x => x.Name == row.FieldData.Name);
                if (row.FieldData.Value.GetType() != piTmp.PropertyType)
                {
                    var castedValue = piTmp.PropertyType.InvokeMember("Parse", BindingFlags.InvokeMethod, null, piTmp.PropertyType, new object[] { row.FieldData.Value });
                    piTmp.SetValue(Parameter, castedValue, null);
                }
                else
                {
                    piTmp.SetValue(Parameter, row.FieldData.Value, null);
                }
            }

            //Saving Sound Engine Parameters ========================================================
            if (_soundSettingsPanel != null) Parameters = this._soundSettingsPanel.Parameters;

            //Saving the Graphical changes
            Parameter = ClientSettings.Current.Settings.SoundParameters;
            reflectedType = ClientSettings.Current.Settings.EngineParameters.GetType();
            pi = reflectedType.GetProperties();
            foreach (ParamRow row in Parameters)
            {
                var piTmp = pi.First(x => x.Name == row.FieldData.Name);
                if (row.FieldData.Value.GetType() != piTmp.PropertyType)
                {
                    var castedValue = piTmp.PropertyType.InvokeMember("Parse", BindingFlags.InvokeMethod, null, piTmp.PropertyType, new object[] { row.FieldData.Value });
                    piTmp.SetValue(Parameter, castedValue, null);
                }
                else
                {
                    piTmp.SetValue(Parameter, row.FieldData.Value, null);
                }
            }

            ClientSettings.Current.Save();
        }

        //ButtonList Event management ==========================================
        public void ButtonList_Pressed(object sender, EventArgs e)
        {
            ButtonControl bt = (ButtonControl)sender;
            List<string> values = (List<string>)bt.Tag;
            ((ParamRow)bt.Tag2).FieldData.Value = values[0];
            try
            {
                int currentIndex = values.FindIndex(x => x == bt.Text);
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
