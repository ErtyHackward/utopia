using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing.Design;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Windows.Forms.Design;
using Utopia.Shared.Configuration;
using Utopia.Shared.Services;

namespace Utopia.Shared.Chunks
{
    public partial class ChunkSpawnableEntity
    {
        [TypeConverter(typeof(EntityConverter))] //Display Cube List
        [DisplayName("Entity")]
        public string EntityName
        {
            //When first loaded set property with the first item in the rule list.
            get
            {
                if (BluePrintId == 0) BluePrintId = EditorConfigHelper.Config.BluePrints.Min(x => x.Value.BluePrintId);
                return EditorConfigHelper.Config.BluePrints[BluePrintId].Name;
            }
            set
            {
                //Get ID from name, name must be unic !
                BluePrintId = EditorConfigHelper.Config.BluePrints.Values.First(x => x.Name == value).BluePrintId;
            }
        }

        internal class EntityConverter : StringConverter
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
                return new StandardValuesCollection(EditorConfigHelper.Config.BluePrints.Values.Select(x => x.Name).OrderBy(x => x).ToList());
            }
        }

        internal class SpawningSeasonsEditor : UITypeEditor
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
                    foreach (var s in weatherService.Seasons)
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
