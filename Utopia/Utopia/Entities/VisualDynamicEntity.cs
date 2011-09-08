using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Utopia.Shared.Chunks.Entities.Interfaces;
using SharpDX;
using S33M3Engines.Maths;
using S33M3Engines.Struct;
using S33M3Engines.D3D;

namespace Utopia.Entities
{
    /// <summary>
    /// Visual Class Wrapping a IDynamicEntity
    /// Could be Player, Monsters, ...
    /// </summary>
    public class VisualDynamicEntity : GameComponent, IDisposable
    {
        #region Private variables
        private IDynamicEntity _dynamicEntity;

        //=======Should be moved inside VisualVertexEntity when the schema will bind the Dynamicentity to voxelentity, if we go this way !
        private BoundingBox _boundingBox;
        private Vector3 _entityEyeOffset;         //Offset of the camera Placement inside the entity, from entity center point.

        private FTSValue<Vector3> _worldPosition = new FTSValue<Vector3>();           //World Position
        private FTSValue<Quaternion> _lookAtDirection = new FTSValue<Quaternion>();   //LookAt angle
        private FTSValue<Quaternion> _worldRotation = new FTSValue<Quaternion>();     //World Rotation

        private Vector3 _boundingMinPoint, _boundingMaxPoint;
        #endregion

        #region Public Variables/Properties
        //Implement the interface Needed when a Camera is "plugged" inside this entity
        public virtual Vector3 CameraWorldPosition
        {
            get { return _worldPosition.ActualValue + _entityEyeOffset; }
        }

        public virtual Quaternion CameraOrientation
        {
            get { return _lookAtDirection.ActualValue; }
        }
        #endregion

        public VisualDynamicEntity(Vector3 size, IDynamicEntity dynamicEntity)
            :base()
        {
            _dynamicEntity = dynamicEntity;
            _worldPosition.Value = _dynamicEntity.Position;
            _worldPosition.ValueInterp = _dynamicEntity.Position;
            _boundingMinPoint = new Vector3(-(size.X / 2.0f), 0, -(size.Z / 2.0f));
            _boundingMaxPoint = new Vector3(+(size.X / 2.0f), size.Y, +(size.Z / 2.0f));

            RefreshBoundingBox();

            _entityEyeOffset = new Vector3(0, size.Y / 100 * 80, 0);
        }

        #region Private Methods
        #endregion

        #region Public Methods
        public override void Update(ref GameTime timeSpent)
        {
            _dynamicEntity.Position = _worldPosition.Value;
            RefreshBoundingBox();
        }

        public void RefreshBoundingBox()
        {
            _boundingBox = new BoundingBox(_boundingMinPoint + _worldPosition.Value,
                                          _boundingMaxPoint + _worldPosition.Value);
        }

        public override void Dispose()
        {
        }
        #endregion

    }
}
