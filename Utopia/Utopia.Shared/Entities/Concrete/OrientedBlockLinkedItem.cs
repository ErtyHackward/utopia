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
        [ProtoMember(2)]
        public bool IsOrientedSlope { get; set; }

        public override ushort ClassId
        {
            get { return EntityClassId.OrientedBlockLinkedItem; }
        }

        public OrientedBlockLinkedItem()
        {
            Type = EntityType.Static;
            Name = "Oriented Block linked Entity";
            MountPoint = BlockFace.Top;
            IsPlayerCollidable = false;
            IsPickable = true;
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
            if (owner.EntityState.PickPointNormal.Y == 1) // = Put on TOP 
            {
                pos.Position = new Vector3D(owner.EntityState.PickedBlockPosition.X + faceOffset.X,
                                                   owner.EntityState.PickedBlockPosition.Y + 1f,
                                                   owner.EntityState.PickedBlockPosition.Z + faceOffset.Z);

            }
            else if (owner.EntityState.PickPointNormal.Y == -1) // PUT on cube Bottom = (Ceiling)
            {
                pos.Position = new Vector3D(owner.EntityState.PickedBlockPosition.X + faceOffset.X,
                                                   owner.EntityState.PickedBlockPosition.Y - 1f,
                                                   owner.EntityState.PickedBlockPosition.Z + faceOffset.Z);
            }
            else // Put on a side is not possible.
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
