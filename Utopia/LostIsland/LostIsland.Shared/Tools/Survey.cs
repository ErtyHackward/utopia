using Utopia.Shared.Chunks.Entities.Inventory;

namespace LostIsland.Shared.Tools
{

    //Survey gets you the number of blocks of selected cubeid under the selection cube. 
    public class Survey : Tool
    {        
        public override ushort ClassId
        {
            get { return LostIslandEntityClassId.Survey; }
        }

        public override int MaxStackSize
        {
            get { return 1; }
        }

        public override Utopia.Shared.Chunks.Entities.Interfaces.IToolImpact Use(bool runOnServer = false)
        {
            throw new System.NotImplementedException();
        }

        public override void Rollback(Utopia.Shared.Chunks.Entities.Interfaces.IToolImpact impact)
        {
            throw new System.NotImplementedException();
        }
    }
}
