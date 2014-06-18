using System;
using System.ComponentModel;
using ProtoBuf;
using SharpDX;
using Utopia.Shared.Entities.Concrete;
using Utopia.Shared.Entities.Interfaces;
using Utopia.Shared.Entities.Inventory;
using S33M3Resources.Structs;
using System.Linq;
using Utopia.Shared.Entities.Concrete.Interface;
using Utopia.Shared.Structs.Helpers;

namespace Utopia.Shared.Entities
{
    /// <summary>
    /// Base class for items that can be placed into a world cube, wih a relationship with it (Removing the linked key will remove the entity)
    /// A Cube placeable Item will be by default a centered cube position, and can only be placed in a cube where no other CubePlaceableItem is present.
    /// </summary>
    [ProtoContract]
    [ProtoInclude(100, typeof(OrientedBlockLinkedItem))]
    [ProtoInclude(101, typeof(Plant))]
    [ProtoInclude(102, typeof(LinkedLightSource))]
    [ProtoInclude(103, typeof(GrowingEntity))]
    public abstract class BlockLinkedItem : Item, IBlockLinkedEntity, IBlockLocationRoot
    {
        /// <summary>
        /// The cube where the entity root belongs to.
        /// </summary>
        [Browsable(false)]
        [ProtoMember(1)]
        public Vector3I BlockLocationRoot { get; set; }

        /// <summary>
        /// The cube at wich the Entity is linked, if this cube is removed, the entity will also be removed
        /// </summary>
        [Browsable(false)]
        [ProtoMember(2)]
        public Vector3I LinkedCube { get; set; }

        /// <summary>
        /// Allows to specify the possible face of the block where entity can be attached to
        /// </summary>
        [Category("BlockLinkedItem")]
        [Description("Allows to specify the possible face of the block where entity can be attached to")]
        [ProtoMember(3)]
        public BlockFace MountPoint { get; set; }

        /// <summary>
        /// The entity will be centered into the choosen block face
        /// </summary>
        [Category("BlockLinkedItem")]
        [Description("The entity will be centered into the choosen block face")]
        [ProtoMember(4)]
        public bool BlockFaceCentered { get; set; }
        
        /// <summary>
        /// The entity cannot be added if the block where it will be placed contain another entity
        /// </summary>
        [Category("BlockLinkedItem")]
        [Description("The entity cannot be added if the block where it will be placed contain another entity")]
        [ProtoMember(5)]
        public bool BlockEmptyRequired { get; set; }

        /// <summary>
        /// Indicates if the item could be mounted without block link (on other entities)
        /// </summary>
        [Category("BlockLinkedItem")]
        [Description("Indicates if the item could be mounted without block link (on other entities)")]
        [ProtoMember(6)]
        public bool AllowFreeMount { get; set; }

        /// <summary>
        /// Indicates if this item has block link or not
        /// </summary>
        [Browsable(false)]
        [ProtoMember(7)]
        public bool NotLinked { get { return !Linked; } set { Linked = !value; } }

        /// <summary>
        /// Indicates if this item has block link or not
        /// </summary>
        [Browsable(false)]
        public bool Linked { get; set; }

        public override void SetPosition(EntityPosition pos, IItem item, IDynamicEntity owner)
        {
            base.SetPosition(pos, item, owner);

            var cubeEntity = (BlockLinkedItem)item;

            if (!owner.EntityState.IsBlockPicked)
            {
                cubeEntity.Linked = false;
            }
            else
            {
                cubeEntity.LinkedCube = owner.EntityState.PickedBlockPosition;
                cubeEntity.BlockLocationRoot = BlockHelper.EntityToBlock(pos.Position);
                cubeEntity.Linked = true;
            }
        }

        public override EntityPosition GetPosition(IDynamicEntity owner)
        {
            var pos = new EntityPosition();

            if (!AllowFreeMount && !owner.EntityState.IsBlockPicked)
                return pos;

            if (!MountPoint.HasFlag(BlockFace.Top) && owner.EntityState.PickPointNormal.Y == 1)
            {
                return pos;
            }

            if (!MountPoint.HasFlag(BlockFace.Bottom) && owner.EntityState.PickPointNormal.Y == -1)
            {
                return pos;
            }

            if (!MountPoint.HasFlag(BlockFace.Sides) && (owner.EntityState.PickPointNormal.X != 0 || owner.EntityState.PickPointNormal.Z != 0))
            {
                return pos;
            }

            if (BlockEmptyRequired && owner.EntityState.IsBlockPicked)
            {
                //Get the chunk where the entity will be added and check if another entity is not present at the destination root block !
                var workingchunk = LandscapeManager.GetChunkFromBlock(owner.EntityState.PickedBlockPosition);

                if (workingchunk == null)
                    return pos;

                foreach (var entity in workingchunk.Entities.OfType<BlockLinkedItem>().Where(x => x.Linked && x.LinkedCube == owner.EntityState.PickedBlockPosition))
                {
                    if (entity.BlockLocationRoot == owner.EntityState.NewBlockPosition && entity.LinkedCube == owner.EntityState.PickedBlockPosition)
                    {
                        //CubePlaced Entity already present at this location
                        return pos;
                    }
                }
            }

            // locate the entity
            if (owner.EntityState.PickPointNormal.Y == 1) // = Put on TOP 
            {
                if (BlockFaceCentered)
                {
                    var newBlockPos = owner.EntityState.IsBlockPicked ? owner.EntityState.NewBlockPosition : owner.EntityState.PickPoint.ToCubePosition();
                    pos.Position = new Vector3D(
                            newBlockPos + new Vector3(0.5f - (float)owner.EntityState.PickPointNormal.X / 2,
                            owner.EntityState.PickPoint.Y % 1,
                            0.5f - (float)owner.EntityState.PickPointNormal.Z / 2)
                        );
                }
                else
                    pos.Position = new Vector3D(owner.EntityState.PickPoint);

            }
            else if (owner.EntityState.PickPointNormal.Y == -1) //PUT on cube Bottom = (Ceiling)
            {
                pos.Position = new Vector3D(owner.EntityState.PickPoint);
                pos.Position.Y -= DefaultSize.Y;
            }
            else //Put on a side
            {
                if (BlockFaceCentered)
                {
                    var newBlockPos = owner.EntityState.IsBlockPicked ? owner.EntityState.NewBlockPosition : owner.EntityState.PickPoint.ToCubePosition();
                    pos.Position = new Vector3D(
                            newBlockPos + new Vector3(0.5f - (float)owner.EntityState.PickPointNormal.X / 2, 
                            0.5f, 
                            0.5f - (float)owner.EntityState.PickPointNormal.Z / 2)
                        );
                }
                else
                {
                    pos.Position = new Vector3D(owner.EntityState.PickPoint);
                }

                pos.Position += new Vector3D(owner.EntityState.PickPointNormal.X == -1 ? -0.01 : 0,
                                                    0,
                                                    owner.EntityState.PickPointNormal.Z == -1 ? -0.01 : 0);


                var slope = 0d;

                if (owner.EntityState.PickPointNormal.X == -1) slope = -Math.PI / 2;
                if (owner.EntityState.PickPointNormal.X ==  1) slope =  Math.PI / 2; // ok
                if (owner.EntityState.PickPointNormal.Z == -1) slope =  Math.PI; // ok
                if (owner.EntityState.PickPointNormal.Z ==  1) slope =  0;

                pos.Rotation = Quaternion.RotationAxis(new Vector3(0, 1, 0), (float)slope);
            }

            pos.Valid = true;

            return pos;
        }
    }
}
