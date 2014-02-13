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

namespace Utopia.Shared.Entities
{
    /// <summary>
    /// Base class for items that can be placed into a world cube, wih a relationship with it (Removing the linked key will remove the entity)
    /// A Cube placeable Item will be by default a centered cube position, and can only be placed in a cube where no other CubePlaceableItem is present.
    /// </summary>
    [ProtoContract]
    [ProtoInclude(100, typeof(OrientedBlockLinkedItem))]
    [ProtoInclude(101, typeof(Plant))]
    [ProtoInclude(102, typeof(LightSource))]
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

        [Description("Allows to specify the possible face of the block where entity can be attached to")]
        [ProtoMember(3)]
        public BlockFace MountPoint { get; set; }

        [Description("The entity will be centered into the choosen block face")]
        [ProtoMember(4)]
        public bool BlockFaceCentered { get; set; }
        
        [Description("The entity cannot be added if the block where it will be placed contain another entity")]
        [ProtoMember(5)]
        public bool BlockEmptyRequired { get; set; }

        public override void SetPosition(EntityPosition pos, IItem item, IDynamicEntity owner)
        {
            base.SetPosition(pos, item, owner);

            var cubeEntity = (BlockLinkedItem)item;

            cubeEntity.LinkedCube = owner.EntityState.PickedBlockPosition;
            cubeEntity.BlockLocationRoot = owner.EntityState.NewBlockPosition;

        }

        public override EntityPosition GetPosition(IDynamicEntity owner)
        {
            var pos = new EntityPosition();

            if (!owner.EntityState.IsBlockPicked)
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

            if (BlockEmptyRequired)
            {
                //Get the chunk where the entity will be added and check if another entity is not present at the destination root block !
                var workingchunk = LandscapeManager.GetChunkFromBlock(owner.EntityState.PickedBlockPosition);
                foreach (var entity in workingchunk.Entities.OfType<BlockLinkedItem>().Where(x => x.LinkedCube == owner.EntityState.PickedBlockPosition))
                {
                    if (entity.BlockLocationRoot == owner.EntityState.NewBlockPosition && entity.LinkedCube == owner.EntityState.PickedBlockPosition)
                    {
                        //CubePlaced Entity already present at this location
                        return pos;
                    }
                }
            }

            Vector3 faceOffset = owner.EntityState.PickedBlockFaceOffset;

            // locate the entity
            if (owner.EntityState.PickPointNormal.Y == 1) // = Put on TOP 
            {
                pos.Position = new Vector3D(owner.EntityState.PickPoint);

            }
            else if (owner.EntityState.PickPointNormal.Y == -1) //PUT on cube Bottom = (Ceiling)
            {
                pos.Position = new Vector3D(owner.EntityState.PickPoint);
                pos.Position.Y -= DefaultSize.Y;
            }
            else //Put on a side
            {
                if (BlockFaceCentered == false)
                {
                    pos.Position = new Vector3D(owner.EntityState.PickPoint);
                }
                else
                {
                    Vector3I newBlockPos = owner.EntityState.NewBlockPosition;
                    pos.Position = new Vector3D(newBlockPos + new Vector3(0.5f - (float)owner.EntityState.PickPointNormal.X / 2, 0.5f, 0.5f - (float)owner.EntityState.PickPointNormal.Z / 2));
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
