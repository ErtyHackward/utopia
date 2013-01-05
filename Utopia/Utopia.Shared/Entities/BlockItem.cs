using ProtoBuf;
using S33M3Resources.Structs;
using System.ComponentModel;
using SharpDX;
using Utopia.Shared.Entities.Concrete.Interface;
using Utopia.Shared.Entities.Interfaces;
using Utopia.Shared.Entities.Inventory;

namespace Utopia.Shared.Entities
{
    /// <summary>
    /// Base class for Item that will be placed in a centered way on a block,
    /// This entity cannot be placed on a block where another entity is placed.
    /// </summary>
    [ProtoContract]
    public abstract class BlockItem : Item, IBlockLocationRoot
    {
        /// <summary>
        /// The cube where the entity root belongs to.
        /// </summary>
        [Browsable(false)]
        [ProtoMember(1)]
        public Vector3I BlockLocationRoot { get; set; }


        #region Public Methods
        public override IToolImpact Use(IDynamicEntity owner, bool runOnServer = false)
        {
            var impact = new ToolImpact { Success = false };

            if (owner.EntityState.IsBlockPicked)
            {
                var cursor = LandscapeManager.GetCursor(owner.EntityState.NewBlockPosition);
                    
                // check if the place is free for te entity "Root"
                if (cursor.PeekProfile().IsSolidToEntity) return impact;

                // create a new version of the item, and put it into the world
                var cubeEntity = (BlockItem)EntityFactory.CreateFromBluePrint(BluePrintId);
                cubeEntity.BlockLocationRoot = owner.EntityState.NewBlockPosition;
                // Get the chunk where the entity will be added and check if another entity is present inside this block
                var workingchunk = LandscapeManager.GetChunk(owner.EntityState.NewBlockPosition);
                foreach (IBlockLocationRoot entity in workingchunk.Entities.Entities.Values)
                {
                    if (entity.BlockLocationRoot == cubeEntity.BlockLocationRoot)
                    {
                        // IBlockLocationRoot Entity already present at this location
                        return impact;
                    }
                }

                // Do the Chunk on chunk Next to this one ==> TO DO

                // If was not possible to set Item Place do nothing
                var position = GetPosition(owner);

                if (!position.Valid)
                    return impact;

                SetPosition(position, cubeEntity);

                cursor.AddEntity(cubeEntity, owner.DynamicId);

                impact.Success = true;
            }

            return impact;
        }

        public override EntityPosition GetPosition(IDynamicEntity owner)
        {
            var pos = new EntityPosition();

            if (!owner.EntityState.IsBlockPicked)
                return pos;

            pos.Position = new Vector3D(owner.EntityState.NewBlockPosition.X + 0.5f,
                                        owner.EntityState.NewBlockPosition.Y,
                                        owner.EntityState.NewBlockPosition.Z + 0.5f);

            pos.Rotation = Quaternion.Identity;
            pos.Valid = true;

            return pos;
        }

        #endregion
    }
}
