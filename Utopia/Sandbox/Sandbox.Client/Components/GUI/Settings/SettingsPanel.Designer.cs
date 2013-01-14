using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using S33M3CoreComponents.GUI.Nuclex.Controls;
using S33M3Resources.Structs;
using S33M3CoreComponents.GUI.Nuclex.Controls.Desktop;
using S33M3CoreComponents.GUI.Nuclex;
using Utopia.Shared.Settings;

namespace Sandbox.Client.Components.GUI.Settings
{
    public abstract partial class SettingsPanel
    {
        private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();


        #region Private Variables
        protected LabelControl _panelLabel;
        public List<ParamRow> Parameters;
        #endregion

        #region Public Variables
        #endregion
        
        private void InitializeComponent(object SettingParameters)
        {
            CreateComponents(SettingParameters);
            Resize();
            BindComponents();
        }

        #region Public Methods
        #endregion

        #region Private Methods
        private void CreateComponents(object SettingParameters)
        {
            _panelLabel = new LabelControl()
            {
                Text = _panelName,
                Color = new ByteColor(255, 255, 255),
                CustomFont = _parent.CommonResources.FontBebasNeue25
            };

            Parameters = new List<ParamRow>(Settings2Components.CreateComponentsRows(SettingParameters,
                                                                                     _parent.CommonResources.FontBebasNeue17,
                                                                                     _parent.CommonResources.StInputBackground,
                                                                                     _parent.CommonResources.StButtonBackground,
                                                                                     _parent.CommonResources.StButtonBackgroundDown,
                                                                                     _parent.CommonResources.StButtonBackgroundHover));
        }

        private void BindComponents()
        {
            this.Children.Add(_panelLabel);
            foreach (ParamRow row in Parameters)
            {
                this.Children.Add(ToDispose(row.LabelName));
                this.Children.Add(ToDispose(row.InputingComp));
                if (row.LabelInfo != null) this.Children.Add(ToDispose(row.LabelInfo));
                switch (row.ParamInputMethod)
                {
                    case ParamInputMethod.InputBox:
                        break;
                    case ParamInputMethod.CheckBox:
                        ((OptionControl)row.InputingComp).Changed += this._parent.SettingsPanel_Changed;
                        break;
                    case ParamInputMethod.Slider:
                        ((SliderControl)row.InputingComp).Moved += this._parent.SliderControl_Moved;
                        this._parent.SliderTextResfresh((SliderControl)row.InputingComp);
                        break;
                    case ParamInputMethod.ButtonList:
                        ((ButtonControl)row.InputingComp).Pressed += new EventHandler(this._parent.ButtonList_Pressed);
                        break;
                    default:
                        break;
                }
            }
        }

        public static int SliderWidth = 300;
        public void Resize()
        {
            if (this.Parent != null) this.Bounds = new UniRectangle(0, 0, this.Parent.Bounds.Size.X.Offset, this.Parent.Bounds.Size.Y.Offset);

            float BorderMargin = 15;
            _panelLabel.Bounds = new UniRectangle(BorderMargin, BorderMargin, 0, 0);

            float lineHeight = 50;

            foreach (ParamRow row in Parameters)
            {
                row.LabelName.Bounds = new UniRectangle(BorderMargin, lineHeight, 1, 0);
                switch (row.ParamInputMethod)
                {
                    case ParamInputMethod.InputBox:
                        row.InputingComp.Bounds = new UniRectangle(BorderMargin + 200, lineHeight - 5, 50, 25);
                        break;
                    case ParamInputMethod.CheckBox:
                        row.InputingComp.Bounds = new UniRectangle(BorderMargin + 200, lineHeight - 5, 50, 25);
                        break;
                    case ParamInputMethod.Slider:
                        row.InputingComp.Bounds = new UniRectangle(BorderMargin + 200, lineHeight - 5, SliderWidth, 25);
                        break;
                    case ParamInputMethod.ButtonList:
                        row.InputingComp.Bounds = new UniRectangle(BorderMargin + 200, lineHeight - 7, 150, 30);
                        break;
                    default:
                        break;
                }

                if (row.LabelInfo != null) row.LabelInfo.Bounds = new UniRectangle(row.InputingComp.Bounds.Location.X + row.InputingComp.Bounds.Size.X + 10, lineHeight, 1, 0);
                lineHeight += 30;
            }
        }
        #endregion
    }
}
