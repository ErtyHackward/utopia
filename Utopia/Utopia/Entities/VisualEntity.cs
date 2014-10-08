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
        /// <summary>
        /// The BBox surrounding the Entity, it will be used for collision detections mainly !
        /// </summary>
        public BoundingBox WorldBBox = new BoundingBox();
        public BoundingBox LocalBBox = new BoundingBox();
        public ByteColor BlockLight;
        public IEntity Entity;
        public bool SkipOneCollisionTest;

        public VisualEntity(Vector3 entitySize, IEntity entity)
        {
            Entity = entity;
            
            //If a default size has been given, then use it to compute the Entity bounding Box around it
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

        protected float GetModelScale(IEntity entity)
        {
            var tree = Entity as TreeGrowingEntity;
            var scale = 1f / 16;

            if (tree != null && tree.Scale > 0)
            {
                scale = tree.Scale;
            }

            return scale;
        }

        public virtual void SetEntityVoxelBB(BoundingBox bb)
        {
            var scale = GetModelScale(Entity);
            LocalBBox = new BoundingBox(bb.Minimum * scale, bb.Maximum * scale);
            RefreshWorldBoundingBox(Entity.Position);
        }

        protected void CreateLocalBoundingBox(Vector3 entitySize)
        {
            //Will be used to update the bounding box with world coordinate when the entity is moving
            LocalBBox.Minimum = new Vector3(-(entitySize.X / 2.0f), 0,            -(entitySize.Z / 2.0f));
            LocalBBox.Maximum = new Vector3(+(entitySize.X / 2.0f), entitySize.Y, +(entitySize.Z / 2.0f));
        }

        /// <summary>
        /// Compute player bounding box in World coordinate
        /// </summary>
        /// <param name="worldPosition"></param>
        public void RefreshWorldBoundingBox(ref Vector3D worldPosition)
        {
            WorldBBox.Minimum = LocalBBox.Minimum + worldPosition.AsVector3();
            WorldBBox.Maximum = LocalBBox.Maximum + worldPosition.AsVector3();
        }

        /// <summary>
        /// Compute player bounding box in World coordinate
        /// </summary>
        /// <param name="worldPosition"></param>
        public void RefreshWorldBoundingBox(Vector3D worldPosition)
        {
            WorldBBox.Minimum = LocalBBox.Minimum + worldPosition.AsVector3();
            WorldBBox.Maximum = LocalBBox.Maximum + worldPosition.AsVector3();
        }

        /// <summary>
        /// Compute player bounding box in World coordinate
        /// </summary>
        /// <param name="worldPosition"></param>
        public void RefreshWorldBoundingBox(Vector3 worldPosition)
        {
            WorldBBox.Minimum = LocalBBox.Minimum + worldPosition;
            WorldBBox.Maximum = LocalBBox.Maximum + worldPosition;
        }

        /// <summary>
        /// Compute player bounding box in World coordinate
        /// </summary>
        /// <param name="worldPosition"></param>
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
