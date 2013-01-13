using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using S33M3CoreComponents.GUI.Nuclex.Controls;
using S33M3CoreComponents.GUI.Nuclex.Controls.Desktop;
using S33M3CoreComponents.Tools;
using S33M3CoreComponents.Sprites2D;
using S33M3Resources.Structs;

namespace Utopia.Shared.Settings
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
                        yield return AddInputComponent(attrib.ParamName, value, data.Name, DataTypes.GetTypeFamilly(value.GetType()) == DataTypes.typeFamilly.IntegerNumber, customFont, customInputBackgroundTexture);
                        break;
                    case ParamInputMethod.CheckBox:
                        yield return AddCheckBoxComponent(attrib.ParamName, (bool)value, data.Name, DataTypes.GetTypeFamilly(value.GetType()) == DataTypes.typeFamilly.IntegerNumber, customFont, customInputBackgroundTexture);
                        break;
                    case ParamInputMethod.Slider:
                        yield return AddSliderComponent(attrib.ParamName, value, data.Name, attrib, customFont);
                        break;
                    case ParamInputMethod.ButtonList:
                        yield return AddListComponent(attrib.ParamName, value, data.Name, attrib, customFont, customButton, customButtonDown, customButtonHover);
                        break;
                    default:
                        break;
                }
            }
        }

        private static ParamRow AddCheckBoxComponent(string ParameterName, bool value, string fieldName, bool isNumeric, SpriteFont customFont, SpriteTexture customInputBackgroundTexture)
        {
            LabelControl label = new LabelControl() { Text = ParameterName, CustomFont = customFont };
            OptionControl input = new OptionControl()
            {
                Selected = value
            };
            ParamRow row = new ParamRow() { LabelName = label, InputingComp = input, ParamInputMethod = ParamInputMethod.CheckBox, FieldData = new ParamValue() { Value = value, Name = fieldName } };
            input.Tag2 = row;
            return row;
        }

        private static ParamRow AddInputComponent(string ParameterName, object value, string fieldName, bool isNumeric, SpriteFont customFont, SpriteTexture customInputBackgroundTexture)
        {
            LabelControl label = new LabelControl() { Text = ParameterName, CustomFont = customFont };
            InputControl input = new InputControl()
            {
                Text = value.ToString(),
                IsNumeric = isNumeric,
                CustomBackground = customInputBackgroundTexture,
                CustomFont = customFont,
                Color = SharpDX.Color.White
            };
            ParamRow row = new ParamRow() { LabelName = label, InputingComp = input, ParamInputMethod = ParamInputMethod.InputBox, FieldData = new ParamValue() { Value = value, Name = fieldName } };
            input.Tag2 = row;
            return row;
        }

        private static ParamRow AddListComponent(string ParameterName, object value, string fieldName, ParameterAttribute attrib, SpriteFont customFont, SpriteTexture customButton, SpriteTexture customButtonDown, SpriteTexture customButtonHover)
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
            ParamRow row = new ParamRow() { LabelName = label, InputingComp = buttonList, ParamInputMethod = ParamInputMethod.ButtonList, FieldData = new ParamValue() { Value = value, Name = fieldName } };
            buttonList.Tag2 = row;
            return row;
        }

        private static ParamRow AddSliderComponent(string ParameterName, object value, string fieldName, ParameterAttribute attrib, SpriteFont customFont)
        {
            LabelControl label = new LabelControl() { Text = ParameterName, CustomFont = customFont };
            LabelControl labelInfo = new LabelControl() { Text = "???", CustomFont = customFont, Suffix = attrib.InfoSuffix };

            HorizontalSliderControl input = new HorizontalSliderControl()
            {
                ThumbSize = 0.1f,//1 / (float)(attrib.MaxSliderValue - attrib.MinSliderValue),
                ThumbSmoothMovement = true
            };
            input.ThumbMinValue = (int)attrib.MinSliderValue;
            input.ThumbMaxValue = (int)attrib.MaxSliderValue;
            input.Value = (int)value;
            input.Tag = labelInfo;
            ParamRow row = new ParamRow() { LabelName = label, InputingComp = input, LabelInfo = labelInfo, ParamInputMethod = ParamInputMethod.Slider, FieldData = new ParamValue() { Value = value, Name = fieldName } };
            input.Tag2 = row;
            return row;
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
