using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing.Design;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Windows.Forms.Design;
using Utopia.Shared.Configuration;
using Utopia.Shared.LandscapeEntities.Trees;
using Utopia.Shared.Settings;
using Utopia.Shared.Tools;

namespace Utopia.Shared.Entities
{
    public partial class GrowingEntity
    {
        public class MultiBlockListEditor : UITypeEditor
        {
            private CheckValuesEditorControl<BlockProfile> editor = null;

            // we tell to the designer host that this editor is a DropDown editor
            public override UITypeEditorEditStyle GetEditStyle(System.ComponentModel.ITypeDescriptorContext context)
            {
                return UITypeEditorEditStyle.DropDown;
            }

            public override object EditValue(ITypeDescriptorContext context, IServiceProvider provider, object value)
            {
                List<BlockProfile> values = null;
                if (value != null)
                {
                    values = new List<BlockProfile>(EditorConfigHelper.Config.BlockProfiles.Where(x => ((List<byte>)value).Contains(x.Id) && x.Name != "System Reserved")); //value as List<BlockProfile>;
                }

                if (provider != null)
                {
                    // use windows forms editor service to show drop down
                    IWindowsFormsEditorService edSvc = provider.GetService(typeof(IWindowsFormsEditorService))
                            as IWindowsFormsEditorService;
                    if (edSvc == null) return value;

                    if (editor == null)
                        editor = new CheckValuesEditorControl<BlockProfile>();

                    // prepare list

                    editor.Begin(edSvc, EditorConfigHelper.Config.BlockProfiles.Where(x => x.Name != "System Reserved").OrderBy(x => x.Name).ToList(), values);

                    // show drop down now
                    edSvc.DropDownControl(editor);

                    // now we take the result
                    value = new List<byte>(editor.GetSelectedValues().Select(x => x.Id));

                    // reset
                    editor.End();
                }

                return value;
            }
        }

        public class TreeListEditor : StringConverter
        {
            public override bool GetStandardValuesSupported(ITypeDescriptorContext context)
            {
                //true means show a combobox
                return true;
            }

            public override bool GetStandardValuesExclusive(ITypeDescriptorContext context)
            {
                //true will limit to list. false will show the list, 
                //but allow free-form entry
                return true;
            }

            public override TypeConverter.StandardValuesCollection GetStandardValues(ITypeDescriptorContext context)
            {
                return new StandardValuesCollection(EditorConfigHelper.Config.TreeBluePrints.OrderBy(x => x.Name).ToList());
            }

            public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
            {
                return EditorConfigHelper.Config.TreeBluePrints.FirstOrDefault(x => x.Name == (string)value).Id;
            }
            public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
            {
                var castedValue = value as TreeBluePrint;
                if (castedValue == null)
                {
                    var v = EditorConfigHelper.Config.TreeBluePrints.FirstOrDefault(x => x.Id == (int)value);
                    return v == null ? null : v.ToString();
                }
                else
                {
                    return castedValue.ToString();
                }
            }
            public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
            {
                return true;
            }
            public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
            {
                return true;
            }

        }

    }
}
