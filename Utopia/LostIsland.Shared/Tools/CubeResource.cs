using Utopia.Shared.Chunks.Entities.Inventory;
using Utopia.Shared.Interfaces;
using Utopia.Shared.Chunks.Entities.Inventory.Tools;

namespace LostIsland.Shared.Tools
{
    public class CubeResource : Tool
    {
        private readonly ILandscapeManager2D _landscapeManager;
        public byte CubeId;

        public override int MaxStackSize
        {
            get { return 999; }
        }

        public override ushort ClassId
        {
            get { return LostIslandEntityClassId.CubeResource; }
        }

        public override string StackType
        {
            get
            {
                return base.StackType + CubeId; //effectively this.getType().Name + cubeid , so blockadder1 blockadder2 etc ...
            }
        }

        public CubeResource(ILandscapeManager2D landscapeManager)
        {
            _landscapeManager = landscapeManager;
        }

        public override void Load(System.IO.BinaryReader reader)
        {
            base.Load(reader);
            CubeId = reader.ReadByte();
        }

        public override void Save(System.IO.BinaryWriter writer)
        {
            base.Save(writer);
            writer.Write(CubeId);
        }

        public override Utopia.Shared.Chunks.Entities.Interfaces.IToolImpact Use(bool runOnServer = false)
        {
            var entity = Parent;
            var impact = new ToolImpact { Success = false };


            if (entity.EntityState.IsPickingActive)
            {
                var cursor = _landscapeManager.GetCursor(entity.EntityState.NewBlockPosition);
                if (cursor.Read() == 0)
                {
                    cursor.Write(CubeId);
                    impact.Success = true;
                    return impact;
                }
            }
            impact.Message = "Pick a cube to use this tool";
            return impact;
        }

        public override void Rollback(Utopia.Shared.Chunks.Entities.Interfaces.IToolImpact impact)
        {
            throw new System.NotImplementedException();
        }
    }
}
