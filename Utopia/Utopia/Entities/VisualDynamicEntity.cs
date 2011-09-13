using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using S33M3Engines.D3D;
using Utopia.Shared.Chunks.Entities.Concrete;
using Utopia.Shared.Chunks.Entities.Interfaces;
using SharpDX;
using S33M3Engines.Struct;
using S33M3Engines.Shared.Math;

namespace Utopia.Entities
{
    public class VisualDynamicEntity
    {
        #region Private variables
        //Player Visual characteristics (Not insde the PlayerCharacter object)
        private BoundingBox _playerBoundingBox;
        private Vector3 _boundingMinPoint, _boundingMaxPoint;                         //Use to recompute the bounding box in world coordinate
        private FTSValue<DVector3> _worldPosition = new FTSValue<DVector3>();         //World Position
        private FTSValue<Quaternion> _lookAtDirection = new FTSValue<Quaternion>();   //LookAt angle
        private FTSValue<Quaternion> _moveDirection = new FTSValue<Quaternion>();     //Real move direction (derived from LookAt, but will depend the mode !)
        #endregion

        #region Public variables/properties
        /// <summary>
        /// The Player
        /// </summary>
        public readonly IDynamicEntity DynamicEntity;
        /// <summary>
        /// The Player Voxel body
        /// </summary>
        public readonly VoxelEntity VoxelEntity;
        #endregion

        public VisualDynamicEntity(IDynamicEntity dynamicEntity, VoxelEntity voxelEntity)
        {
            this.DynamicEntity = dynamicEntity;
            this.VoxelEntity = voxelEntity;

            Initialize();
        }

        #region Private Methods
        private void Initialize()
        {
            //Will be used to update the bounding box with world coordinate when the entity is moving
            _boundingMinPoint = new Vector3(-(DynamicEntity.Size.X / 2.0f), 0, -(DynamicEntity.Size.Z / 2.0f));
            _boundingMaxPoint = new Vector3(+(DynamicEntity.Size.X / 2.0f), DynamicEntity.Size.Y, +(DynamicEntity.Size.Z / 2.0f));

            //Compute the initial Player world bounding box
            RefreshBoundingBox(ref _worldPosition.Value, out _playerBoundingBox);

            //Set Position
            //Set the entity world position following the position received from server
            _worldPosition.Value = DynamicEntity.Position;
            _worldPosition.ValuePrev = DynamicEntity.Position;

            //Set LookAt
            _lookAtDirection.Value = DynamicEntity.Rotation;
            _lookAtDirection.ValuePrev = _lookAtDirection.Value;

            //Set Move direction = to LookAtDirection
            _moveDirection.Value = _lookAtDirection.Value;
        }

        /// <summary>
        /// Compute player bounding box in World coordinate
        /// </summary>
        /// <param name="worldPosition"></param>
        /// <param name="boundingBox"></param>
        private void RefreshBoundingBox(ref DVector3 worldPosition, out BoundingBox boundingBox)
        {
            boundingBox = new BoundingBox(_boundingMinPoint + worldPosition.AsVector3(),
                                          _boundingMaxPoint + worldPosition.AsVector3());
        }

        private void RefreshEntityMovementAndRotation()
        {
            _lookAtDirection.BackUpValue();
            _worldPosition.BackUpValue();

            _worldPosition.Value = DynamicEntity.Position;
            _lookAtDirection.Value = DynamicEntity.Rotation;
        }

        #endregion

        #region Public Methods
        public void Update(ref GameTime timeSpent)
        {
            RefreshEntityMovementAndRotation(); 
        }

        public void Interpolation(ref double interpolationHd, ref float interpolationLd)
        {
            Quaternion.Slerp(ref _lookAtDirection.ValuePrev, ref _lookAtDirection.Value, interpolationLd, out _lookAtDirection.ValueInterp);
            DVector3.Lerp(ref _worldPosition.ValuePrev, ref _worldPosition.Value, interpolationHd, out _worldPosition.ValueInterp);
        }
        #endregion
    }
}
