using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SharpDX;
using Utopia.Shared.Entities.Interfaces;
using Utopia.Shared.Structs;
using S33M3Resources.Structs;
using Utopia.Shared.Entities.Concrete;

namespace Utopia.Entities
{
    public abstract class VisualEntity
    {
        public enum EntityCollisionType
        {
            BoundingBox,
            SlopeNorth,
            SlopeSouth,
            SlopeWest,
            SlopeEast,
            Model
        }

        /// <summary>
        /// The BBox surrounding the Entity, it will be used for collision detections mainly !
        /// </summary>
        public BoundingBox WorldBBox = new BoundingBox();
        public BoundingBox LocalBBox = new BoundingBox();
        public ByteColor BlockLight;
        public IEntity Entity;
        public EntityCollisionType CollisionType;

        public VisualEntity(Vector3 entitySize, IEntity entity)
        {
            CollisionType = EntityCollisionType.BoundingBox;

            Entity = entity;
            Matrix rotation = Matrix.Identity;
          
            if (Entity is OrientedCubePlaceableItem)
            {
                OrientedCubePlaceableItem orientedEntity = (OrientedCubePlaceableItem)Entity;
                if (orientedEntity.IsOrientedSlope)
                {
                    switch (orientedEntity.Orientation)
                    {
                        case OrientedCubePlaceableItem.OrientatedItem.North:
                            CollisionType = EntityCollisionType.SlopeNorth;
                            break;
                        case OrientedCubePlaceableItem.OrientatedItem.South:
                            CollisionType = EntityCollisionType.SlopeSouth;
                            break;
                        case OrientedCubePlaceableItem.OrientatedItem.East:
                            CollisionType = EntityCollisionType.SlopeEast;
                            break;
                        case OrientedCubePlaceableItem.OrientatedItem.West:
                            CollisionType = EntityCollisionType.SlopeWest;
                            break;
                    }
                }
            }

            //If not size was specified and the entity is a voxel entity
            //if (entitySize == Vector3.Zero && entity is IVoxelEntity)
            //{
            //    BoundingBox voxelModelBB = ((IVoxelEntity)entity).ModelInstance.VoxelModel.States[0].BoundingBox;

            //    if (voxelModelBB != null)
            //    {
            //        LocalBBox = new BoundingBox(voxelModelBB.Minimum / 16, voxelModelBB.Maximum / 16);
                    
            //        //Add instance rotation, if existing
            //        if (entity is IStaticEntity)
            //        {
            //            LocalBBox = LocalBBox.Transform(Matrix.RotationQuaternion(((IStaticEntity)entity).Rotation));
            //        }

            //        ComputeWorldBoundingBox(entity.Position, out WorldBBox);
            //    }
            //}
            //else
            //{
            //    if (entitySize != Vector3.Zero)
            //    {
            //        CreateLocalBoundingBox(entitySize);
            //        //Add instance rotation, if existing
            //        if (entity is IStaticEntity)
            //        {
            //            LocalBBox = LocalBBox.Transform(Matrix.RotationQuaternion(((IStaticEntity)entity).Rotation));
            //        }
            //        ComputeWorldBoundingBox(entity.Position, out WorldBBox);
            //    }
            //}

            if (entitySize != Vector3.Zero)
            {
                CreateLocalBoundingBox(entitySize);
                //Add instance rotation, if existing
                if (entity is IStaticEntity)
                {
                    LocalBBox = LocalBBox.Transform(Matrix.RotationQuaternion(((IStaticEntity)entity).Rotation));
                }
                ComputeWorldBoundingBox(entity.Position, out WorldBBox);
            }
        }

        public void SetEntityVoxelBB(BoundingBox bb)
        {
            LocalBBox = new BoundingBox(bb.Minimum / 16, bb.Maximum / 16);
            RefreshWorldBoundingBox(Entity.Position);
        }

        protected void CreateLocalBoundingBox(Vector3 entitySize)
        {
            //Will be used to update the bounding box with world coordinate when the entity is moving
            LocalBBox.Minimum = new Vector3(-(entitySize.X / 2.0f), 0, -(entitySize.Z / 2.0f));
            LocalBBox.Maximum = new Vector3(+(entitySize.X / 2.0f), entitySize.Y, +(entitySize.Z / 2.0f));
        }

        /// <summary>
        /// Compute player bounding box in World coordinate
        /// </summary>
        /// <param name="worldPosition"></param>
        /// <param name="boundingBox"></param>
        public void RefreshWorldBoundingBox(ref Vector3D worldPosition)
        {
            WorldBBox.Minimum = LocalBBox.Minimum + worldPosition.AsVector3();
            WorldBBox.Maximum = LocalBBox.Maximum + worldPosition.AsVector3();
        }

        /// <summary>
        /// Compute player bounding box in World coordinate
        /// </summary>
        /// <param name="worldPosition"></param>
        /// <param name="boundingBox"></param>
        public void RefreshWorldBoundingBox(Vector3D worldPosition)
        {
            WorldBBox.Minimum = LocalBBox.Minimum + worldPosition.AsVector3();
            WorldBBox.Maximum = LocalBBox.Maximum + worldPosition.AsVector3();
        }

        /// <summary>
        /// Compute player bounding box in World coordinate
        /// </summary>
        /// <param name="worldPosition"></param>
        /// <param name="boundingBox"></param>
        public void RefreshWorldBoundingBox(Vector3 worldPosition)
        {
            WorldBBox.Minimum = LocalBBox.Minimum + worldPosition;
            WorldBBox.Maximum = LocalBBox.Maximum + worldPosition;
        }

        /// <summary>
        /// Compute player bounding box in World coordinate
        /// </summary>
        /// <param name="worldPosition"></param>
        /// <param name="boundingBox"></param>
        public BoundingBox ComputeWorldBoundingBox(ref Vector3D worldPosition)
        {
            return new BoundingBox(LocalBBox.Minimum + worldPosition.AsVector3(),
                                          LocalBBox.Maximum + worldPosition.AsVector3());
        }

        public void ComputeWorldBoundingBox(ref Vector3D worldPosition, out BoundingBox worldBB)
        {
            worldBB = new BoundingBox(LocalBBox.Minimum + worldPosition.AsVector3(),
                                          LocalBBox.Maximum + worldPosition.AsVector3());
        }

        public void ComputeWorldBoundingBox(Vector3D worldPosition, out BoundingBox worldBB)
        {
            worldBB = new BoundingBox(LocalBBox.Minimum + worldPosition.AsVector3(),
                                          LocalBBox.Maximum + worldPosition.AsVector3());
        }
    }
}
