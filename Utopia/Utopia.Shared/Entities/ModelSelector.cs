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

        private IWindowsFormsEditorService _service;
        private ListView _list;

        private void Initialize()
        {
            if (_list != null)
            {
                _list.Click -= _list_Click;
            }

            _list = new ListView();
            _list.View = View.LargeIcon;
            _list.Click += _list_Click;

            var imgList = new ImageList();

            imgList.ImageSize = new Size(32, 32);
            imgList.ColorDepth = ColorDepth.Depth32Bit;
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

        void _list_Click(object sender, EventArgs e)
        {
            if (_list.SelectedItems.Count > 0)
            {
                _list.Tag = _list.SelectedItems[0].Text;
                _service.CloseDropDown();
            }
        }

        public override object EditValue(System.ComponentModel.ITypeDescriptorContext context, IServiceProvider provider, object value)
        {
            if (_list == null)
                Initialize();

            _service = provider.GetService(typeof(IWindowsFormsEditorService)) as IWindowsFormsEditorService;

            _service.DropDownControl(_list);
            
            return _list.Tag;
        }

        public override UITypeEditorEditStyle GetEditStyle(System.ComponentModel.ITypeDescriptorContext context)
        {
            return UITypeEditorEditStyle.DropDown;
        }
    }
}
