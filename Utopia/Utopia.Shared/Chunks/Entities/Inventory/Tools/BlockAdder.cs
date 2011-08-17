
#region Using Statements
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


using System.Diagnostics;
#endregion

namespace Utopia.Shared.Chunks.Entities.Inventory.Tools
{
    public class BlockAdder : Tool
    {
     
        /*
       
        public bool Use()
        {
            
              
            if (currentSelectedAdjacent.HasValue)
            {
                player.world.SetBlockAndReBuild(currentSelectedAdjacent.Value.Position, blockType);
                return true;
            }
            else return false;
        }

        public override void switchType(int delta)
        {
            
            if (delta >= 120)
            {
                blockType++;
                if (blockType == BlockType.MAXIMUM) blockType = BlockType.MAXIMUM - 1;
                this.IconSourceRectangle = TextureHelper.GetSourceRectangle(blockType);
            }
            else if (delta <= -120)
            {
                blockType--;
                if (blockType == BlockType.None) blockType = (BlockType)1;
                this.IconSourceRectangle = TextureHelper.GetSourceRectangle(blockType);
            }

        
        }*/
    }
}
