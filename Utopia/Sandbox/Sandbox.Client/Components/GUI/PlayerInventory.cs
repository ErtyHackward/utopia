using System.Drawing;
using S33M3CoreComponents.Inputs;
using S33M3CoreComponents.Sprites;
using S33M3DXEngine;
using Utopia.Entities;
using Utopia.GUI.Inventory;
using Utopia.Shared.Entities.Dynamic;

namespace Sandbox.Client.Components.GUI
{
    /// <summary>
    /// Sandbox inventory window
    /// </summary>
    public class PlayerInventory : CharacterInventory
    {
        private SpriteTexture _stInventoryWindow;
        private SpriteTexture _stInventoryCloseButton;
        private SpriteTexture _stInventoryCloseButtonHover;
        private SpriteTexture _stInventoryCloseButtonDown;
        private SpriteTexture _stInventoryCloseButtonLabel;

        public PlayerInventory(D3DEngine engine, PlayerCharacter character, IconFactory iconFactory, InputsManager inputManager) : 
            base(character, iconFactory, new Point(100,100), new Point(100,100), inputManager)
        {
            _stInventoryWindow              = new SpriteTexture(engine.Device, @"Images\Inventory\inventory_window.png");
            _stInventoryCloseButton         = new SpriteTexture(engine.Device, @"Images\Inventory\inventory_close.png");
            _stInventoryCloseButtonHover    = new SpriteTexture(engine.Device, @"Images\Inventory\inventory_close_hover.png");
            _stInventoryCloseButtonDown     = new SpriteTexture(engine.Device, @"Images\Inventory\inventory_close_down.png");
            _stInventoryCloseButtonLabel    = new SpriteTexture(engine.Device, @"Images\Inventory\inventory_close_label.png");

            CustomWindowImage = _stInventoryWindow;
            Bounds.Size = new S33M3CoreComponents.GUI.Nuclex.UniVector(812, 526);
        }
    }
}
