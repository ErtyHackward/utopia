﻿
#region Using Statements
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


using System.Diagnostics;
using Utopia.Shared.Structs;
using Utopia.Shared.Structs.Landscape;

#endregion

namespace Utopia.Shared.Chunks.Entities.Inventory.Tools
{
    public class BlockAdder : Tool
    {
        public override ToolImpact Use(TerraCubeWithPosition pickedBlock, Location3<int>? newCubePlace, TerraCube terraCube)
        {
            if (newCubePlace.HasValue)
            {
                return new ToolImpact(new TerraCubeWithPosition(newCubePlace.Value, terraCube));
            }
            return new ToolImpact();//no impact when there is no available newCubePlace
        }

        public override EntityClassId ClassId
        {
            get { return EntityClassId.Sword; }
        }

        public override int MaxStackSize
        {
            get { return 1; }
        }
    }
}
