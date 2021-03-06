﻿using Utopia.Entities;
using Utopia.Shared.Entities.Interfaces;
using Utopia.Shared.Entities.Inventory;
using S33M3CoreComponents.GUI.Nuclex.Controls.Desktop;
using S33M3CoreComponents.GUI.Nuclex.Controls;
using S33M3CoreComponents.GUI.Nuclex;
using S33M3Resources.Structs;
using S33M3CoreComponents.Inputs;

namespace Utopia.GUI.Inventory
{
    public class ItemInfoWindow : WindowControl
    {
        private readonly IconFactory _iconFactory;
        private readonly InventoryCell _cell;
        private readonly LabelControl _nameLabel;
        private readonly LabelControl _descriptionLabel;

        private IItem _activeItem;

        public IItem ActiveItem
        {
            get { return _activeItem; }
            set
            {
                if (value != null)
                {
                    _cell.Slot = new ContainedSlot { Item = value };
                    _nameLabel.Text = value.Name;
                    _descriptionLabel.Text = value.Description;
                }
                else
                {
                    _cell.Slot = null;
                    _nameLabel.Text = string.Empty;
                    _descriptionLabel.Text = string.Empty;
                }
                _activeItem = value;
            }
        }

        public ItemInfoWindow(IconFactory iconFactory, InputsManager inputManager)
        {
            _iconFactory = iconFactory;
            Title = "Information";
            Bounds = new UniRectangle(700, 120, 200, 240);

            _cell = new InventoryCell(null, _iconFactory, new Vector2I(), inputManager)
                        {
                            DrawCellBackground = false,
                            Bounds = new UniRectangle(10, 30, 64, 64)
                        };

            _nameLabel = new LabelControl
                             {
                                 Bounds = new UniRectangle(80, 50, 100, 20),
                                 Autosizing = true
                             };

            _descriptionLabel = new LabelControl
                                    {
                                        Bounds = new UniRectangle(10, 80, 180, 100),
                                        Autosizing = true
                                    };

            Children.Add(_descriptionLabel);
            Children.Add(_nameLabel);
            Children.Add(_cell);
        }
    }
}
