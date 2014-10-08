using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Design;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Windows.Forms.Design;

namespace Utopia.Shared.Tools
{
    /// <summary>
    /// PropertyGrid editor component to show all possible models to use
    /// </summary>
    public class TextureSelector : UITypeEditor
    {
        public static Dictionary<string, Image> TextureIcons = new Dictionary<string, Image>();

        private IWindowsFormsEditorService _service;
        private ListView _list;

        private void Initialize()
        {
            if (_list != null)
            {
                _list.Click -= _list_Click;
            }

            _list = new ListView();
            _list.Height = 300;
            _list.MultiSelect = false;
            _list.View = View.Tile;
            _list.Click += _list_Click;

            var imgList = new ImageList();

            imgList.ImageSize = new Size(32, 32);
            imgList.ColorDepth = ColorDepth.Depth32Bit;
            _list.LargeImageList = imgList;
            _list.SmallImageList = imgList;

            foreach (var texture in TextureIcons)
            {
                imgList.Images.Add(texture.Value);
                var item = new ListViewItem();
                item.Text = texture.Key;
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

        public override bool GetPaintValueSupported(System.ComponentModel.ITypeDescriptorContext context)
        {
            return true;
        }

        public override void PaintValue(PaintValueEventArgs e)
        {
            if (e.Value == null)
                return;

            var value = (Utopia.Shared.Settings.TextureData.TextureMeta)e.Value;

            Image img;
            if (TextureIcons.TryGetValue(value.ToString(), out img))
            {
                e.Graphics.DrawImage(img, e.Bounds);
            }

            base.PaintValue(e);
        }

        //The returned Value of the Entity, in our case its the entitty name stored inside the object Tag.
        public override object EditValue(System.ComponentModel.ITypeDescriptorContext context, IServiceProvider provider, object value)
        {
            if (_list == null) Initialize();
            _service = provider.GetService(typeof(IWindowsFormsEditorService)) as IWindowsFormsEditorService;

            var v = value as Utopia.Shared.Settings.TextureData.TextureMeta;
            if (v != null && v.Name != null)
            {
                var currentItem = _list.FindItemWithText(((Utopia.Shared.Settings.TextureData.TextureMeta)value).Name);

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
            Utopia.Shared.Settings.TextureData.TextureMeta meta = new Settings.TextureData.TextureMeta();
            if (_list.Tag != null)
            {
                if (_list.Tag.GetType() == typeof(string))
                {
                    meta.Name = (string)_list.Tag;
                }
                else
                {
                    meta.Name = ((Utopia.Shared.Settings.TextureData.TextureMeta)_list.Tag).Name;
                    meta.AnimationFrames = 1;
                }
            }

            return meta;
        }

        public override UITypeEditorEditStyle GetEditStyle(System.ComponentModel.ITypeDescriptorContext context)
        {
            return UITypeEditorEditStyle.DropDown;
        }
    }
}
