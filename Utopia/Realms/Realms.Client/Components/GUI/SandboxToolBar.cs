using S33M3CoreComponents.GUI.Nuclex;
using S33M3CoreComponents.Inputs;
using S33M3CoreComponents.Sprites2D;
using S33M3DXEngine;
using S33M3Resources.Structs;
using Utopia.Entities;
using Utopia.GUI.Inventory;
using Utopia.Shared.Entities;
using Utopia.Shared.Entities.Dynamic;

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
        
        public SandboxToolBar(D3DEngine engine, PlayerCharacter player, IconFactory iconFactory, InputsManager inputManager, EntityFactory factory) : 
            base(player, iconFactory, inputManager, factory)
        {
            _stBackground       = new SpriteTexture(engine.Device, @"Images\Inventory\toolbar_bg.png");
            _stToolbarSlot      = new SpriteTexture(engine.Device, @"Images\Inventory\toolbar_slot.png");
            _stToolbatSlotHover = new SpriteTexture(engine.Device, @"Images\Inventory\toolbar_slot_active.png");

            background = _stBackground;

            Bounds = new UniRectangle(0, 0, 656, 116);

            var offset = new Vector2I(50, 48);
            var size = new Vector2I(57, 57);

            for (int i = 0; i < _toolbarSlots.Count; i++)
            {
                var inventoryCell = _toolbarSlots[i];
                inventoryCell.Bounds = new UniRectangle(offset.X + (size.X) * i, offset.Y, 42, 42);
                inventoryCell.CustomBackground = _stToolbarSlot;
                inventoryCell.CustomBackgroundHover = _stToolbatSlotHover;
                inventoryCell.DrawIconsGroupId = 3;
                inventoryCell.DrawIconsActiveCellId = 4;
            }
        }
    }
}
