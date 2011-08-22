using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Utopia.Shared.Chunks.Entities.Inventory.Tools;
using Utopia.Shared.Structs;
using Utopia.Shared.Structs.Landscape;

namespace Utopia.Shared.Chunks.Entities.Inventory
{
    /// <summary>
    /// A Tool is something you can use
    /// </summary>
    public abstract class Tool : Item
    {

        //TODO RenderCubeProfile has interesting info for the server , isSolid, cubeFamily etc but has client rendering things too
        //player.cs already has buildingCubeId so passing it would be nice ( or having it as field of the building tool)
        public abstract ToolImpact Use(TerraCubeWithPosition pickedBlock, Location3<int>? newCubePlace,  TerraCube terraCube);

        //does the tool require player to have a block picked
        public bool NeedsPick { get; protected set; }

        protected Tool ()
        {
            NeedsPick = true;
            UniqueName = this.GetType().Name;
        }

    }
}
