using System.Drawing;
using S33M3CoreComponents.Inputs;
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
        public PlayerInventory(CharacterEntity character, IconFactory iconFactory, Point windowStartPosition, Point gridOffset, InputsManager inputManager) : 
            base(character, iconFactory, windowStartPosition, gridOffset, inputManager)
        {

        }
    }
}
