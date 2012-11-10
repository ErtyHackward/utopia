using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Design;
using System.IO;
using System.Windows.Forms;
using System.Windows.Forms.Design;

namespace Utopia.Shared.Tools
{
    /// <summary>
    /// PropertyGrid editor component to show all possible models to use
    /// </summary>
    public class ModelSelector : UITypeEditor
    {
        public static Dictionary<string, Image> Models = new Dictionary<string, Image>();

        private IWindowsFormsEditorService _service;
        private ListView _list;

        private void Initialize()
        {
            if (_list != null)
            {
                _list.Click -= _list_Click;
            }

            _list = new ListView();
            _list.MultiSelect = false;
            _list.View = View.LargeIcon;
            _list.Click += _list_Click;

            var imgList = new ImageList();

            imgList.ImageSize = new Size(32, 32);
            imgList.ColorDepth = ColorDepth.Depth32Bit;
            _list.LargeImageList = imgList;
            _list.SmallImageList = imgList;


            foreach (var model in Models)
            {
                imgList.Images.Add(model.Value);
                var item = new ListViewItem();
                item.Text = model.Key;
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

        //The returned Value of the Entity, in our case its the entitty name stored inside the object Tag.
        public override object EditValue(System.ComponentModel.ITypeDescriptorContext context, IServiceProvider provider, object value)
        {
            if (_list == null) Initialize();
            _service = provider.GetService(typeof(IWindowsFormsEditorService)) as IWindowsFormsEditorService;

            if (value != null)
            {
                var currentItem = _list.FindItemWithText((string)value);

                if (currentItem != null)
                {
                    currentItem.Selected = true;
                    _list.Tag = value;
                }
            }
            else
            {
                // clear selection;
                if (_list.SelectedItems.Count > 0)
                {
                    _list.SelectedItems[0].Selected = false;
                }
            }


            _service.DropDownControl(_list);
            return _list.Tag;
        }

        public override UITypeEditorEditStyle GetEditStyle(System.ComponentModel.ITypeDescriptorContext context)
        {
            return UITypeEditorEditStyle.DropDown;
        }
    }
}
