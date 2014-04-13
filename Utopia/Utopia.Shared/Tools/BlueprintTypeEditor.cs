using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Design;
using System.Linq;
using System.Windows.Forms;
using System.Windows.Forms.Design;
using Utopia.Shared.Configuration;
using Utopia.Shared.Entities.Interfaces;

namespace Utopia.Shared.Tools
{
    /// <summary>
    /// PropertyGrid editor component to show all possible models to use
    /// </summary>
    public class BlueprintTypeEditor : UITypeEditor
    {
        public static WorldConfiguration Configuration;
        public static Dictionary<string, Image> Images;
        
        private IWindowsFormsEditorService _service;
        private ListView _list;

        private string GetVoxelEntityImgName(IVoxelEntity voxelEntity)
        {
            if (!string.IsNullOrEmpty(voxelEntity.ModelName))
            {
                if (!string.IsNullOrEmpty(voxelEntity.ModelState))
                {
                    return voxelEntity.ModelName + ":" + voxelEntity.ModelState;
                }
                return voxelEntity.ModelName;
            }

            return null;
        }

        private void Initialize()
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

            var groupBlocks = new ListViewGroup("Blocks");
            _list.Groups.Add(groupBlocks);
            var groupEntities = new ListViewGroup("Entities");
            _list.Groups.Add(groupEntities);

            foreach (var blockProfile in Configuration.BlockProfiles.Skip(1))
            {
                Image icon;
                if (Images.TryGetValue("CubeResource_" + blockProfile.Name, out icon))
                {
                    imgList.Images.Add(icon);
                    var item = new ListViewItem();
                    item.Text = blockProfile.Name;
                    item.ImageIndex = imgList.Images.Count - 1;
                    item.Tag = (ushort)blockProfile.Id;
                    _list.Items.Add(item).Group = groupBlocks;
                }
            }

            foreach (var pair in Configuration.BluePrints.OrderBy(p => p.Value.Name))
            {
                var blueprint = pair.Value;

                Image icon = null;


                var iconName = GetVoxelEntityImgName((IVoxelEntity)blueprint);
                if (!string.IsNullOrEmpty(iconName))
                    Images.TryGetValue(iconName, out icon);

                if (icon != null)
                    imgList.Images.Add(icon);
                var item = new ListViewItem();
                item.Text = blueprint.Name;
                if (icon != null)
                    item.ImageIndex = imgList.Images.Count - 1;
                item.Tag = pair.Key;
                _list.Items.Add(item).Group = groupEntities;
            }

        }

        public override bool IsDropDownResizable
        {
            get { return true; }
        }

        void _list_Click(object sender, EventArgs e)
        {
            if (_list.SelectedItems.Count > 0)
            {
                _list.Tag = _list.SelectedItems[0].Tag;
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

            var value = (ushort)e.Value;

            string iconName;

            if (value < 256)
            {
                iconName = "CubeResource_" + Configuration.BlockProfiles[value].Name;
            }
            else
            {
                iconName = GetVoxelEntityImgName((IVoxelEntity)Configuration.BluePrints[value]);
            }

            Image img;
            if (Images.TryGetValue(iconName, out img))
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
                Initialize();
            }
            _service = provider.GetService(typeof(IWindowsFormsEditorService)) as IWindowsFormsEditorService;

            if (value != null)
            {
                var currentItem = _list.Items.Cast<ListViewItem>().FirstOrDefault( i => (ushort)i.Tag == (ushort)value);

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