using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using S33M3CoreComponents.Maths;
using S33M3Resources.Structs;
using SharpDX;
using Utopia.Shared.Entities.Interfaces;

namespace Utopia.Shared.Entities.Concrete
{
    public class OrientedCubePlaceableItem : CubePlaceableItem
    {
        public enum OrientatedItem : byte
        {
            North,
            South,
            East,
            West
        }

        private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();
        /// <summary>
        /// Gets landscape manager, this field is injected
        /// </summary>
        [Browsable(false)]
        public OrientatedItem Orientation { get; set; }

        /// <summary>
        /// Gets landscape manager, this field is injected
        /// </summary>
        public bool IsOrientedSlope { get; set; }

        public override ushort ClassId
        {
            get { return EntityClassId.OrientedCubePlaceableItem; }
        }

        public OrientedCubePlaceableItem()
        {
            Type = EntityType.Static;
            Name = "Oriented Entity";
            MountPoint = BlockFace.Top;
            IsPlayerCollidable = false;
            IsPickable = true;
        }

        protected override bool SetNewItemPlace(CubePlaceableItem cubeEntity, IDynamicEntity owner, Vector3I vector)
        {
            var playerRotation = MQuaternion.GetLookAtFromQuaternion(owner.HeadRotation);
            OrientedCubePlaceableItem entity = (OrientedCubePlaceableItem)cubeEntity;
            // locate the entity
            if (vector.Y == 1) // = Put on TOP 
            {
                cubeEntity.Position = new Vector3D(owner.EntityState.PickedBlockPosition.X + 0.5f,
                                                   owner.EntityState.PickedBlockPosition.Y + 1f,
                                                   owner.EntityState.PickedBlockPosition.Z + 0.5f);
                
            }
            else if (vector.Y == -1) //PUT on cube Bottom = (Ceiling)
            {
                cubeEntity.Position = new Vector3D(owner.EntityState.PickedBlockPosition.X + 0.5f,
                                   owner.EntityState.PickedBlockPosition.Y - 1f,
                                   owner.EntityState.PickedBlockPosition.Z + 0.5f);
            }
            else //Put on a side not possible for OrientedCubePlaceabltItems
            {
                return false;
            }

            double entityRotation;
            if (Math.Abs(playerRotation.Z) >= Math.Abs(playerRotation.X))
            {
                if (playerRotation.Z < 0)
                {
                    entityRotation = MathHelper.Pi;
                    entity.Orientation = OrientatedItem.North;
                }
                else
                {
                    entityRotation = 0;
                    entity.Orientation = OrientatedItem.South;
                }
            }
            else
            {
                if (playerRotation.X < 0)
                {
                    entityRotation = MathHelper.PiOver2;
                    entity.Orientation = OrientatedItem.West;
                }
                else
                {
                    entityRotation = -MathHelper.PiOver2;
                    entity.Orientation = OrientatedItem.East;
                }
            }

            //Specific Item Rotation for this instance
            cubeEntity.Rotation = Quaternion.RotationAxis(new Vector3(0, 1, 0), (float)entityRotation);

            return true;
        }


        public override void Load(BinaryReader reader, EntityFactory factory)
        {
            // first we need to load base information
            base.Load(reader, factory);
            Orientation = (OrientatedItem)reader.ReadByte();
            IsOrientedSlope = reader.ReadBoolean();
        }

        public override void Save(BinaryWriter writer)
        {
            // first we need to save base information
            base.Save(writer);
            writer.Write((byte)Orientation);
            writer.Write(IsOrientedSlope);
        }

    }
}
