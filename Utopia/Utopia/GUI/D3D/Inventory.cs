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

        public Armor headGear { get; set; }
        public Armor torso { get; set; }
        public Armor arms { get; set; }
        public Armor hands { get; set; }
        public Armor legs { get; set; }
        public Armor feet { get; set; }
        public Item leftRing { get; set; }
        public Item rightRing { get; set; }
        public Item neckLace { get; set; }


        public ContainerEntity bag = new ContainerEntity();

        public List<Tool> toolbar = new List<Tool>();

        public Tool LeftTool { get; set; }
        public Tool RightTool { get; set; }

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
