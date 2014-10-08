using System.Linq;
using ProtoBuf;
using S33M3CoreComponents.Maths;
using S33M3Resources.Structs;
using SharpDX;
using System;
using System.ComponentModel;
using Utopia.Shared.Configuration;
using Utopia.Shared.Entities.Concrete.Interface;
using Utopia.Shared.Entities.Interfaces;
using Utopia.Shared.Entities.Inventory;
using Utopia.Shared.Structs.Helpers;
using Utopia.Shared.Entities.Concrete.System;

namespace Utopia.Shared.Entities.Concrete
{
    /// <summary>
    /// A block item that is rotated according to current player rotation
    /// </summary>
    [ProtoContract]
    [ProtoInclude(100, typeof(Door))]
    [ProtoInclude(101, typeof(MusicPlayer))]
    [ProtoInclude(102, typeof(Ladder))]
    [ProtoInclude(103, typeof(Container))]
    [ProtoInclude(104, typeof(SoulStone))]
    [Description("Entity of this type will have one of 4 orientations and occupy a block.")]
    public class OrientedBlockItem : BlockItem, IOrientedSlope
    {
        /// <summary>
        /// Gets or sets item orientation
        /// </summary>
        [Browsable(false)]
        [ProtoMember(1)]
        public ItemOrientation Orientation { get; set; }
        
        /// <summary>
        /// Gets or sets value indicating if entity can climb on this entity by the angle of 45 degree
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

        public OrientedBlockItem()
        {
            Name = "Oriented Entity";
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

            Vector3I? newBlockPos = null;

            var playerRotation = owner.HeadRotation.GetLookAtVector();

            if (owner.EntityState.IsBlockPicked)
                newBlockPos = owner.EntityState.NewBlockPosition;
            else if (owner.EntityState.IsEntityPicked && owner.EntityState.PickedEntityLink.IsStatic)
            {
                var entity = owner.EntityState.PickedEntityLink.ResolveStatic(LandscapeManager);

                if (entity is BlockItem)
                {
                    newBlockPos = owner.EntityState.PickedEntityPosition.ToCubePosition();
                    
                    var rotation = entity.Rotation;
                    var normal = Vector3.TransformNormal(owner.EntityState.PickPointNormal, Matrix.RotationQuaternion(rotation));
                    var converted = new Vector3I((int)Math.Round(normal.X, MidpointRounding.ToEven), (int)Math.Round(normal.Y, MidpointRounding.ToEven), (int)Math.Round(normal.Z, MidpointRounding.ToEven));
                    newBlockPos += converted;
                }

            }

            if (newBlockPos == null)
                return pos;

            var cursor = LandscapeManager.GetCursor(newBlockPos.Value);
            
            if (cursor == null || cursor.Read() != WorldConfiguration.CubeId.Air || LandscapeManager.GetChunkFromBlock(newBlockPos.Value).Entities.OfType<BlockItem>().Any(i => i.BlockLocationRoot == newBlockPos))
                return pos;
            
            // locate the entity, set translation in World space
            pos.Position = new Vector3D(newBlockPos.Value.X + 0.5f,
                                        newBlockPos.Value.Y,
                                        newBlockPos.Value.Z + 0.5f);
            
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
