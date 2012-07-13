using System;
using Utopia.Shared.Entities.Interfaces;
using Utopia.Shared.Entities.Inventory;
using Utopia.Shared.Interfaces;
using S33M3Resources.Structs;

namespace Utopia.Shared.Entities
{
    /// <summary>
    /// Base class for items that can be placed into a world cube
    /// </summary>
    public abstract class CubePlaceableItem : Item, ITool, IWorldIntercatingEntity
    {
        /// <summary>
        /// Gets landscape manager, this field is injected
        /// </summary>
        public ILandscapeManager2D LandscapeManager { get; set; }

        /// <summary>
        /// Gets entityFactory, this field is injected
        /// </summary>
        public EntityFactory Factory { get; set; }

        public IToolImpact Use(IDynamicEntity owner, ToolUseMode useMode, bool runOnServer)
        {
            var impact = new ToolImpact { Success = false };

            if (useMode == ToolUseMode.RightMouse)
            {
                if (owner.EntityState.IsBlockPicked)
                {
                    var chunk = LandscapeManager.GetChunk(owner.EntityState.PickedBlockPosition);

                    //Create a new version of the Grass, and put it into the world
                    var cubeEntity = (IItem)Factory.CreateEntity(ClassId);
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
