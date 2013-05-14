using ProtoBuf;
using S33M3CoreComponents.Maths;
using S33M3Resources.Structs;
using SharpDX;
using System;
using System.ComponentModel;
using Utopia.Shared.Entities.Concrete.Interface;
using Utopia.Shared.Entities.Interfaces;
using Utopia.Shared.Entities.Inventory;

namespace Utopia.Shared.Entities.Concrete
{
    /// <summary>
    /// A block item that is rotated according to current player rotation
    /// </summary>
    [ProtoContract]
    [Description("Entity of this type will have one of 4 orientations and occupy a block.")]
    public class OrientedBlockItem : BlockItem, IOrientedSlope
    {
        public override ushort ClassId
        {
            get { return EntityClassId.OrientedBlockItem; }
        }

        /// <summary>
        /// Gets or sets item orientation
        /// </summary>
        [Browsable(false)]
        [ProtoMember(1)]
        public ItemOrientation Orientation { get; set; }
        
        /// <summary>
        /// Gets or sets value indicating if entity can climb on this entity by the angle of 45 degree
        /// </summary>
        [ProtoMember(2)]
        public bool IsOrientedSlope { get; set; }
        
        public OrientedBlockItem()
        {
            Name = "Oriented Entity";
            IsPlayerCollidable = true;
            IsPickable = true;
        }

        public override void SetPosition(EntityPosition pos, IItem item, IDynamicEntity owner)
        {
            base.SetPosition(pos, item, owner);

            var blockItem = item as OrientedBlockItem;
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
            
            var playerRotation = owner.HeadRotation.GetLookAtVector();
            
            // locate the entity, set translation in World space
            pos.Position = new Vector3D(owner.EntityState.NewBlockPosition.X + 0.5f,
                                        owner.EntityState.NewBlockPosition.Y,
                                        owner.EntityState.NewBlockPosition.Z + 0.5f);

            //Set Orientation
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

            //Specific Item Rotation for this instance
            pos.Rotation = Quaternion.RotationAxis(new Vector3(0, 1, 0), (float)entityRotation);
            pos.Valid = true;

            return pos;
        }

    }
}
