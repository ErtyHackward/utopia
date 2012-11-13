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
        private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

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

            // locate the entity
            if (vector.Y == 1) // = Put on TOP 
            {
                cubeEntity.Position = new Vector3D(owner.EntityState.PickedBlockPosition.X + 0.5f,
                                                   owner.EntityState.PickedBlockPosition.Y + 1f,
                                                   owner.EntityState.PickedBlockPosition.Z + 0.5f);
                double entityRotation;

                if (Math.Abs(playerRotation.Z) >= Math.Abs(playerRotation.X))
                {
                    if (playerRotation.Z < 0) entityRotation = MathHelper.Pi; //  result = "North";
                    else entityRotation = 0;
                }
                else
                {
                    if (playerRotation.X < 0) entityRotation = MathHelper.PiOver2;
                    else entityRotation = -MathHelper.PiOver2;
                }

                cubeEntity.Rotation = Quaternion.RotationAxis(new Vector3(0, 1, 0), (float)entityRotation);
            }
            else if (vector.Y == -1) //PUT on cube Bottom = (Ceiling)
            {
                cubeEntity.Position = new Vector3D(owner.EntityState.PickedBlockPosition.X + 0.5f,
                                   owner.EntityState.PickedBlockPosition.Y,
                                   owner.EntityState.PickedBlockPosition.Z + 0.5f);
                double entityRotation;

                if (Math.Abs(playerRotation.Z) >= Math.Abs(playerRotation.X))
                {
                    if (playerRotation.Z < 0) entityRotation = 0; //  result = "North";
                    else entityRotation = MathHelper.Pi;
                }
                else
                {
                    if (playerRotation.X < 0) entityRotation = MathHelper.PiOver2;
                    else entityRotation = -MathHelper.PiOver2;
                }

                cubeEntity.Rotation = Quaternion.RotationAxis(new Vector3(0, 1, 0), (float)entityRotation);
                cubeEntity.Rotation *= Quaternion.RotationAxis(new Vector3(1, 0, 0), MathHelper.Pi);
            }
            else //Put on a side
            {
                return false;
            }

            return true;
        }

    }
}
