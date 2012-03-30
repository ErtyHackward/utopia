using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using Utopia.Settings;
using S33M3CoreComponents.GUI.Nuclex.Controls;
using S33M3CoreComponents.GUI.Nuclex.Controls.Desktop;
using S33M3CoreComponents.Tools;

namespace Sandbox.Client.Components.GUI.Settings
{
    public static class Settings2Components
    {
        public static IEnumerable<ParamRow> CreateComponentsRows(object settingParameter)
        {
            foreach (PropertyInfo data in GetParameters(settingParameter))
            {
                var value = data.GetValue(settingParameter, null);

                ParameterAttribute attrib = (ParameterAttribute)data.GetCustomAttributes(typeof(ParameterAttribute), true)[0];

                switch (attrib.InputMethod)
                {
                    case ParamInputMethod.InputBox:
                        yield return AddInputComponent(attrib.ParamName, value, DataTypes.GetTypeFamilly(value.GetType()) == DataTypes.typeFamilly.IntegerNumber);
                        break;
                    case ParamInputMethod.CheckBox:
                        yield return AddInputComponent(attrib.ParamName, value, DataTypes.GetTypeFamilly(value.GetType()) == DataTypes.typeFamilly.IntegerNumber);
                        break;
                    case ParamInputMethod.Slider:
                        yield return AddInputComponent(attrib.ParamName, value, DataTypes.GetTypeFamilly(value.GetType()) == DataTypes.typeFamilly.IntegerNumber);
                        break;
                    case ParamInputMethod.List:
                        yield return AddInputComponent(attrib.ParamName, value, DataTypes.GetTypeFamilly(value.GetType()) == DataTypes.typeFamilly.IntegerNumber);
                        break;
                    default:
                        break;
                }
            }
        }

        private static ParamRow AddInputComponent(string ParameterName, object value, bool isNumeric)
        {
            LabelControl label = new LabelControl() { Text = ParameterName, CustomFont = SandboxMenuComponent.FontBebasNeue17 };
            InputControl input = new InputControl()
            {
                Text = value.ToString(),
                IsNumeric = isNumeric,
                CustomBackground = SandboxMenuComponent.StInputBackground,
                CustomFont = SandboxMenuComponent.FontBebasNeue17,
                Color = SharpDX.Colors.White
            };
            return new ParamRow() { ParamName = label, InputingComp = input };
        }

        private static IEnumerable<PropertyInfo> GetParameters(object param)
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
    }
}
