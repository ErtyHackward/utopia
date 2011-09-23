
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
    public class Wally : BlockAdder
    {
        //public override ToolImpact Use(TerraCubeWithPosition pickedBlock, Vector3I? newCubePlace, TerraCube terraCube)
        //{
        //    if (newCubePlace.HasValue)
        //    {
        //        List<TerraCubeWithPosition> poses = new List<TerraCubeWithPosition>();

        //        for (int x = newCubePlace.Value.X - 3; x < newCubePlace.Value.X + 3; x++)
        //        {
        //            for (int y = newCubePlace.Value.Y; y < newCubePlace.Value.Y + 5; y++)
        //            {

        //                Vector3I loc = new Vector3I(x, y, newCubePlace.Value.Z);
        //                poses.Add(new TerraCubeWithPosition(loc, terraCube));
        //            }
        //        }

        //        return new ToolImpact(poses.ToArray());
        //    }



        //    return new ToolImpact();//no impact when there is no available newCubePlace
        //}

        public override EntityClassId ClassId
        {
            get { return EntityClassId.None; }
        }

        public override int MaxStackSize
        {
            get { return 1; }
        }
    }
}
