using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using S33M3CoreComponents.GUI.Nuclex.Controls;
using S33M3Resources.Structs;
using S33M3CoreComponents.GUI.Nuclex.Visuals.Flat;
using S33M3CoreComponents.GUI.Nuclex;
using SharpDX.Direct3D11;
using Utopia.Settings;
using System.Reflection;
using S33M3CoreComponents.GUI.Nuclex.Controls.Desktop;
using S33M3CoreComponents.Tools;

namespace Sandbox.Client.Components.GUI.Settings
{
    partial class GraphicSettingsPanel
    {
        private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        #region Private Variables
        protected LabelControl _panelLabel;

        protected List<ParamRow> Parameters = new List<ParamRow>();
        #endregion

        #region Public Variables
        #endregion

        private void InitializeComponent()
        {
            CreateComponents();
            Resize();
            BindComponents();
        }

        #region Public Methods
        #endregion

        #region Private Methods
        private void CreateComponents()
        {
            _panelLabel = new LabelControl()
            {
                Text = "Graphical settings",
                Color = new ByteColor(255, 255, 255),
                CustomFont = SandboxMenuComponent.FontBebasNeue25
            };

            var parameters = ClientSettings.Current.Settings.GraphicalParameters;
            foreach (PropertyInfo data in GetParameters(parameters))
            {
                var value = data.GetValue(parameters, null);

                string name = string.Empty;
                string info = string.Empty;
                FieldInfo val = default(FieldInfo);
                object paramValue = null;
                foreach (FieldInfo field in value.GetType().GetFields())
                {
                    if (field.Name == "Value")
                    {
                        val = field;
                        paramValue = field.GetValue(value);
                    }
                    if (field.Name == "Name")
                    {
                        name = field.GetValue(value).ToString();
                    }
                    if (field.Name == "Info")
                    {
                        info = field.GetValue(value).ToString();
                    }
                }

                switch (DataTypes.GetTypeFamilly(paramValue.GetType()))
                {
                    case DataTypes.typeFamilly.IntegerNumber:
                        AddIntParam(name, (int)paramValue);
                     break;
                    case DataTypes.typeFamilly.FloatNumber:
                     break;
                    case DataTypes.typeFamilly.String:
                     break;
                    case DataTypes.typeFamilly.Boolean:
                     break;
                    case DataTypes.typeFamilly.Unknown:
                    default:
                     logger.Warn("Cannot parse parameter {0} with value {1}", data.Name, value); 
                     break;
                }
            }
        }

        private void AddIntParam(string ParameterName, int value)
        {
            LabelControl label = new LabelControl() { Text = ParameterName, CustomFont = SandboxMenuComponent.FontBebasNeue17 };
            InputControl input = new InputControl()
            {
                Text = value.ToString(),
                IsNumeric = true,
                CustomBackground = SandboxMenuComponent.StInputBackground,
                CustomFont = SandboxMenuComponent.FontBebasNeue17,
                Color = SharpDX.Colors.White
            };
            Parameters.Add(new ParamRow() { ParamName = label, InputingComp = input });
        }

        private string AddSpaceBeforeUpperCase(string name)
        {
            StringBuilder sb = new StringBuilder();

            bool isfirstChar = true;
            foreach (char c in name)
            {
                if (char.IsUpper(c) && isfirstChar == false)
                {
                    sb.Append(' ');
                }
                sb.Append(c);
                isfirstChar = false;
            }

            return sb.ToString();

        }

        private IEnumerable<PropertyInfo> GetParameters(GraphicalParameters param)
        {
            //Create a line per graphical components
            Type reflectedType;
            PropertyInfo[] pi;

            reflectedType = param.GetType();
            pi = reflectedType.GetProperties();
            foreach (PropertyInfo field in pi)
            {
                yield return field;
            }
        }

        private void BindComponents()
        {
            this.Children.Add(_panelLabel);
            foreach (ParamRow row in Parameters)
            {
                this.Children.Add(row.ParamName);
                this.Children.Add(row.InputingComp);
            }
        }

        public void Resize()
        {
            if(this.Parent != null) this.Bounds = new UniRectangle(0,0,this.Parent.Bounds.Size.X.Offset,this.Parent.Bounds.Size.Y.Offset);

            float BorderMargin = 15;
            _panelLabel.Bounds = new UniRectangle(BorderMargin, BorderMargin, 0, 0);

            float lineHeight = 50;

            foreach (ParamRow row in Parameters)
            {
                row.ParamName.Bounds = new UniRectangle(BorderMargin, lineHeight, 1 , 0);
                row.InputingComp.Bounds = new UniRectangle(BorderMargin + 200, lineHeight, 50, 25);
                lineHeight += 30;
            }
        }
        #endregion
    }
}
