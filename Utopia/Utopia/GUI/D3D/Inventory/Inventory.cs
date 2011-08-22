using System.Collections.Generic;
using System.Linq;
using Utopia.Shared.Chunks.Entities.Concrete;

//TODO state should be maintained on server,  move to shared project ?
namespace Utopia.Shared.Chunks.Entities.Inventory
{
    //based on http://code.google.com/p/rpginventory/source/browse

    public class PlayerInventory
    {

        public Armor HeadGear { get; set; }
        public Armor Torso { get; set; }
        public Armor Arms { get; set; }
        public Armor Hands { get; set; }
        public Armor Legs { get; set; }
        public Armor Feet { get; set; }
        public Item LeftRing { get; set; }
        public Item RightRing { get; set; }
        public Item NeckLace { get; set; }


        public BaseContainer Bag = new BaseContainer();

        public List<Tool> Toolbar = new List<Tool>();

        public Tool LeftTool { get; set; }
        public Tool RightTool { get; set; }

        public void ChangeTool(ref Tool item, int dir)
        {
            List<Tool> tools = Toolbar;
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
