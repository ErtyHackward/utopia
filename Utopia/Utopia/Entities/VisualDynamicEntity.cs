using System;
using S33M3Engines;
using S33M3Engines.Cameras;
using S33M3Engines.D3D;
using S33M3Engines.Maths;
using S33M3Engines.Shared.Math;
using S33M3Engines.Struct;
using SharpDX;
using Utopia.Action;
using Utopia.InputManager;
using Utopia.Shared.Chunks;
using Utopia.Shared.Chunks.Entities;
using Utopia.Shared.Chunks.Entities.Interfaces;

namespace Utopia.Entities
{
    /// <summary>
    /// Visual Class Wrapping a IDynamicEntity
    /// Could be Player, Monsters, ...
    /// </summary>
    public class VisualDynamicEntity : GameComponent, ICameraPlugin, IDisposable
    {
        #region Private variables
        private IDynamicEntity _dynamicEntity;

        //=======Should be moved inside VisualVertexEntity when the schema will bind the Dynamicentity to voxelentity, if we go this way !
        private BoundingBox _boundingBox;
        private Vector3 _entityEyeOffset;         //Offset of the camera Placement inside the entity, from entity center point.
        private Vector3 _size;                    // ==> Should be extracted from the boundingbox around the voxel entity
        //================================================================================================================================

        private FTSValue<DVector3> _worldPosition = new FTSValue<DVector3>();           //World Position
        private FTSValue<Quaternion> _lookAtDirection = new FTSValue<Quaternion>();   //LookAt angle
        private FTSValue<Quaternion> _moveDirection = new FTSValue<Quaternion>();     //Real move direction (derived from LookAt, but will depend the mode !)

        private Vector3 _boundingMinPoint, _boundingMaxPoint;

        private ActionsManager _actions;
        private InputsManager _inputsManager;
        private SingleArrayChunkContainer _cubesHolder;
        private DVector3 _entityHeadXAxis, _entityHeadYAxis, _entityHeadZAxis;
        private DVector3 _entityXAxis, _entityYAxis, _entityZAxis;
        private DVector3 _lookAt;
        private Matrix _headRotation;
        private Matrix _entityRotation;
        private double _rotationDelta;
        private double _moveDelta;
        private D3DEngine _engine;
        private double _accumPitchDegrees;
        #endregion

        #region Public Variables/Properties
        //Implement the interface Needed when a Camera is "plugged" inside this entity
        public virtual DVector3 CameraWorldPosition
        {
            get { return _worldPosition.ActualValue + _entityEyeOffset; }
        }

        public virtual Quaternion CameraOrientation
        {
            get { return _lookAtDirection.ActualValue; }
        }
        #endregion

        public VisualDynamicEntity(D3DEngine engine, Vector3 size, IDynamicEntity dynamicEntity, ActionsManager actions, InputsManager inputsManager, SingleArrayChunkContainer cubesHolder)
            : base()
        {
            _engine = engine;
            _actions = actions;
            _inputsManager = inputsManager;
            _cubesHolder = cubesHolder;
            _dynamicEntity = dynamicEntity;
            _size = size;

            _lookAtDirection.Value = Quaternion.Identity;
            _lookAtDirection.ValueInterp = Quaternion.Identity;

            _worldPosition.Value = _dynamicEntity.Position;
            _worldPosition.ValueInterp = _dynamicEntity.Position;
            //_lookAtDirection.Value = _dynamicEntity.Rotation;
            //_lookAtDirection.ValueInterp = _dynamicEntity.Rotation;
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
            //Compute the delta following the time elapsed : Speed * Time = Distance (Over the elapsed time).
            _moveDelta = _dynamicEntity.MoveSpeed * timeSpent.ElapsedGameTimeInS_HD;
            _rotationDelta = _dynamicEntity.RotationSpeed * timeSpent.ElapsedGameTimeInS_HD;

            //Backup previous values
            _lookAtDirection.BackUpValue();
            _worldPosition.BackUpValue();

            //Compute Rotation with mouse
            EntityRotations();

            //Keybord Movement
            EntityMovements(ref timeSpent);

            //Physic simulation !
            //PhysicOnEntity(ref timeSpent);

            //Take into account the physics

            //Refresh location and Rotations compoent with the new values
            RefreshBoundingBox();
            UpdateLookAt();
            //Send the Actual Position to the Dynamic Entity
            _dynamicEntity.Position = _worldPosition.Value;
        }

        public override void Interpolation(ref double interpolationHd, ref float interpolationLd)
        {
            DVector3.Lerp(ref _worldPosition.ValuePrev, ref _worldPosition.Value, interpolationHd, out _worldPosition.ValueInterp);
            Quaternion.Slerp(ref _lookAtDirection.ValuePrev, ref _lookAtDirection.Value, interpolationLd, out _lookAtDirection.ValueInterp);
        }

        public override void Dispose()
        {
        }
        #endregion

        #region Private Methods
        private void RefreshBoundingBox()
        {
            _boundingBox = new BoundingBox(_boundingMinPoint + _worldPosition.Value.AsVector3(),
                                          _boundingMaxPoint + _worldPosition.Value.AsVector3());
        }

        #region Entity Movement management
        private void EntityMovements(ref GameTime TimeSpend)
        {
            switch (_dynamicEntity.DisplacementMode)
            {
                case EntityDisplacementModes.Flying:
                    FreeFirstPersonMove();
                    break;
                case EntityDisplacementModes.Walking:
                    WalkingFirstPerson(ref TimeSpend);
                    break;
                case EntityDisplacementModes.Swiming:
                    break;
                default:
                    break;
            }
        }

        private void FreeFirstPersonMove()
        {
            if (_actions.isTriggered(Actions.Move_Forward))
                _worldPosition.Value += _lookAt * _moveDelta;

            if (_actions.isTriggered(Actions.Move_Backward))
                _worldPosition.Value -= _lookAt * _moveDelta;

            if (_actions.isTriggered(Actions.Move_StrafeLeft))
                _worldPosition.Value -= _entityHeadXAxis * _moveDelta;

            if (_actions.isTriggered(Actions.Move_StrafeRight))
                _worldPosition.Value += _entityHeadXAxis * _moveDelta;

            if (_actions.isTriggered(Actions.Move_Down))
                _worldPosition.Value += DVector3.Down * _moveDelta;

            if (_actions.isTriggered(Actions.Move_Up))
                _worldPosition.Value += DVector3.Up * _moveDelta;
        }

        private void WalkingFirstPerson(ref GameTime TimeSpend)
        {
            //_physicSimu.Freeze(true, false, true);

            ////Move 3 time slower if not touching ground
            //if (!_physicSimu.OnGround) _moveDelta /= 3f;

            //if (_actions.isTriggered(Actions.Move_Forward))
            //    if (_actions.isTriggered(Actions.Move_Run)) _physicSimu.PrevPosition += _entityZAxis * _moveDelta * 2f;
            //    else _physicSimu.PrevPosition += _entityZAxis * _moveDelta;

            //if (_actions.isTriggered(Actions.Move_Backward))
            //    _physicSimu.PrevPosition -= _entityZAxis * _moveDelta;

            //if (_actions.isTriggered(Actions.Move_StrafeLeft))
            //    _physicSimu.PrevPosition += _entityXAxis * _moveDelta;

            //if (_actions.isTriggered(Actions.Move_StrafeRight))
            //    _physicSimu.PrevPosition -= _entityXAxis * _moveDelta;

            //if (_physicSimu.OnGround && _actions.isTriggered(Actions.Move_Jump))
            //    _physicSimu.Impulses.Add(new Impulse(ref TimeSpend) { ForceApplied = new DVector3(0, 300, 0) });
        }
        #endregion

        #region Entity Rotation management
        private void EntityRotations()
        {
            if (_engine.UnlockedMouse == false)
            {
                Rotate(_inputsManager.MouseMoveDelta.X, _inputsManager.MouseMoveDelta.Y, 0.0f, _dynamicEntity.DisplacementMode);
            }
        }

        private void Rotate(double headingDegrees, double pitchDegrees, double rollDegrees, EntityDisplacementModes mode)
        {
            if (headingDegrees == 0 && pitchDegrees == 0 && rollDegrees == 0) return;

            headingDegrees *= _rotationDelta;
            pitchDegrees *= _rotationDelta;
            rollDegrees *= _rotationDelta;

            Rotate(headingDegrees, pitchDegrees, mode);
        }

        private void Rotate(double headingDegrees, double pitchDegrees, EntityDisplacementModes mode)
        {
            _accumPitchDegrees += pitchDegrees;

            if (_accumPitchDegrees > 90.0f)
            {
                pitchDegrees = 90.0f - (_accumPitchDegrees - pitchDegrees);
                _accumPitchDegrees = 90.0f;
            }

            if (_accumPitchDegrees < -90.0f)
            {
                pitchDegrees = -90.0f - (_accumPitchDegrees - pitchDegrees);
                _accumPitchDegrees = -90.0f;
            }

            double heading = MathHelper.ToRadians(headingDegrees);
            double pitch = MathHelper.ToRadians(pitchDegrees);
            Quaternion rotation;

            // Rotate the camera around the world Y axis.
            if (heading != 0.0f)
            {
                Quaternion.RotationAxis(ref MVector3.Up, (float)heading, out rotation);
                _lookAtDirection.Value = rotation * _lookAtDirection.Value;
                _moveDirection.Value = rotation * _moveDirection.Value;
                _moveDirection.Value.Normalize();
            }

            // Rotate the camera around its local X axis.
            if (pitch != 0.0f)
            {
                Quaternion.RotationAxis(ref MVector3.Right, (float)pitch, out rotation);
                _lookAtDirection.Value = _lookAtDirection.Value * rotation;
            }
            _lookAtDirection.Value.Normalize();

            UpdateLookAt();
            UpdateEntityRotation();
        }

        private void UpdateLookAt()
        {
            Matrix.RotationQuaternion(ref _lookAtDirection.Value, out _headRotation);

            _entityHeadXAxis = new DVector3(_headRotation.M11, _headRotation.M21, _headRotation.M31);
            _entityHeadYAxis = new DVector3(_headRotation.M12, _headRotation.M22, _headRotation.M32);
            _entityHeadZAxis = new DVector3(_headRotation.M13, _headRotation.M23, _headRotation.M33);


            _lookAt = new DVector3(-_entityHeadZAxis.X, -_entityHeadZAxis.Y, -_entityHeadZAxis.Z);
            _lookAt.Normalize();
        }

        private void UpdateEntityRotation()
        {
            Matrix.RotationQuaternion(ref _moveDirection.Value, out _entityRotation);

            _entityXAxis = new DVector3(_entityRotation.M11, _entityRotation.M21, _entityRotation.M31);
            _entityYAxis = new DVector3(_entityRotation.M12, _entityRotation.M22, _entityRotation.M32);
            _entityZAxis = new DVector3(_entityRotation.M13, _entityRotation.M23, _entityRotation.M33);
        }
        #endregion

        #endregion
    }
}
