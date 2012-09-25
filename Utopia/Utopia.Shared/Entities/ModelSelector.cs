using System;
using System.Collections.Generic;
using System.Drawing.Design;
using System.Linq;
using System.Text;
using System.Windows.Forms.Design;

namespace Utopia.Shared.Entities
{
    public class ModelSelector : UITypeEditor
    {
        public override object EditValue(System.ComponentModel.ITypeDescriptorContext context, IServiceProvider provider, object value)
        {
            var svc = provider.GetService(typeof(IWindowsFormsEditorService)) as IWindowsFormsEditorService;

            return value;
        }

        public override UITypeEditorEditStyle GetEditStyle(System.ComponentModel.ITypeDescriptorContext context)
        {
            return UITypeEditorEditStyle.DropDown;
        }
    }
}
