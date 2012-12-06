using System;
using System.ComponentModel;
using ProtoBuf;
using S33M3CoreComponents.Maths;
using S33M3Resources.Structs;
using SharpDX;
using Utopia.Shared.Entities.Concrete.Interface;
using Utopia.Shared.Entities.Interfaces;

namespace Utopia.Shared.Entities.Concrete
{
    [ProtoContract]
    public class OrientedBlockLinkedItem : BlockLinkedItem, IOrientedSlope
    {
        private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();
        /// <summary>
        /// Gets landscape manager, this field is injected
        /// </summary>
        [Browsable(false)]
        [ProtoMember(1)]
        public OrientedItem Orientation { get; set; }

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

        protected override bool SetNewItemPlace(BlockLinkedItem cubeEntity, IDynamicEntity owner, Vector3I vector)
        {
            Vector3 faceOffset = BlockFaceCentered ? new Vector3(0.5f, 0.5f, 0.5f) : owner.EntityState.PickedBlockFaceOffset;

            var playerRotation = owner.HeadRotation.GetLookAtVector();
            var entity = (OrientedBlockLinkedItem)cubeEntity;
            // locate the entity
            if (vector.Y == 1) // = Put on TOP 
            {
                cubeEntity.Position = new Vector3D(owner.EntityState.PickedBlockPosition.X + faceOffset.X,
                                                   owner.EntityState.PickedBlockPosition.Y + 1f,
                                                   owner.EntityState.PickedBlockPosition.Z + faceOffset.Z);
                
            }
            else if (vector.Y == -1) // PUT on cube Bottom = (Ceiling)
            {
                cubeEntity.Position = new Vector3D(owner.EntityState.PickedBlockPosition.X + faceOffset.X,
                                                   owner.EntityState.PickedBlockPosition.Y - 1f,
                                                   owner.EntityState.PickedBlockPosition.Z + faceOffset.Z);
            }
            else // Put on a side not possible.
            {
                return false;
            }

            double entityRotation;
            if (Math.Abs(playerRotation.Z) >= Math.Abs(playerRotation.X))
            {
                if (playerRotation.Z < 0)
                {
                    entityRotation = MathHelper.Pi;
                    entity.Orientation = OrientedItem.North;
                }
                else
                {
                    entityRotation = 0;
                    entity.Orientation = OrientedItem.South;
                }
            }
            else
            {
                if (playerRotation.X < 0)
                {
                    entityRotation = MathHelper.PiOver2;
                    entity.Orientation = OrientedItem.East;
                }
                else
                {
                    entityRotation = -MathHelper.PiOver2;
                    entity.Orientation = OrientedItem.West;
                }
            }

            // Specific Item Rotation for this instance
            cubeEntity.Rotation = Quaternion.RotationAxis(new Vector3(0, 1, 0), (float)entityRotation);

            return true;
        }
    }
}
