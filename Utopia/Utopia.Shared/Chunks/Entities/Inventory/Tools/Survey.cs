using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Utopia.Shared.Structs;
using Utopia.Shared.Structs.Landscape;

namespace Utopia.Shared.Chunks.Entities.Inventory.Tools
{

    //Survey gets you the number of blocks of selected cubeid under the selection cube. 
    public class Survey : Tool
    {
        public override ToolImpact Use(TerraCubeWithPosition pickedBlock, Location3<int>? newCubePlace, TerraCube terraCube1)
        {
            int count = 0;

            //This shows tool code has to run on the server : Shared project is not enough you dont'have access to the world cubes
            //Other tools currently work because they affect the world client side with entityEffect 

            /*           if (newCubePlace.HasValue)
                       {
                           for (int y = newCubePlace.Value.Y; y > 0; y--)
                           {
                               TerraCube sample = Landscape.getCube(newCubePlace.Value.X, newCubePlace.Value.Y, newCubePlace.Value.Z);
                               if (sample.Id==terraCube.Id) count++;
                           }
                 
                       }
                       return new ToolImpact(String.Format("Survey result : {0} cubes of type {1} ",count, terraCube));
               */
            return new ToolImpact(String.Format("Survey is broken, should run on server, but message system works"));
               
        }
    }
}
