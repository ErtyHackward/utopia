using System;
using System.Collections.Generic;
using System.Drawing.Design;
using System.Linq;
using System.Text;

namespace Utopia.Shared.Tools
{
    /// <summary>
    /// Allows to pick a list of entities and saves it in list of KeyValuePair&lt;int,int&gt;
    /// </summary>
    public class EntitiesListEditor : UITypeEditor
    {
        

        public override UITypeEditorEditStyle GetEditStyle(System.ComponentModel.ITypeDescriptorContext context)
        {
            return UITypeEditorEditStyle.Modal;
        }
    }
}
