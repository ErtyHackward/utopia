using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Utopia.Shared.Chunks.Entities.Concrete;
using Utopia.Shared.Chunks.Entities.Inventory;

namespace Utopia.Shared.Chunks.Entities.Inventory
{
    //based on http://code.google.com/p/rpginventory/source/browse

    public class PlayerInventory
    {

        public Armor headGear;
        public Armor torso;
        public Armor arms;
        public Armor hands;
        public Armor legs;
        public Armor feet;
        public Item leftRing;
        public Item rightRing;
        public Item neckLace;


        public ContainerEntity bag = new ContainerEntity();

        public List<Tool> toolbar = new List<Tool>();


        public void changeTool(ref Tool item, int dir)
        {
            List<Tool> tools = toolbar;
            int i = tools.IndexOf(item);
            if (dir > 0)
            {
                if (i + dir == tools.Count) item = tools.First();
                else item = tools[i + 1];
            }
            else
            {
                if (i + dir == -1) item = tools.Last();
                else item = tools[i - 1];
            }

        }

    }

   

}
