using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Design;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Windows.Forms.Design;

namespace Utopia.Shared.Entities
{
    public class ModelSelector : UITypeEditor
    {
        public static List<string> Models = new List<string>();

        private ListView _list;

        private void Initialize()
        {
            _list = new ListView();
            _list.View = View.LargeIcon;

            var imgList = new ImageList();

            _list.LargeImageList = imgList;
            _list.SmallImageList = imgList;


            foreach (var model in Models)
            {
                imgList.Images.Add(Image.FromFile(model));

                var item = new ListViewItem();

                item.Text = Path.GetFileNameWithoutExtension(model);
                item.ImageIndex = imgList.Images.Count - 1;

                _list.Items.Add(item);
            }
        }

        public override object EditValue(System.ComponentModel.ITypeDescriptorContext context, IServiceProvider provider, object value)
        {
            if (_list == null)
                Initialize();

            var svc = provider.GetService(typeof(IWindowsFormsEditorService)) as IWindowsFormsEditorService;

            svc.DropDownControl(_list);

            return value;
        }

        public override UITypeEditorEditStyle GetEditStyle(System.ComponentModel.ITypeDescriptorContext context)
        {
            return UITypeEditorEditStyle.DropDown;
        }
    }
}
