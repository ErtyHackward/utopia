using System.Drawing;
using S33M3CoreComponents.Inputs;
using S33M3DXEngine;
using Utopia.Entities;
using Utopia.GUI.Inventory;
using Utopia.Shared.Entities.Dynamic;

namespace Realms.Client.Components.GUI.Inventory
{
    public class CraftingInventory : CraftingWindow
    {
        private readonly D3DEngine _engine;
        private readonly SandboxCommonResources _commonResources;

        public CraftingInventory(D3DEngine engine, PlayerCharacter character, IconFactory iconFactory, InputsManager inputManager, SandboxCommonResources commonResources) :
            base(character.Inventory, iconFactory, new Point(200, 120), new Point(270, 50), inputManager)
        {
            _engine = engine;
            _commonResources = commonResources;
        }
    }
}
