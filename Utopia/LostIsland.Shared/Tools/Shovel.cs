using System.Collections.Generic;
using Utopia.Shared.Chunks.Entities.Inventory.Tools;
using Utopia.Shared.Cubes;
using Utopia.Shared.Interfaces;

namespace LostIsland.Shared.Tools
{
    /// <summary>
    /// a shovel is blockRemover restricted to grass & dirt
    /// </summary>
    public class Shovel : BlockRemover
    {
        private readonly ILandscapeManager2D _landscapeManager;

        public Shovel()
        {
            RemoveableCubeIds = new HashSet<byte>();
            RemoveableCubeIds.Add(CubeId.Dirt);
            RemoveableCubeIds.Add(CubeId.Grass);
        }

        public override ushort ClassId
        {
            get { return LostIslandEntityClassId.Shovel; }
        }

        public override int MaxStackSize
        {
            get { return 1; }
        }

        public override string DisplayName
        {
            get
            {
                return "Shovel";
            }
        }

        public Shovel(ILandscapeManager2D landscapeManager)
        {
            _landscapeManager = landscapeManager;
        }

        public override Utopia.Shared.Chunks.Entities.Interfaces.IToolImpact Use(bool runOnServer = false)
        {
            var entity = Parent;
            var impact = new ToolImpact { Success = false };

            if (entity.EntityState.IsPickingActive)
            {
                var cursor = _landscapeManager.GetCursor(entity.EntityState.PickedBlockPosition);

                var blockValue = cursor.Read();
                if (blockValue == CubeId.Dirt || blockValue == CubeId.Grass)
                {
                    cursor.Write(0);
                    impact.Success = true;
                    return impact;
                }

                impact.Message = "Shovel can only dig dirt and grass";
                return impact;
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
