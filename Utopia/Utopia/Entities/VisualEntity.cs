using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SharpDX;
using Utopia.Shared.Entities.Interfaces;
using Utopia.Shared.Structs;
using S33M3Resources.Structs;

namespace Utopia.Entities
{
    public abstract class VisualEntity
    {

        /// <summary>
        /// The BBox surrounding the Entity, it will be used for collision detections mainly !
        /// </summary>
        public BoundingBox WorldBBox = new BoundingBox();
        public BoundingBox LocalBBox = new BoundingBox();
        public ByteColor Color;
        public IEntity Entity;

        public VisualEntity(Vector3 entitySize, IEntity entity)
        {
            Entity = entity;

            CreateLocalBoundingBox(entitySize);
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
    }
}
