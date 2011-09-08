using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Utopia.Shared.Chunks.Entities.Interfaces;
using SharpDX;
using S33M3Engines.Maths;
using S33M3Engines.Struct;
using S33M3Engines.D3D;
using Utopia.Action;
using Utopia.Shared.Chunks;
using S33M3Engines.InputHandler.MouseHelper;

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
        private Vector3 _size;                    // ==> Should be extracted from the boundingbox around the voxel entity
        //================================================================================================================================

        private FTSValue<Vector3> _worldPosition = new FTSValue<Vector3>();           //World Position
        private FTSValue<Quaternion> _lookAtDirection = new FTSValue<Quaternion>();   //LookAt angle

        private Vector3 _boundingMinPoint, _boundingMaxPoint;

        private ActionsManager _actions;
        private SingleArrayChunkContainer _cubesHolder;
        private Vector3 _entityHeadXAxis, _entityHeadYAxis, _entityHeadZAxis;
        private Vector3 _lookAt;
        private Matrix _headRotation;
        private float _rotationDelta;
        private float _moveDelta;
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

        public VisualDynamicEntity(Vector3 size, IDynamicEntity dynamicEntity, ActionsManager actions, SingleArrayChunkContainer cubesHolder)
            :base()
        {
            _actions = actions;
            _cubesHolder = cubesHolder;
            _dynamicEntity = dynamicEntity;
            _size = size;
            _worldPosition.Value = _dynamicEntity.Position;
            _worldPosition.ValueInterp = _dynamicEntity.Position;
            _lookAtDirection.Value = _dynamicEntity.Rotation;
            _lookAtDirection.ValueInterp = _dynamicEntity.Rotation;
        }
        #region Public Methods
        public override void Initialize()
        {
            //Will be used to update the bounding box with world coordinate when the entity is moving
            _boundingMinPoint = new Vector3(-(_size.X / 2.0f), 0, -(_size.Z / 2.0f));
            _boundingMaxPoint = new Vector3(+(_size.X / 2.0f), _size.Y, +(_size.Z / 2.0f));

            RefreshBoundingBox();

            _entityEyeOffset = new Vector3(0, _size.Y / 100 * 80, 0);
        }

        public override void Update(ref GameTime timeSpent)
        {
            ////Compute the delta following the time elapsed : Speed * Time = Distance (Over the elapsed time).
            //_moveDelta = _dynamicEntity.MoveSpeed * timeSpent.ElapsedGameTimeInS_LD;
            //_rotationDelta = _dynamicEntity.RotationSpeed * timeSpent.ElapsedGameTimeInS_LD;

            ////Backup previous values
            //_lookAtDirection.BackUpValue();
            //_worldPosition.BackUpValue();

            ////Rotation with mouse
            //EntityRotationsOnEvents(Mode);

            ////Keybord Movement
            //EntityMovementsOnEvents(Mode, ref TimeSpend);

            ////Physic simulation !
            //PhysicOnEntity(Mode, ref TimeSpend);

            ////Take into account the physics

            ////Refresh location and Rotations compoent with the new values
            //RefreshBoundingBox();
            //UpdateLookAt();
            ////Send the Actual Position to the Dynamic Entity
            //_dynamicEntity.Position = _worldPosition.Value;
        }

        public override void Dispose()
        {
        }
        #endregion

        #region Private Methods
        private void RefreshBoundingBox()
        {
            _boundingBox = new BoundingBox(_boundingMinPoint + _worldPosition.Value,
                                          _boundingMaxPoint + _worldPosition.Value);
        }

        private void UpdateLookAt()
        {
            Matrix.RotationQuaternion(ref _lookAtDirection.Value, out _headRotation);

            _entityHeadXAxis = new Vector3(_headRotation.M11, _headRotation.M21, _headRotation.M31);
            _entityHeadYAxis = new Vector3(_headRotation.M12, _headRotation.M22, _headRotation.M32);
            _entityHeadZAxis = new Vector3(_headRotation.M13, _headRotation.M23, _headRotation.M33);

            _lookAt = new Vector3(-_entityHeadZAxis.X, -_entityHeadZAxis.Y, -_entityHeadZAxis.Z);
            _lookAt.Normalize();
        }


        private void EntityRotationsOnEvents()
        {
            //MouseState mouseState;
            //int centerX = (int)_camManager.ActiveCamera.Viewport.Width / 2; // Largeur Viewport pour centrer la souris !
            //int centerY = (int)_camManager.ActiveCamera.Viewport.Height / 2;
            //if (_d3dEngine.UnlockedMouse == false)
            //{
            //    //_inputHandler.GetCurrentMouseState(out mouseState); //To be sure the take the latest place of the mouse cursor !
            //    mouseState = Mouse.GetState();
            //    Mouse.SetPosition(centerX, centerY);
            //    Rotate((mouseState.X - centerX), (mouseState.Y - centerY), 0.0f, mode);
            //}
        }
        #endregion

    }
}
