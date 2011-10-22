using Nuclex.UserInterface;
using Nuclex.UserInterface.Controls;
using Nuclex.UserInterface.Controls.Desktop;
using Utopia.Entities;
using Utopia.Shared.Entities.Interfaces;
using Utopia.Shared.Entities.Inventory;

namespace Utopia.GUI.D3D.Inventory
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
                    _nameLabel.Text = value.DisplayName;
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

        public ItemInfoWindow(IconFactory iconFactory)
        {
            _iconFactory = iconFactory;
            Title = "Information";
            Bounds = new UniRectangle(700, 120, 200, 240);

            _cell = new InventoryCell(null, _iconFactory, new Shared.Structs.Vector2I())
                        {
                            DrawCellBackground = false,
                            Bounds = new UniRectangle(10, 30, 64, 64)
                        };

            _nameLabel = new LabelControl
                             {
                                 Bounds = new UniRectangle(80, 50, 100, 20)
                             };

            _descriptionLabel = new LabelControl
                                    {
                                        Bounds = new UniRectangle(10, 80, 20, 100)
                                    };

            Children.Add(_descriptionLabel);
            Children.Add(_nameLabel);
            Children.Add(_cell);
        }
    }
}
