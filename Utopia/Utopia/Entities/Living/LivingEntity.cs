using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using S33M3Engines.Struct;
using SharpDX;
using S33M3Engines.D3D;
using S33M3Engines.InputHandler;
using S33M3Engines.InputHandler.MouseHelper;
using S33M3Engines.Maths;
using System.Windows.Forms;
using ButtonState = S33M3Engines.InputHandler.MouseHelper.ButtonState;
using S33M3Physics.Euler;
using S33M3Physics;
using S33M3Engines.D3D.DebugTools;
using S33M3Physics.Verlet;
using Utopia.Shared.Structs;
using Utopia.Shared.Structs.Landscape;
using Utopia.Shared;
using Utopia.Settings;
using S33M3Engines.Shared.Math;
using S33M3Engines;
using S33M3Engines.Cameras;
using Utopia.Shared.Chunks;
using Utopia.Shared.Cubes;

namespace Utopia.Entities.Living
{
    // A Living Entity, is mainly defined by its :
    // - Possibility to look at something (lookAt point, and lookAt rotation)
    // - A living entity contains the code needed for it to move !
    public class LivingEntity : Entity, ILivingEntity
    {
        #region private variables
        protected CameraManager _camManager;
        protected D3DEngine _d3dEngine;
        private Vector3 _lookAt = new Vector3(0f, 0f, -1f);
        private FTSValue<Quaternion> _moveDirection = new FTSValue<Quaternion>();
        private float _moveSpeed;
        private float _walkingSpeed;
        private float _flyingSpeed;
        private float _moveRotationSpeed;
        private float _headRotationSpeed;
        private float _moveDelta;
        private float _headRotationDelta;
        private float _accumPitchDegrees;
        private Vector3 _entityHeadXAxis, _entityHeadYAxis, _entityHeadZAxis;
        private Vector3 _entityXAxis, _entityYAxis, _entityZAxis;
        private Matrix _headRotation, _entityRotation;
        private LivingEntityMode _moveMode = LivingEntityMode.WalkingFirstPerson;
        private float _groundBelowEntity;
        private bool _headInsideWater = false;
        protected InputHandlerManager _inputHandler;
        float _gravityInfluence;

        private VerletSimulator _physicSimu;
        private RefreshHeadUnderWaterDelegate _refreshHeadUnderWater;
        

        #endregion

        #region Public properties
        public Vector3 LookAt { get { return _lookAt; } }
        public FTSValue<Quaternion> MoveDirection { get { return _moveDirection; } }

        public bool HeadInsideWater { get { return _headInsideWater; } set { _headInsideWater = value; } }
        public float MoveSpeed { get { return _moveSpeed; } set { _moveSpeed = value; } }
        public float WalkingSpeed { get { return _walkingSpeed; } set { _walkingSpeed = value; } }
        public float FlyingSpeed { get { return _flyingSpeed; } set { _flyingSpeed = value; } }
        public float MoveRotationSpeed { get { return _moveRotationSpeed; } set { _moveRotationSpeed = value; } }
        public float HeadRotationSpeed { get { return _headRotationSpeed; } set { _headRotationSpeed = value; } }
        protected SingleArrayChunkContainer CubesHolder { get; set; }
        public delegate void RefreshHeadUnderWaterDelegate();

        public RefreshHeadUnderWaterDelegate RefreshHeadUnderWater{ get { return _refreshHeadUnderWater; } set { _refreshHeadUnderWater = value; } }

        public LivingEntityMode Mode
        {
            get { return _moveMode; }
            set
            {
                _moveMode = value;
                if (_moveMode == LivingEntityMode.WalkingFirstPerson)
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

        public LivingEntity(D3DEngine d3dEngine, CameraManager camManager, InputHandlerManager inputHandler, DVector3 startUpWorldPosition, Vector3 size, float walkingSpeed, float flyingSpeed, float headRotationSpeed, SingleArrayChunkContainer cubesHolder)
            : base(startUpWorldPosition, size)
        {
            _camManager = camManager;
            _d3dEngine = d3dEngine;
            _refreshHeadUnderWater = CheckHeadUnderWater;

            _inputHandler = inputHandler;

            _walkingSpeed = walkingSpeed;
            _flyingSpeed = flyingSpeed;
            _moveSpeed = _flyingSpeed;

            CubesHolder = cubesHolder;

            _headRotationSpeed = headRotationSpeed;
            _moveRotationSpeed = 0;

            LookAtDirection.Value = Quaternion.Identity;
            MoveDirection.Value = Quaternion.Identity;

            _entityHeadXAxis = Vector3.UnitX;
            _entityHeadYAxis = Vector3.UnitY;
            _entityHeadZAxis = Vector3.UnitZ;

            _entityXAxis = Vector3.UnitX;
            _entityYAxis = Vector3.UnitY;
            _entityZAxis = Vector3.UnitZ;

            _physicSimu = new VerletSimulator(ref base._boundingBox) { WithCollisionBounsing = false };
            _physicSimu.ConstraintFct += isCollidingWithTerrain;


        }

        public override void Initialize()
        {
        }

        #region Update
        TerraCube _headCube;
        int _headCubeIndex;
        protected void CheckHeadUnderWater()
        {
            if (CubesHolder.IndexSafe(MathHelper.Fastfloor(CameraWorldPosition.X), MathHelper.Fastfloor(CameraWorldPosition.Y), MathHelper.Fastfloor(CameraWorldPosition.Z), out _headCubeIndex))
            {
                //Get the cube at the camera position !
                _headCube = CubesHolder.Cubes[_headCubeIndex];
                if (_headCube.Id == CubeId.Water || _headCube.Id == CubeId.WaterSource)
                {
                    //Take into account the Offseting in case of Offseted Water !
                    HeadInsideWater = true;
                }
                else
                {
                    HeadInsideWater = false;
                }
            }
        }

        public override void Update(ref GameTime TimeSpend)
        {
            //Compute the amount of movement to take into account for this Update !
            
            //Compute Gravity Influence 
            _gravityInfluence = MathHelper.FullLerp(1, 10, 0, 300, Math.Min(Math.Max((float)_camManager.ActiveCamera.WorldPosition.Y - 300, 0),300));

            _moveDelta = _moveSpeed * _gravityInfluence * TimeSpend.ElapsedGameTimeInS_LD;

            _headRotationDelta = HeadRotationSpeed * TimeSpend.ElapsedGameTimeInS_LD;

            //Backup values
            LookAtDirection.BackUpValue();
            WorldPosition.BackUpValue();
            WorldRotation.BackUpValue();

            //Head + Body rotation with mouse
            EntityRotationsOnEvents(Mode);

            //Keybord Movement
            EntityMovementsOnEvents(Mode, ref TimeSpend);

            //Physic simulation !
            PhysicOnEntity(Mode, ref TimeSpend);

            base.Update(ref TimeSpend);
        }

        public override void Interpolation(ref double interpolation, ref float interpolation_ld)
        {
            DVector3.Lerp(ref WorldPosition.ValuePrev, ref WorldPosition.Value, interpolation, out WorldPosition.ValueInterp);
            Quaternion.Slerp(ref LookAtDirection.ValuePrev, ref LookAtDirection.Value, interpolation_ld, out LookAtDirection.ValueInterp);
            Quaternion.Slerp(ref MoveDirection.ValuePrev, ref MoveDirection.Value, interpolation_ld, out MoveDirection.ValueInterp);
            Quaternion.Slerp(ref WorldRotation.ValuePrev, ref WorldRotation.Value, interpolation_ld, out WorldRotation.ValueInterp);
        }
        #endregion

        #region private methods

        #region Movement Management
        private void EntityMovementsOnEvents(LivingEntityMode mode,ref GameTime TimeSpend)
        {
            switch (mode)
            {
                case LivingEntityMode.FreeFirstPerson:
                    FreeFirstPersonMove();
                    break;
                case LivingEntityMode.WalkingFirstPerson:
                    WalkingFirstPerson(ref TimeSpend);
                    break;
                default:
                    break;
            }
        }

        private void FreeFirstPersonMove()
        {
            if (_inputHandler.CurKeyboardState.IsKeyDown(ClientSettings.Current.Settings.KeyboardMapping.Move.Forward) || (_inputHandler.CurMouseState.LeftButton == ButtonState.Pressed && _inputHandler.CurMouseState.RightButton == ButtonState.Pressed))
                WorldPosition.Value += _lookAt * _moveDelta;

            if (_inputHandler.CurKeyboardState.IsKeyDown(ClientSettings.Current.Settings.KeyboardMapping.Move.Backward))
                WorldPosition.Value -= _lookAt * _moveDelta;

            if (_inputHandler.CurKeyboardState.IsKeyDown(ClientSettings.Current.Settings.KeyboardMapping.Move.StrafeLeft))
                WorldPosition.Value -= _entityHeadXAxis * _moveDelta;

            if (_inputHandler.CurKeyboardState.IsKeyDown(ClientSettings.Current.Settings.KeyboardMapping.Move.StrafeRight))
                WorldPosition.Value += _entityHeadXAxis * _moveDelta;

            if (_inputHandler.CurKeyboardState.IsKeyDown(ClientSettings.Current.Settings.KeyboardMapping.Move.Down))
                WorldPosition.Value += MVector3.Down * _moveDelta;

            if (_inputHandler.CurKeyboardState.IsKeyDown(ClientSettings.Current.Settings.KeyboardMapping.Move.Up))
                WorldPosition.Value += MVector3.Up * _moveDelta;
        }

        private void WalkingFirstPerson(ref GameTime TimeSpend)
        {
            _physicSimu.Freeze(true, false, true);

            if (!_physicSimu.OnGround) _moveDelta /= 3f;

            if (_inputHandler.CurKeyboardState.IsKeyDown(ClientSettings.Current.Settings.KeyboardMapping.Move.Forward) || (_inputHandler.CurMouseState.LeftButton == ButtonState.Pressed && _inputHandler.CurMouseState.RightButton == ButtonState.Pressed))
                if (_inputHandler.CurKeyboardState.IsKeyDown(ClientSettings.Current.Settings.KeyboardMapping.Move.Run)) _physicSimu.PrevPosition += _entityZAxis * _moveDelta * 2f;
                else _physicSimu.PrevPosition += _entityZAxis * _moveDelta;

            if (_inputHandler.CurKeyboardState.IsKeyDown(ClientSettings.Current.Settings.KeyboardMapping.Move.Backward))
                _physicSimu.PrevPosition -= _entityZAxis * _moveDelta;

            if (_inputHandler.CurKeyboardState.IsKeyDown(ClientSettings.Current.Settings.KeyboardMapping.Move.StrafeLeft))
                _physicSimu.PrevPosition += _entityXAxis * _moveDelta;

            if (_inputHandler.CurKeyboardState.IsKeyDown(ClientSettings.Current.Settings.KeyboardMapping.Move.StrafeRight))
                _physicSimu.PrevPosition -= _entityXAxis * _moveDelta;

            if (_inputHandler.PrevKeyboardState.IsKeyDown(ClientSettings.Current.Settings.KeyboardMapping.Move.Jump))
                //Only If I'm on the ground !
                if (_physicSimu.OnGround)
                    _physicSimu.Impulses.Add(new Impulse(ref TimeSpend) { ForceApplied = new DVector3(0, 300, 0) });
        }
        #endregion

        #region Head + Body Rotation management
        private void EntityRotationsOnEvents(LivingEntityMode mode)
        {
            MouseState mouseState;
            int centerX = (int)_camManager.ActiveCamera.Viewport.Width / 2; // Largeur Viewport pour centrer la souris !
            int centerY = (int)_camManager.ActiveCamera.Viewport.Height / 2;
            if (_d3dEngine.UnlockedMouse == false)
            {
                _inputHandler.GetCurrentMouseState(out mouseState); //To be sure the take the latest place of the mouse cursor !
                Mouse.SetPosition(centerX, centerY);
                Rotate((mouseState.X - centerX), (mouseState.Y - centerY), 0.0f, mode);
            }
        }

        private void Rotate(float headingDegrees, float pitchDegrees, float rollDegrees, LivingEntityMode mode)
        {
            if (headingDegrees == 0 && pitchDegrees == 0 && rollDegrees == 0) return;

            headingDegrees *= _headRotationDelta;
            pitchDegrees *= _headRotationDelta;
            rollDegrees *= _headRotationDelta;

            switch (mode)
            {
                case LivingEntityMode.FreeFirstPerson:
                    RotateLookAt(headingDegrees, pitchDegrees);
                    RotateMove(headingDegrees);
                    break;
                case LivingEntityMode.WalkingFirstPerson:
                    RotateLookAt(headingDegrees, pitchDegrees);
                    RotateMove(headingDegrees);
                    break;
                default:
                    break;
            }
        }

        private void RotateMove(float headingDegrees)
        {
            float heading = MathHelper.ToRadians(headingDegrees);
            Quaternion rotation;

            // Rotate the camera about the world Y axis.
            if (heading != 0.0f)
            {
                Quaternion.RotationAxis(ref MVector3.Up, heading, out rotation);
                _moveDirection.Value = rotation * _moveDirection.Value;
            }

            _moveDirection.Value.Normalize();

            UpdateEntityData();
        }
        private void UpdateEntityData()
        {
            Matrix.RotationQuaternion(ref _moveDirection.Value, out _entityRotation);

            _entityXAxis = new Vector3(_entityRotation.M11, _entityRotation.M21, _entityRotation.M31);
            _entityYAxis = new Vector3(_entityRotation.M12, _entityRotation.M22, _entityRotation.M32);
            _entityZAxis = new Vector3(_entityRotation.M13, _entityRotation.M23, _entityRotation.M33);
        }

        private void RotateLookAt(float headingDegrees, float pitchDegrees)
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

            float heading = MathHelper.ToRadians(headingDegrees);
            float pitch = MathHelper.ToRadians(pitchDegrees);
            Quaternion rotation;

            // Rotate the camera about the world Y axis.
            if (heading != 0.0f)
            {
                Quaternion.RotationAxis(ref MVector3.Up, heading, out rotation);
                _lookAtDirection.Value = rotation * _lookAtDirection.Value;
            }

            // Rotate the camera about its local X axis.
            if (pitch != 0.0f)
            {
                Quaternion.RotationAxis(ref MVector3.Right, pitch, out rotation);
                _lookAtDirection.Value = _lookAtDirection.Value * rotation;
            }

            _lookAtDirection.Value.Normalize();

            UpdateHeadData();
        }
        private void UpdateHeadData()
        {
            Matrix.RotationQuaternion(ref _lookAtDirection.Value, out _headRotation);

            _entityHeadXAxis = new Vector3(_headRotation.M11, _headRotation.M21, _headRotation.M31);
            _entityHeadYAxis = new Vector3(_headRotation.M12, _headRotation.M22, _headRotation.M32);
            _entityHeadZAxis = new Vector3(_headRotation.M13, _headRotation.M23, _headRotation.M33);

            _lookAt = new Vector3(-_entityHeadZAxis.X, -_entityHeadZAxis.Y, -_entityHeadZAxis.Z);
            _lookAt.Normalize();
        }
        #endregion

        #region EarlyPhysic Simulation
        private void PhysicOnEntity(LivingEntityMode mode, ref GameTime TimeSpend)
        {
            switch (mode)
            {
                case LivingEntityMode.FreeFirstPerson:
                    break;
                case LivingEntityMode.WalkingFirstPerson:
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

            CubesHolder.GetNextSolidBlockToPlayer(ref _boundingBox, ref GroundDirection, out groundCube);
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
            if (CubesHolder.IsSolidToPlayer(ref _boundingBox2Evaluate)) 
                newPositionWithColliding.X = previousPosition.X;

            //Y Testing
            newPositionWithColliding.Y = newPosition2Evaluate.Y;

            //My Position raise  ==> If I were on the ground, I'm no more
            if (previousPosition.Y < newPositionWithColliding.Y && _physicSimu.OnGround) _physicSimu.OnGround = false;

            RefreshBoundingBox(ref newPositionWithColliding, out _boundingBox2Evaluate);
            if (CubesHolder.IsSolidToPlayer(ref _boundingBox2Evaluate))
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
            if (CubesHolder.IsSolidToPlayer(ref _boundingBox2Evaluate)) 
                newPositionWithColliding.Z = previousPosition.Z;

            newPosition2Evaluate = newPositionWithColliding;
        }
        #endregion

        #endregion

        #region IDebugInfo Members

        public virtual string GetInfo()
        {
            return "";
        }

        #endregion
    }
}
