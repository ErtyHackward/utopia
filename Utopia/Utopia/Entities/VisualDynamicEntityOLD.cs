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
using Utopia.InputManager;
using S33M3Engines;
using Utopia.Shared.Chunks.Entities;
using S33M3Engines.Shared.Math;
using S33M3Engines.Cameras;
using Utopia.Shared.Structs;
using Utopia.Shared.Cubes;
using S33M3Physics.Verlet;
using S33M3Physics;
using Utopia.Shared.Chunks.Entities.Concrete;
using Utopia.Entities.Voxel;
using S33M3Engines.WorldFocus;

namespace Utopia.Entities
{
    /// <summary>
    /// Visual Class Wrapping a IDynamicEntity
    /// Could be Player, Monsters, ...
    /// </summary>
    public abstract class VisualDynamicEntityOLD : VisualEntityOLD, ICameraPlugin, IDisposable
    {
        #region Private variables

        private FTSValue<Quaternion> _lookAtDirection = new FTSValue<Quaternion>();   //LookAt angle
        private FTSValue<Quaternion> _moveDirection = new FTSValue<Quaternion>();     //Real move direction (derived from LookAt, but will depend the mode !)
        private ActionsManager _actions;
        private InputsManager _inputsManager;
        private SingleArrayChunkContainer _cubesHolder;
        private DVector3 _entityHeadXAxis, _entityHeadYAxis, _entityHeadZAxis;
        private DVector3 _entityXAxis, _entityYAxis, _entityZAxis;
        private Matrix _headRotation;
        private Matrix _entityRotation;
        private double _rotationDelta;
        private double _moveDelta;
        private D3DEngine _engine;
        private double _accumPitchDegrees;
        private double _gravityInfluence;
        private float _groundBelowEntity;
        private VerletSimulator _physicSimu;
        private EntityDisplacementModes _displacementMode;
        #endregion

        #region Public Variables/Properties
        public DVector3 LookAt;

        bool _headInsideWater;

        public bool HeadInsideWater
        {
            get { return _headInsideWater; }
            set { _headInsideWater = value; }
        }

        /// <summary> The Core component </summary>
        public readonly IDynamicEntity DynamicEntity;

        //Implement the interface Needed when a Camera is "plugged" inside this entity
        public virtual DVector3 CameraWorldPosition
        {
            get { return WorldPosition.Value + EntityEyeOffset; }
        }

        public virtual Quaternion CameraOrientation
        {
            get { return _lookAtDirection.Value; }
        }

        public EntityDisplacementModes Mode
        {
            get { return _displacementMode; }
            set
            {
                _displacementMode = value;
                if (_displacementMode == EntityDisplacementModes.Walking)
                {
                    _physicSimu.StartSimulation(ref WorldPosition.Value, ref WorldPosition.Value);
                }
                else
                {
                    _physicSimu.StopSimulation();
                }
            }
        }
        #endregion

        public VisualDynamicEntityOLD(D3DEngine engine, 
                                   CameraManager cameraManager,
                                   WorldFocusManager worldFocusManager,
                                   IDynamicEntity dynamicEntity, 
                                   ActionsManager actions, 
                                   InputsManager inputsManager, 
                                   SingleArrayChunkContainer cubesHolder, 
                                   VoxelMeshFactory voxelMeshFactory, 
                                   VoxelEntity voxelEntity)
            : base(engine, cameraManager, worldFocusManager, voxelMeshFactory, voxelEntity, dynamicEntity)
        {
            _engine = engine;
            _actions = actions;
            _inputsManager = inputsManager;
            _cubesHolder = cubesHolder;
            DynamicEntity = dynamicEntity;

            _physicSimu = new VerletSimulator(ref BoundingBox) { WithCollisionBounsing = false };
            _physicSimu.ConstraintFct += isCollidingWithTerrain;

            Mode = dynamicEntity.DisplacementMode;

            Init();
        }
        #region Public Methods

        public void Init()
        {
            //Set Position
            //Set the entity world position following the position received from server
            WorldPosition.Value = DynamicEntity.Position;
            WorldPosition.ValuePrev = DynamicEntity.Position;

            //Set LookAt
            //Take back only the saved server Yaw rotation (Or Heading) and only using it;
            _lookAtDirection.Value = DynamicEntity.Rotation;
            double playerSavedYaw = MQuaternion.getYaw(ref _lookAtDirection.Value);
            Quaternion.RotationAxis(ref MVector3.Up, (float)playerSavedYaw, out _lookAtDirection.Value);
            _lookAtDirection.ValuePrev = _lookAtDirection.Value;

            //Set Move direction = to LookAtDirection
            _moveDirection.Value = _lookAtDirection.Value;
        }

        public override void Draw()
        {
            base.Draw();
        }

        public override void Update(ref GameTime timeSpent)
        {
            if (IsPlayerConstroled)
            {

                inputHandler();

                switch (_displacementMode)
                {
                    case EntityDisplacementModes.Flying:
                        _gravityInfluence = 6;  // We will move 6 times faster if flying
                        break;
                    case EntityDisplacementModes.Walking:
                        _gravityInfluence = 1;
                        break;
                    case EntityDisplacementModes.Swiming:
                        _gravityInfluence = 1 / 2; // We will move 2 times slower when swimming
                        break;
                    default:
                        break;
                }

                //Compute the delta following the time elapsed : Speed * Time = Distance (Over the elapsed time).
                _moveDelta = DynamicEntity.MoveSpeed * _gravityInfluence * timeSpent.ElapsedGameTimeInS_HD;
                _rotationDelta = DynamicEntity.RotationSpeed * timeSpent.ElapsedGameTimeInS_HD;

                //Backup previous values
                _lookAtDirection.BackUpValue();
                WorldPosition.BackUpValue();

                //Rotation with mouse
                EntityRotationsOnEvents(_displacementMode);

                //Movement
                EntityMovementsOnEvents(_displacementMode, ref timeSpent);

                //Physic simulation !
                PhysicOnEntity(_displacementMode, ref timeSpent);

                //Send the Actual Position to the Entity object only of it has change !!!
                if (DynamicEntity.Position != WorldPosition.Value) DynamicEntity.Position = WorldPosition.Value;
                if (DynamicEntity.Rotation != _lookAtDirection.Value) DynamicEntity.Rotation = _lookAtDirection.Value;

            }
            else
            {
                _lookAtDirection.BackUpValue();
                WorldPosition.BackUpValue();

                WorldPosition.Value = DynamicEntity.Position;
                _lookAtDirection.Value = DynamicEntity.Rotation;
            }

            base.Update(ref timeSpent);
        }

        public override void Interpolation(ref double interpolationHd, ref float interpolationLd)
        {
            Quaternion.Slerp(ref _lookAtDirection.ValuePrev, ref _lookAtDirection.Value, interpolationLd, out _lookAtDirection.ValueInterp);
            DVector3.Lerp(ref WorldPosition.ValuePrev, ref WorldPosition.Value, interpolationHd, out WorldPosition.ValueInterp);
            base.Interpolation(ref interpolationHd, ref interpolationLd);
        }

        public override void Dispose()
        {
            base.Dispose();
        }
        #endregion

        #region Private Methods

        private void inputHandler()
        {
            if (_actions.isTriggered(Actions.Move_Mode))
            {
                if (_displacementMode == EntityDisplacementModes.Flying)
                {
                    Mode = EntityDisplacementModes.Walking;
                }
                else
                {
                    Mode = EntityDisplacementModes.Flying;
                }
            }
        }

        #region Movement Management
        private void EntityMovementsOnEvents(EntityDisplacementModes mode, ref GameTime TimeSpend)
        {
            switch (mode)
            {
                case EntityDisplacementModes.Flying:
                    FreeFirstPersonMove();
                    break;
                case EntityDisplacementModes.Walking:
                    WalkingFirstPerson(ref TimeSpend);
                    break;
                default:
                    break;
            }
        }

        private void FreeFirstPersonMove()
        {
            if (_actions.isTriggered(Actions.Move_Forward))
                WorldPosition.Value += LookAt * _moveDelta;

            if (_actions.isTriggered(Actions.Move_Backward))
                WorldPosition.Value -= LookAt * _moveDelta;

            if (_actions.isTriggered(Actions.Move_StrafeLeft))
                WorldPosition.Value -= _entityHeadXAxis * _moveDelta;

            if (_actions.isTriggered(Actions.Move_StrafeRight))
                WorldPosition.Value += _entityHeadXAxis * _moveDelta;

            if (_actions.isTriggered(Actions.Move_Down))
                WorldPosition.Value += DVector3.Down * _moveDelta;

            if (_actions.isTriggered(Actions.Move_Up))
                WorldPosition.Value += DVector3.Up * _moveDelta;
        }

        private void WalkingFirstPerson(ref GameTime TimeSpend)
        {
            _physicSimu.Freeze(true, false, true);

            //Move 3 time slower if not touching ground
            if (!_physicSimu.OnGround) _moveDelta /= 3f;

            if (_actions.isTriggered(Actions.Move_Forward))
                if (_actions.isTriggered(Actions.Move_Run)) _physicSimu.PrevPosition += _entityZAxis * _moveDelta * 2f;
                else _physicSimu.PrevPosition += _entityZAxis * _moveDelta;

            if (_actions.isTriggered(Actions.Move_Backward))
                _physicSimu.PrevPosition -= _entityZAxis * _moveDelta;

            if (_actions.isTriggered(Actions.Move_StrafeLeft))
                _physicSimu.PrevPosition += _entityXAxis * _moveDelta;

            if (_actions.isTriggered(Actions.Move_StrafeRight))
                _physicSimu.PrevPosition -= _entityXAxis * _moveDelta;

            if (_physicSimu.OnGround && _actions.isTriggered(Actions.Move_Jump))
                _physicSimu.Impulses.Add(new Impulse(ref TimeSpend) { ForceApplied = new DVector3(0, 300, 0) });
        }
        #endregion

        #region Head + Body Rotation management
        private void EntityRotationsOnEvents(EntityDisplacementModes mode)
        {
            if (_engine.UnlockedMouse == false)
            {
                Rotate(_inputsManager.MouseMoveDelta.X, _inputsManager.MouseMoveDelta.Y, 0.0f,  mode);
            }
        }

        private void Rotate(double headingDegrees, double pitchDegrees, double rollDegrees, EntityDisplacementModes mode)
        {
            if (headingDegrees == 0 && pitchDegrees == 0 && rollDegrees == 0) return;

            headingDegrees *= _rotationDelta;
            pitchDegrees *= _rotationDelta;
            rollDegrees *= _rotationDelta;

            switch (mode)
            {
                case EntityDisplacementModes.Flying:
                    RotateLookAt(headingDegrees, pitchDegrees);
                    RotateMove(headingDegrees);
                    break;
                case EntityDisplacementModes.Walking:
                    RotateLookAt(headingDegrees, pitchDegrees);
                    RotateMove(headingDegrees);
                    break;
                default:
                    break;
            }
        }

        private void RotateMove(double headingDegrees)
        {
            double heading = MathHelper.ToRadians(headingDegrees);
            Quaternion rotation;

            // Rotate the camera about the world Y axis.
            if (heading != 0.0f)
            {
                Quaternion.RotationAxis(ref MVector3.Up, (float)heading, out rotation);
                _moveDirection.Value = rotation * _moveDirection.Value;
            }

            _moveDirection.Value.Normalize();

            UpdateEntityData();
        }
        private void UpdateEntityData()
        {
            Matrix.RotationQuaternion(ref _moveDirection.Value, out _entityRotation);

            _entityXAxis = new DVector3(_entityRotation.M11, _entityRotation.M21, _entityRotation.M31);
            _entityYAxis = new DVector3(_entityRotation.M12, _entityRotation.M22, _entityRotation.M32);
            _entityZAxis = new DVector3(_entityRotation.M13, _entityRotation.M23, _entityRotation.M33);
        }

        private void RotateLookAt(double headingDegrees, double pitchDegrees)
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

            // Rotate the camera about the world Y axis.
            if (heading != 0.0f)
            {
                Quaternion.RotationAxis(ref MVector3.Up, (float)heading, out rotation);
                _lookAtDirection.Value = rotation * _lookAtDirection.Value;
            }

            // Rotate the camera about its local X axis.
            if (pitch != 0.0f)
            {
                Quaternion.RotationAxis(ref MVector3.Right, (float)pitch, out rotation);
                _lookAtDirection.Value = _lookAtDirection.Value * rotation;
            }

            _lookAtDirection.Value.Normalize();
            UpdateHeadData();
        }

        private void UpdateHeadData()
        {
            Matrix.RotationQuaternion(ref _lookAtDirection.Value, out _headRotation);

            _entityHeadXAxis = new DVector3(_headRotation.M11, _headRotation.M21, _headRotation.M31);
            _entityHeadYAxis = new DVector3(_headRotation.M12, _headRotation.M22, _headRotation.M32);
            _entityHeadZAxis = new DVector3(_headRotation.M13, _headRotation.M23, _headRotation.M33);

            LookAt = new DVector3(-_entityHeadZAxis.X, -_entityHeadZAxis.Y, -_entityHeadZAxis.Z);
            LookAt.Normalize();
        }
        #endregion

        #region Physic Simulation
        private void PhysicOnEntity(EntityDisplacementModes mode, ref GameTime TimeSpend)
        {
            switch (mode)
            {
                case EntityDisplacementModes.Flying:
                    break;
                case EntityDisplacementModes.Walking:
                    PhysicSimulation(ref TimeSpend);
                    break;
                default:
                    break;
            }
        }

        private void PhysicSimulation(ref GameTime TimeSpend)
        {
            TerraCubeWithPosition groundCube;
            Location3<int> GroundDirection = new Location3<int>(0, -1, 0);
            DVector3 newWorldPosition;

            _cubesHolder.GetNextSolidBlockToPlayer(ref BoundingBox, ref GroundDirection, out groundCube);
            if (groundCube.Cube.Id != CubeId.Error)
            {
                _groundBelowEntity = groundCube.Position.Y + 1;
            }

            _physicSimu.Simulate(ref TimeSpend, out newWorldPosition);
            WorldPosition.Value = newWorldPosition;

            if (WorldPosition.Value.Y > _groundBelowEntity)
            {
                _physicSimu.OnGround = false;
            }
        }

        private void isCollidingWithTerrain(ref DVector3 newPosition2Evaluate, ref DVector3 previousPosition)
        {

            BoundingBox _boundingBox2Evaluate;
            DVector3 newPositionWithColliding = previousPosition;
            //Create a Bounding box with my new suggested position, taking only the X that has been changed !
            //X Testing
            newPositionWithColliding.X = newPosition2Evaluate.X;
            RefreshBoundingBox(ref newPositionWithColliding, out _boundingBox2Evaluate);
            if (_cubesHolder.IsSolidToPlayer(ref _boundingBox2Evaluate))
                newPositionWithColliding.X = previousPosition.X;

            //Y Testing
            newPositionWithColliding.Y = newPosition2Evaluate.Y;

            //My Position raise  ==> If I were on the ground, I'm no more
            if (previousPosition.Y < newPositionWithColliding.Y && _physicSimu.OnGround) _physicSimu.OnGround = false;

            RefreshBoundingBox(ref newPositionWithColliding, out _boundingBox2Evaluate);
            if (_cubesHolder.IsSolidToPlayer(ref _boundingBox2Evaluate))
            {
                //If Jummping
                if (previousPosition.Y < newPositionWithColliding.Y)
                {
                    newPositionWithColliding.Y = previousPosition.Y;
                }
                else //Falling
                {
                    newPositionWithColliding.Y = _groundBelowEntity;
                    previousPosition.Y = _groundBelowEntity; // ==> This way I stop the Y move !
                    _physicSimu.OnGround = true; // On ground ==> Activite the force that will counter the gravity !!
                }
            }

            //Z Testing
            newPositionWithColliding.Z = newPosition2Evaluate.Z;
            RefreshBoundingBox(ref newPositionWithColliding, out _boundingBox2Evaluate);
            if (_cubesHolder.IsSolidToPlayer(ref _boundingBox2Evaluate))
                newPositionWithColliding.Z = previousPosition.Z;

            newPosition2Evaluate = newPositionWithColliding;
        }
        #endregion
        #endregion
    }
}
