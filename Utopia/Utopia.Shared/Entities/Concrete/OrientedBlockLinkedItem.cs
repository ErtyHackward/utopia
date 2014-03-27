using System;
using System.ComponentModel;
using ProtoBuf;
using S33M3CoreComponents.Maths;
using S33M3Resources.Structs;
using SharpDX;
using Utopia.Shared.Entities.Concrete.Interface;
using Utopia.Shared.Entities.Interfaces;
using Utopia.Shared.Entities.Inventory;

namespace Utopia.Shared.Entities.Concrete
{
    [ProtoContract]
    [Description("Entity of this type will have one of 4 orientations and will disappear if their linked block is removed.")]
    public class OrientedBlockLinkedItem : BlockLinkedItem, IOrientedSlope
    {
        private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();
        /// <summary>
        /// Gets landscape manager, this field is injected
        /// </summary>
        [Browsable(false)]
        [ProtoMember(1)]
        public ItemOrientation Orientation { get; set; }

        /// <summary>
        /// Gets landscape manager, this field is injected
        /// </summary>
        [Category("OrientedBlockItem")]
        [ProtoMember(2)]
        public bool IsOrientedSlope { get; set; }

        /// <summary>
        /// Get or Set value indicating of the slope is sliding
        /// </summary>
        [Category("OrientedBlockItem")]
        [ProtoMember(3)]
        public bool IsSlidingSlope { get; set; }

        public OrientedBlockLinkedItem()
        {
            Name = "Oriented Block linked Entity";
            MountPoint = BlockFace.Top;
        }

        public override void SetPosition(EntityPosition pos, IItem item, IDynamicEntity owner)
        {
            base.SetPosition(pos, item, owner);

            var blockItem = item as OrientedBlockLinkedItem;
            if (blockItem != null)
            {
                blockItem.Orientation = pos.Orientation;
            }
        }

        public override EntityPosition GetPosition(IDynamicEntity owner)
        {
            var pos = new EntityPosition();

            if (!owner.EntityState.IsBlockPicked)
                return pos;

            Vector3 faceOffset = BlockFaceCentered ? new Vector3(0.5f, 0.5f, 0.5f) : owner.EntityState.PickedBlockFaceOffset;

            var playerRotation = owner.HeadRotation.GetLookAtVector();
            
            // locate the entity
            if (MountPoint.HasFlag(BlockFace.Top) && owner.EntityState.PickPointNormal.Y == 1) // = Put on TOP 
            {
                pos.Position = new Vector3D(owner.EntityState.PickedBlockPosition.X + faceOffset.X,
                                            owner.EntityState.PickedBlockPosition.Y + 1f,
                                            owner.EntityState.PickedBlockPosition.Z + faceOffset.Z);

            }
            else if (MountPoint.HasFlag(BlockFace.Bottom) && owner.EntityState.PickPointNormal.Y == -1) // PUT on cube Bottom = (Ceiling)
            {
                pos.Position = new Vector3D(owner.EntityState.PickedBlockPosition.X + faceOffset.X,
                                            owner.EntityState.PickedBlockPosition.Y - 1f,
                                            owner.EntityState.PickedBlockPosition.Z + faceOffset.Z);
            }
            else if (MountPoint.HasFlag(BlockFace.Sides) && (owner.EntityState.PickPointNormal.X != 0 || owner.EntityState.PickPointNormal.Z != 0)) // Put on sides
            {
                if (BlockFaceCentered)
                {
                    var newBlockPos = owner.EntityState.IsBlockPicked ? owner.EntityState.NewBlockPosition : owner.EntityState.PickPoint.ToCubePosition();
                    pos.Position = new Vector3D(
                            newBlockPos + new Vector3(0.5f - (float)owner.EntityState.PickPointNormal.X / 2,
                            0f,
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
                if (owner.EntityState.PickPointNormal.X == 1) slope = Math.PI / 2; // ok
                if (owner.EntityState.PickPointNormal.Z == -1) slope = Math.PI; // ok
                if (owner.EntityState.PickPointNormal.Z == 1) slope = 0;

                pos.Rotation = Quaternion.RotationAxis(new Vector3(0, 1, 0), (float)slope);
                pos.Valid = true;
                return pos;
            }
            else
            {
                return pos;
            }

            double entityRotation;
            if (Math.Abs(playerRotation.Z) >= Math.Abs(playerRotation.X))
            {
                if (playerRotation.Z < 0)
                {
                    entityRotation = MathHelper.Pi;
                    pos.Orientation = ItemOrientation.North;
                }
                else
                {
                    entityRotation = 0;
                    pos.Orientation = ItemOrientation.South;
                }
            }
            else
            {
                if (playerRotation.X < 0)
                {
                    entityRotation = MathHelper.PiOver2;
                    pos.Orientation = ItemOrientation.East;
                }
                else
                {
                    entityRotation = -MathHelper.PiOver2;
                    pos.Orientation = ItemOrientation.West;
                }
            }

            // Specific Item Rotation for this instance
            pos.Rotation = Quaternion.RotationAxis(new Vector3(0, 1, 0), (float)entityRotation);
            pos.Valid = true;

            return pos;
        }
    }
}
