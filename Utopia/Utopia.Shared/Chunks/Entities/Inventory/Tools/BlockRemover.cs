using System;
using System.Collections.Generic;
using Utopia.Shared.Cubes;

namespace Utopia.Shared.Chunks.Entities.Inventory.Tools
{
    /// <summary>
    /// BlockRemover base class can remove anyting
    /// </summary>
    public abstract class BlockRemover : Tool
    {
        
        protected HashSet<Byte> RemoveableCubeIds = new HashSet<byte>();
        //TODO Gameplay decision : in xna code it was even _selectable blocks , you could not even select without the good tool (= not show the select box overlay)

        protected BlockRemover()
        {
            // <evil> looks crazy isn't it? maybe change CubeId to be enum?
            RemoveableCubeIds.Add(CubeId.Stone);
            RemoveableCubeIds.Add(CubeId.Dirt);
            RemoveableCubeIds.Add(CubeId.Grass);
            RemoveableCubeIds.Add(CubeId.WoodPlank);
            RemoveableCubeIds.Add(CubeId.Water);
            RemoveableCubeIds.Add(CubeId.LightRed);
            RemoveableCubeIds.Add(CubeId.LightGreen);
            RemoveableCubeIds.Add(CubeId.LightBlue);
            RemoveableCubeIds.Add(CubeId.LightYellow);
            RemoveableCubeIds.Add(CubeId.LightViolet);
            RemoveableCubeIds.Add(CubeId.LightWhite);
            RemoveableCubeIds.Add(CubeId.Stone2);
            RemoveableCubeIds.Add(CubeId.Stone3);
            RemoveableCubeIds.Add(CubeId.Rock);
            RemoveableCubeIds.Add(CubeId.Sand);
            RemoveableCubeIds.Add(CubeId.Gravel);
            RemoveableCubeIds.Add(CubeId.Trunk);
            RemoveableCubeIds.Add(CubeId.Minerai1);
            RemoveableCubeIds.Add(CubeId.Minerai2);
            RemoveableCubeIds.Add(CubeId.Minerai3);
            RemoveableCubeIds.Add(CubeId.Minerai4);
            RemoveableCubeIds.Add(CubeId.Minerai5);
            RemoveableCubeIds.Add(CubeId.Stone4);
            RemoveableCubeIds.Add(CubeId.Minerai6);
            RemoveableCubeIds.Add(CubeId.Brick);
            RemoveableCubeIds.Add(CubeId.WaterSource);
            // </evil>
        }
    }
}
