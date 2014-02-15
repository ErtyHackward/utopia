using System.Linq;
using ProtoBuf;
using S33M3Resources.Structs;
using System.ComponentModel;
using SharpDX;
using Utopia.Shared.Entities.Concrete;
using Utopia.Shared.Entities.Concrete.Interface;
using Utopia.Shared.Entities.Interfaces;
using Utopia.Shared.Entities.Inventory;
using Utopia.Shared.Structs.Helpers;

namespace Utopia.Shared.Entities
{
    /// <summary>
    /// Base class for Item that will be placed in a centered way on a block,
    /// This entity cannot be placed on a block where another entity is placed.
    /// </summary>
    [ProtoContract]
    [ProtoInclude(100, typeof(OrientedBlockItem))]
    public abstract class BlockItem : Item, IBlockLocationRoot
    {
        /// <summary>
        /// The cube where the entity root belongs to.
        /// </summary>
        [Browsable(false)]
        [ProtoMember(1)]
        public Vector3I BlockLocationRoot { get; set; }
        
        #region Public Methods

        public override void SetPosition(EntityPosition pos, IItem item, IDynamicEntity owner)
        {
            base.SetPosition(pos, item, owner);

            var cubeEntity = (BlockItem)item;
            cubeEntity.BlockLocationRoot = BlockHelper.EntityToBlock(pos.Position);
        }

        public override EntityPosition GetPosition(IDynamicEntity owner)
        {
            var pos = new EntityPosition();

            if (!owner.EntityState.IsBlockPicked) return pos;

            // Get the chunk where the entity will be added and check if another entity is present inside this block
            var workingchunk = LandscapeManager.GetChunkFromBlock(owner.EntityState.NewBlockPosition);
            foreach (var entity in workingchunk.Entities.OfType<IBlockLocationRoot>())
            {
                if (entity.BlockLocationRoot == BlockLocationRoot)
                {
                    // IBlockLocationRoot Entity already present at this location
                    return pos;
                }
            }

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
