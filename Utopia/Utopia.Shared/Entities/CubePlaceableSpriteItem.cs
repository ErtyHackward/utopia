using System;
using S33M3Engines.Shared.Math;
using Utopia.Shared.Entities.Interfaces;
using Utopia.Shared.Entities.Inventory;
using Utopia.Shared.Interfaces;

namespace Utopia.Shared.Entities
{
    /// <summary>
    /// Base class for items that can be placed into a world cube
    /// </summary>
    public abstract class CubePlaceableSpriteItem : SpriteItem, ITool
    {
        private readonly ILandscapeManager2D _landscapeManager;

        protected CubePlaceableSpriteItem(ILandscapeManager2D landscapeManager2D)
        {
            if (landscapeManager2D == null) throw new ArgumentNullException("landscapeManager2D");
            _landscapeManager = landscapeManager2D;
        }

        public IToolImpact Use(IDynamicEntity owner, ToolUseMode useMode, bool runOnServer)
        {
            var impact = new ToolImpact { Success = false };

            if (useMode == ToolUseMode.RightMouse)
            {
                if (owner.EntityState.IsBlockPicked)
                {
                    var chunk = _landscapeManager.GetChunk(owner.EntityState.PickedBlockPosition);

                    //Create a new version of the Grass, and put it into the world
                    var cubeEntity = (IItem)EntityFactory.Instance.CreateEntity(ClassId);
                    cubeEntity.Position = new Vector3D(owner.EntityState.PickedBlockPosition.X + 0.5f, owner.EntityState.PickedBlockPosition.Y + 1f, owner.EntityState.PickedBlockPosition.Z + 0.5f);

                    chunk.Entities.Add(cubeEntity);

                    impact.Success = true;
                }
            }
            return impact;
        }

        public void Rollback(IToolImpact impact)
        {
            throw new NotImplementedException();
        }
    }
}
