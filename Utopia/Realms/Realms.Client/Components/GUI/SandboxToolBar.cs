using System.Collections.Generic;
using System.Linq;
using S33M3CoreComponents.GUI.Nuclex;
using S33M3CoreComponents.GUI.Nuclex.Controls;
using S33M3CoreComponents.GUI.Nuclex.Controls.Desktop;
using S33M3CoreComponents.GUI.Nuclex.Visuals.Flat.Interfaces;
using S33M3CoreComponents.Inputs;
using S33M3CoreComponents.Sprites2D;
using S33M3DXEngine;
using S33M3Resources.Structs;
using Utopia.Entities;
using Utopia.GUI.Inventory;
using Utopia.Shared.Entities;
using Utopia.Shared.Entities.Dynamic;
using Utopia.Shared.Entities.Interfaces;

namespace Realms.Client.Components.GUI
{
    public class SandboxToolBar : ToolBarUi
    {
        public override int DrawGroupId
        {
            get
            {
                return 1;
            }
        }

        readonly SpriteTexture _stBackground;
        readonly SpriteTexture _stToolbarSlot;
        readonly SpriteTexture _stToolbatSlotHover;

        private readonly List<LabelControl> _numbersLabels = new List<LabelControl>();

        public List<LabelControl> NumbersLabels
        {
            get { return _numbersLabels; }
        }

        public SandboxToolBar(D3DEngine engine, PlayerCharacter player, IconFactory iconFactory, InputsManager inputManager, EntityFactory factory) : 
            base(player, iconFactory, inputManager, factory)
        {
            _stBackground       = new SpriteTexture(engine.Device, @"Images\Inventory\toolbar_bg.png");
            _stToolbarSlot      = new SpriteTexture(engine.Device, @"Images\Inventory\toolbar_slot.png");
            _stToolbatSlotHover = new SpriteTexture(engine.Device, @"Images\Inventory\toolbar_slot_active.png");

            Background = _stBackground;
            DisplayBackground = true;

            Bounds = new UniRectangle(0, new UniScalar(0.8f, 0), new UniScalar(1, 0), new UniScalar(0.21f, 0));

            var offset = new Vector2I(50, 48);
            var size = new Vector2I(57, 57);

            int buttonIndex = 0;

            foreach (var entity in factory.Config.BluePrints.Values.Where(e => e.ShowInToolbar))
            {
                var button = new ButtonControl { 
                    Bounds = new UniRectangle(
                        new UniScalar(0.1f, 0), 
                        new UniScalar(0.1f, 0), 
                        new UniScalar(0.8f, 0), 
                        new UniScalar(0.8f, 0))
                };

                int arrayIndex;
                SpriteTexture texture;

                iconFactory.Lookup((IItem)entity, out texture, out arrayIndex);

                button.CusomImageLabel = texture;

                Children.Add(button);
                buttonIndex++;
            }

            for (int i = 0; i < _toolbarSlots.Count; i++)
            {
                var inventoryCell = _toolbarSlots[i];
                inventoryCell.Bounds = new UniRectangle(offset.X + (size.X) * i, offset.Y, 42, 42);
                inventoryCell.CustomBackground = _stToolbarSlot;
                inventoryCell.CustomBackgroundHover = _stToolbatSlotHover;
                inventoryCell.DrawIconsGroupId = 3;
                inventoryCell.DrawIconsActiveCellId = 4;
                inventoryCell.Color = new ByteColor(255, 255, 255, 120);

                var label = new LabelControl
                    {
                        Text = ( i + 1 ).ToString(),
                        IsClickTransparent = true,
                        Color = new ByteColor(255, 255, 255, 80),
                        Bounds = new UniRectangle(offset.X + ( size.X ) * i + 18, offset.Y + 42, 10, 10),
                        IsVisible = inventoryCell.Slot != null
                    };
                
                _numbersLabels.Add(label);
                Children.Add(label);
            }

            SlotChanged += SandboxToolBar_SlotChanged;

        }

        void SandboxToolBar_SlotChanged(object sender, InventoryWindowCellMouseEventArgs e)
        {
            if (!DisplayBackground)
            {
                var index = _toolbarSlots.IndexOf(e.Cell);
                _numbersLabels[index].IsVisible = e.Cell.Slot != null;
            }
        }
    }

    public class SandboxToolBarRenderer : IFlatControlRenderer<SandboxToolBar>
    {
        public void Render(SandboxToolBar control, IFlatGuiGraphics graphics)
        {
            if (control.DisplayBackground)
            {
                var absoluteBounds = control.GetAbsoluteBounds();
                graphics.DrawElement("toolbar", ref absoluteBounds);
            }
        }
    }
}
