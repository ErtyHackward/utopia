using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Design;
using System.Linq;
using System.Windows.Forms;
using System.Windows.Forms.Design;
using Utopia.Shared.Entities;
using Utopia.Shared.Entities.Concrete;
using Utopia.Shared.Entities.Inventory;

namespace Utopia.Shared.Tools
{
    /// <summary>
    /// PropertyGrid editor component to show all possible models to use
    /// </summary>
    public class ModelSelector : UITypeEditor
    {
        public static Dictionary<string, Image> Models = new Dictionary<string, Image>();
        public static Dictionary<string, Image> MultiStatesModels = new Dictionary<string, Image>();

        private IWindowsFormsEditorService _service;
        private ListView _list;

        private void Initialize(bool isMultiStatesModels = false)
        {
            if (_list != null)
            {
                _list.Click -= _list_Click;
            }

            _list = new ListView();
            _list.Height = 300;
            _list.MultiSelect = false;
            _list.View = View.LargeIcon;
            _list.Click += _list_Click;

            var imgList = new ImageList();

            imgList.ImageSize = new Size(32, 32);
            imgList.ColorDepth = ColorDepth.Depth32Bit;
            _list.LargeImageList = imgList;
            _list.SmallImageList = imgList;


            foreach (var model in (isMultiStatesModels ? MultiStatesModels : Models).OrderBy(p => p.Key))
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

        public override bool IsDropDownResizable
        {
            get { return true; }
        }

        public override bool GetPaintValueSupported(System.ComponentModel.ITypeDescriptorContext context)
        {
            return true;
        }

        public override void PaintValue(PaintValueEventArgs e)
        {
            if (e.Value == null)
                return;

            var value = (string)e.Value;

            Image img;
            if (Models.TryGetValue(value, out img))
            {
                e.Graphics.DrawImage(img, e.Bounds);
            }

            base.PaintValue(e);
        }

        //The returned Value of the Entity, in our case its the entitty name stored inside the object Tag.
        public override object EditValue(System.ComponentModel.ITypeDescriptorContext context, IServiceProvider provider, object value)
        {
            if (_list == null)
            {
                bool isGrowingModel = context.Instance.GetType().IsSubclassOf(typeof(GrowingEntity));
                Initialize(isGrowingModel);
            }
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
