using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Design;
using System.Linq;
using System.Windows.Forms;
using System.Windows.Forms.Design;
using S33M3Resources.Structs;
using Utopia.Shared.Configuration;
using Utopia.Shared.Entities;
using Utopia.Shared.Entities.Concrete;
using Utopia.Shared.Entities.Interfaces;
using Utopia.Shared.Settings;

namespace Utopia.Shared.Tools
{
    public class BlueprintTypeEditor : BlueprintTypeEditor<object>
    {

    }

    /// <summary>
    /// PropertyGrid editor component to show all possible models to use
    /// </summary>
    public class BlueprintTypeEditor<T> : UITypeEditor
    {      
        private IWindowsFormsEditorService _service;
        private ListView _list;
        private Type typeParameterType = typeof(T);

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

            if (typeParameterType.IsRelatives(typeof(BlockProfile)))
            {
                var groupBlocks = new ListViewGroup("Blocks");
                _list.Groups.Add(groupBlocks);
                

                foreach (var blockProfile in BlueprintTextHintConverter.Configuration.BlockProfiles.Skip(1))
                {
                    Image icon;
                    if (blockProfile != null && BlueprintTextHintConverter.Images.TryGetValue("CubeResource_" + blockProfile.Name, out icon))
                    {
                        imgList.Images.Add(icon);
                        var item = new ListViewItem();
                        item.Text = blockProfile.Name;
                        item.ImageIndex = imgList.Images.Count - 1;
                        item.Tag = (ushort)blockProfile.Id;
                        _list.Items.Add(item).Group = groupBlocks;
                    }
                }
            }

            if (typeParameterType.IsRelatives(typeof(Entity)))
            {
                var groupEntities = new ListViewGroup("Entities");
                _list.Groups.Add(groupEntities);
                
                foreach (var pair in BlueprintTextHintConverter.Configuration.BluePrints.OrderBy(p => p.Value.Name))
                {
                    var blueprint = pair.Value;

                    if (!typeParameterType.IsInstanceOfType(blueprint))
                        continue;

                    Image icon = null;

                    var iconName = GetVoxelEntityImgName((IVoxelEntity)blueprint);
                    if (!string.IsNullOrEmpty(iconName))
                        BlueprintTextHintConverter.Images.TryGetValue(iconName, out icon);

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

            ushort value;

            if (e.Value is byte)
            {
                value = (byte)e.Value;
            }
            else
            {
                value = (ushort)e.Value;
            }

            string iconName;

            if (value < 256)
            {
                iconName = "CubeResource_" + BlueprintTextHintConverter.Configuration.BlockProfiles[value].Name;
            }
            else
            {
                iconName = GetVoxelEntityImgName((IVoxelEntity)BlueprintTextHintConverter.Configuration.BluePrints[value]);
            }

            Image img;
            if (!string.IsNullOrEmpty(iconName))
                if (BlueprintTextHintConverter.Images.TryGetValue(iconName, out img))
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
                ListViewItem currentItem;
                if (value is byte)
                {
                    currentItem = _list.Items.Cast<ListViewItem>().FirstOrDefault(i => (ushort)i.Tag == (byte)value);
                }
                else
                {
                    currentItem = _list.Items.Cast<ListViewItem>().FirstOrDefault(i => (ushort)i.Tag == (ushort)value);
                }

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

            if (_list.Tag is ushort && (ushort)_list.Tag < 256)
            {
                return (byte)(ushort)_list.Tag;
            }
            return _list.Tag;
        }
        
        public override UITypeEditorEditStyle GetEditStyle(System.ComponentModel.ITypeDescriptorContext context)
        {
            return UITypeEditorEditStyle.DropDown;
        }
    }
}