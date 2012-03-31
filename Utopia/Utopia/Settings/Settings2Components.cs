using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using Utopia.Settings;
using S33M3CoreComponents.GUI.Nuclex.Controls;
using S33M3CoreComponents.GUI.Nuclex.Controls.Desktop;
using S33M3CoreComponents.Tools;
using S33M3CoreComponents.Sprites;
using S33M3Resources.Structs;

namespace Utopia.Settings
{
    public static class Settings2Components
    {
        public static IEnumerable<ParamRow> CreateComponentsRows(object settingParameter, SpriteFont customFont, SpriteTexture customInputBackgroundTexture, SpriteTexture customButton, SpriteTexture customButtonDown, SpriteTexture customButtonHover)
        {
            foreach (PropertyInfo data in GetParameters(settingParameter))
            {
                var value = data.GetValue(settingParameter, null);

                ParameterAttribute attrib = (ParameterAttribute)data.GetCustomAttributes(typeof(ParameterAttribute), true)[0];

                switch (attrib.InputMethod)
                {
                    case ParamInputMethod.InputBox:
                        yield return AddInputComponent(attrib.ParamName, value, DataTypes.GetTypeFamilly(value.GetType()) == DataTypes.typeFamilly.IntegerNumber, customFont, customInputBackgroundTexture);
                        break;
                    case ParamInputMethod.CheckBox:
                        yield return AddInputComponent(attrib.ParamName, value, DataTypes.GetTypeFamilly(value.GetType()) == DataTypes.typeFamilly.IntegerNumber, customFont, customInputBackgroundTexture);
                        break;
                    case ParamInputMethod.Slider:
                        yield return AddSliderComponent(attrib.ParamName, value, attrib, customFont);
                        break;
                    case ParamInputMethod.ButtonList:
                        yield return AddListComponent(attrib.ParamName, value, attrib, customFont, customButton, customButtonDown, customButtonHover);
                        break;
                    default:
                        break;
                }
            }
        }

        private static ParamRow AddInputComponent(string ParameterName, object value, bool isNumeric, SpriteFont customFont, SpriteTexture customInputBackgroundTexture)
        {
            LabelControl label = new LabelControl() { Text = ParameterName, CustomFont = customFont };
            InputControl input = new InputControl()
            {
                Text = value.ToString(),
                IsNumeric = isNumeric,
                CustomBackground = customInputBackgroundTexture,
                CustomFont = customFont,
                Color = SharpDX.Colors.White
            };
            return new ParamRow() { ParamName = label, InputingComp = input, ParamInputMethod = ParamInputMethod.InputBox };
        }

        private static ParamRow AddListComponent(string ParameterName, object value, ParameterAttribute attrib, SpriteFont customFont, SpriteTexture customButton, SpriteTexture customButtonDown, SpriteTexture customButtonHover)
        {
            LabelControl label = new LabelControl() { Text = ParameterName, CustomFont = customFont };
            ButtonControl buttonList = new ButtonControl()
            {
                CustomImage = customButton,
                CustomImageDown = customButtonDown,
                CustomImageHover = customButtonHover,
                Text = value.ToString(),
                TextFontId = 1,
                Color = new ByteColor(200, 200, 200, 255)
            };
            buttonList.Tag = attrib.ListValues;
            return new ParamRow() { ParamName = label, InputingComp = buttonList, ParamInputMethod = ParamInputMethod.ButtonList };
        }

        private static ParamRow AddSliderComponent(string ParameterName, object value, ParameterAttribute attrib, SpriteFont customFont)
        {
            LabelControl label = new LabelControl() { Text = ParameterName, CustomFont = customFont };
            LabelControl labelInfo = new LabelControl() { Text = "???", CustomFont = customFont, Suffix = attrib.InfoSuffix };

            labelInfo.Tag = attrib;
            HorizontalSliderControl input = new HorizontalSliderControl()
            {
                ThumbSize = 1 / (float)(attrib.MaxSliderValue - attrib.MinSliderValue)
            };
            input.Tag = value;
            return new ParamRow() { ParamName = label, InputingComp = input, LabelInfo = labelInfo, ParamInputMethod = ParamInputMethod.Slider };
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
