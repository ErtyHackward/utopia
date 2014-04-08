using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing.Design;
using System.Linq;
using System.Text;
using System.Windows.Forms.Design;
using Utopia.Shared.Configuration;

namespace Utopia.Shared.Services
{
    public partial class Season
    {
        internal class SeasonsEditor : UITypeEditor
        {
            private CheckValuesEditorControl<string> editor = null;
            private List<string> _seasons;

            // we tell to the designer host that this editor is a DropDown editor
            public override UITypeEditorEditStyle GetEditStyle(System.ComponentModel.ITypeDescriptorContext context)
            {
                return UITypeEditorEditStyle.DropDown;
            }

            public override object EditValue(ITypeDescriptorContext context, IServiceProvider provider, object value)
            {
                List<string> values = value as List<string>;

                _seasons = new List<string>();
                var weatherService = EditorConfigHelper.Config.Services.OfType<WeatherService>().FirstOrDefault();
                if (weatherService != null)
                {
                    foreach (var s in weatherService.TimeConfiguration.Seasons)
                    {
                        _seasons.Add(s.Name);
                    }
                }

                if (provider != null)
                {
                    // use windows forms editor service to show drop down
                    IWindowsFormsEditorService edSvc = provider.GetService(typeof(IWindowsFormsEditorService))
                            as IWindowsFormsEditorService;
                    if (edSvc == null) return value;

                    if (editor == null)
                        editor = new CheckValuesEditorControl<string>();

                    // prepare list
                    editor.Begin(edSvc, _seasons, values);

                    // show drop down now
                    edSvc.DropDownControl(editor);

                    // now we take the result
                    value = new List<string>(editor.GetSelectedValues());

                    // reset
                    editor.End();
                }

                return value;
            }
        }
    }
}
