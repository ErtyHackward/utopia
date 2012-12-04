using ProtoBuf;
using S33M3CoreComponents.Maths;
using S33M3Resources.Structs;
using SharpDX;
using System;
using System.ComponentModel;
using Utopia.Shared.Entities.Concrete.Interface;
using Utopia.Shared.Entities.Interfaces;

namespace Utopia.Shared.Entities.Concrete
{
    [ProtoContract]
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
        public OrientedItem Orientation { get; set; }
        
        /// <summary>
        /// Gets or sets value indicating if entity can climb on this entity by the angle of 45 degree
        /// </summary>
        [ProtoMember(2)]
        public bool IsOrientedSlope { get; set; }
        
        public OrientedBlockItem()
        {
            Type = EntityType.Static;
            Name = "Oriented Entity";
            IsPlayerCollidable = true;
            IsPickable = true;
        }

        #region Public Methods
        protected override bool SetNewItemPlace(BlockItem cubeEntity, IDynamicEntity owner)
        {
            var playerRotation = owner.HeadRotation.GetLookAtVector();
            OrientedBlockItem entity = (OrientedBlockItem)cubeEntity;

            // locate the entity, set translation in World space
            cubeEntity.Position = new Vector3D(owner.EntityState.NewBlockPosition.X + 0.5f,
                                               owner.EntityState.NewBlockPosition.Y,
                                               owner.EntityState.NewBlockPosition.Z + 0.5f);

            //Set Orientation
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

            //Specific Item Rotation for this instance
            cubeEntity.Rotation = Quaternion.RotationAxis(new Vector3(0, 1, 0), (float)entityRotation);

            return true;
        }
        #endregion


    }
}
