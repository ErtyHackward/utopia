using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using S33M3Engines.D3D;
using SharpDX;
using S33M3Engines.Struct;
using S33M3Engines.Maths;

namespace Utopia.Entities
{
    // An Entity, is mainly defined by its :
    // - World Position
    // - World Rotation
    // - A Bounding Box
    public abstract class Entity : DrawableGameComponent, IEntity
    {
        protected BoundingBox _boundingBox;
        protected DVector3 _entityEyeOffset;

        protected FTSValue<DVector3> _worldPosition = new FTSValue<DVector3>();
        protected FTSValue<Quaternion> _lookAtDirection = new FTSValue<Quaternion>();
        protected FTSValue<Quaternion> _worldRotation = new FTSValue<Quaternion>();

        private Vector3 _boundingMinPoint, _boundingMaxPoint;

        public virtual FTSValue<DVector3> WorldPosition { get { return _worldPosition; } }
        public FTSValue<Quaternion> LookAtDirection { get { return _lookAtDirection; } }
        public virtual FTSValue<Quaternion> WorldRotation { get { return _worldRotation; } }
        public BoundingBox BoundingBox { get { return _boundingBox; } set { _boundingBox = value; } }

        public virtual DVector3 CameraWorldPosition
        {
            get { return _worldPosition.ActualValue + _entityEyeOffset; }
        }

        public virtual Quaternion CameraOrientation
        {
            get { return _lookAtDirection.ActualValue; }
        }

        public Entity( DVector3 startUpWorldPosition, Vector3 size)
        {
            _worldPosition.Value = startUpWorldPosition;
            _worldPosition.ValueInterp = startUpWorldPosition;
            _boundingMinPoint = new Vector3(- (size.X / 2.0f), 0, - (size.Z / 2.0f));
            _boundingMaxPoint = new Vector3(+ (size.X / 2.0f), size.Y, + (size.Z / 2.0f));

            _boundingBox = new BoundingBox(_boundingMinPoint + startUpWorldPosition.AsVector3(),
                                          _boundingMaxPoint + startUpWorldPosition.AsVector3());

            _entityEyeOffset = new DVector3(0, size.Y / 100 * 80, 0);
        }

        public override void Update(ref GameTime TimeSpend)
        {
            //Refresh bounding box to put it in world coordinate
            RefreshBoundingBox(ref WorldPosition.Value, out _boundingBox);
        }

        public void RefreshBoundingBox(ref DVector3 worldPosition, out BoundingBox boundingBox)
        {
            boundingBox = new BoundingBox(_boundingMinPoint + worldPosition.AsVector3(),
                                          _boundingMaxPoint + worldPosition.AsVector3());
        }

    }
}
