using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using S33M3CoreComponents.Inputs;
using S33M3DXEngine.Main;
using SharpDX;
using S33M3CoreComponents.Inputs.Actions;
using S33M3CoreComponents.Maths;
using S33M3Resources.Structs;

namespace Utopia.Entities.EntityMovement
{
    /// <summary>
    /// Helper class that will transform Input and environments signals into movements
    /// </summary>
    public class EntityMovements
    {
        #region Private Variables
        private EntityMovementModes _movementMode;
        private InputsManager _inputsManager;

        private float _gravityInfluence;
        private float _gravityInfluenceOnGround = 1.0f;
        private float _gravityInfluenceFlying = 3.0f;
        private float _gravityInfluenceSwimming = 1.5f;

        private float _entityMoveSpeed;
        private float _entityRotationSpeed;
        private Vector3 _entityMoveVector;

        private Vector3 _entityEyeXAxis = Vector3.UnitX;
        private Vector3 _entityEyeYAxis = Vector3.UnitY;
        private Vector3 _entityEyeZAxis = Vector3.UnitZ;
        private Vector3 _entityXAxis  = Vector3.UnitX;
        private Vector3 _entityYAxis = Vector3.UnitY;
        private Vector3 _entityZAxis = Vector3.UnitZ;
        private Vector3 _lookAt;
        private Quaternion _eyeOrientation, _moveOrientation;
        private Vector3D _worldPosition;
        private Vector3 _eyeOffset;
        private float _accumPitchDegrees;

        private float _moveDelta;
        private float _rotationDelta;
        #endregion

        #region Public Properties
        public float GravityInfluenceOnGround { get { return _gravityInfluenceOnGround; } set { _gravityInfluenceOnGround = value; } }
        public float GravityInfluenceFlying { get { return _gravityInfluenceFlying; } set { _gravityInfluenceFlying = value; } }
        public float GravityInfluenceSwimming { get { return _gravityInfluenceSwimming; } set { _gravityInfluenceSwimming = value; } }
        public float EntityMoveSpeed { get { return _entityMoveSpeed; } set { _entityMoveSpeed = value; } }
        public float EntityRotationSpeed { get { return _entityRotationSpeed; } set { _entityRotationSpeed = value; } }
        public Vector3 LookAt { get { return _lookAt; } }
        public EntityMovementModes MovementMode { get { return _movementMode; } set { SetNewMovementMode(value); } }
        public Quaternion MoveOrientation { get { return _moveOrientation; } set { _moveOrientation = value; RefreshRotation(WorldEyePosition.AsVector3(), WorldEyePosition.AsVector3() + _lookAt, Vector3.UnitY); } }
        public Quaternion EyeOrientation { get { return _eyeOrientation; } }
        public Vector3D WorldPosition { get { return _worldPosition; } set { _worldPosition = value; } }
        public Vector3D WorldEyePosition { get { return _worldPosition + _eyeOffset; } }
        public Vector3 EyeOffset { get { return _eyeOffset; } set { _eyeOffset = value; } }
        #endregion

        public EntityMovements(InputsManager inputManager, EntityMovementModes initialMovementMode = EntityMovementModes.Walking)
        {
            _inputsManager = inputManager;
            MovementMode = initialMovementMode;
        }

        #region Public Methods
        public void Update(GameTime timeSpent)
        {
            //Compute the deltas following the time elapsed : Speed * Time = Distance (Over the elapsed time).
            _moveDelta = _entityMoveSpeed * _gravityInfluence * timeSpent.ElapsedGameTimeInS_LD;
            _rotationDelta = _entityRotationSpeed * timeSpent.ElapsedGameTimeInS_LD;

            //Rotation with mouse
            EntityRotation(timeSpent.ElapsedGameTimeInS_LD);

            //Movement
            EntityMovement(timeSpent.ElapsedGameTimeInS_LD);
        }
        #endregion

        #region Private Methods
        private Vector3 ExtractMovementVector()
        {
            Vector3 entityMoveVector = Vector3.Zero;
            if (_inputsManager.ActionsManager.isTriggered(Actions.Move_Forward))
                switch (_movementMode)
                {
                    case EntityMovementModes.Swiming:
                    case EntityMovementModes.Flying:
                        entityMoveVector += _lookAt;
                        break;
                    case EntityMovementModes.Walking:
                        entityMoveVector += _entityZAxis;
                        break;
                    case EntityMovementModes.FreeFlying:
                        entityMoveVector.Z += 1.0f;
                        break;
                }

            if (_inputsManager.ActionsManager.isTriggered(Actions.Move_Backward))
                switch (_movementMode)
                {
                    case EntityMovementModes.Swiming:
                    case EntityMovementModes.Flying:
                        entityMoveVector -= _lookAt;
                        break;
                    case EntityMovementModes.Walking:
                        entityMoveVector -= _entityZAxis;
                        break;
                    case EntityMovementModes.FreeFlying:
                        entityMoveVector.Z -= 1.0f;
                        break;
                }

            if (_inputsManager.ActionsManager.isTriggered(Actions.Move_StrafeRight))
                switch (_movementMode)
                {
                    case EntityMovementModes.FreeFlying:
                        entityMoveVector.X += 1.0f;
                        break;
                    default :
                        entityMoveVector += _entityEyeXAxis;
                        break;
                }

            if (_inputsManager.ActionsManager.isTriggered(Actions.Move_StrafeLeft))
                switch (_movementMode)
                {
                    case EntityMovementModes.FreeFlying:
                        entityMoveVector.X -= 1.0f;
                        break;
                    default:
                        entityMoveVector -= _entityEyeXAxis;
                        break;
                }

            if (_inputsManager.ActionsManager.isTriggered(Actions.Move_Up))
                switch (_movementMode)
                {
                    case EntityMovementModes.Flying:
                        entityMoveVector += Vector3.UnitY;
                        break;
                    case EntityMovementModes.FreeFlying:
                        entityMoveVector.Y += 1.0f;
                        break;
                }

            if (_inputsManager.ActionsManager.isTriggered(Actions.Move_Down))
                switch (_movementMode)
                {
                    case EntityMovementModes.Flying:
                        entityMoveVector -= Vector3.UnitY;
                        break;
                    case EntityMovementModes.FreeFlying:
                        entityMoveVector.Y -= 1.0f;
                        break;
                }

            return Vector3.Normalize(entityMoveVector);
        }

        private void SetNewMovementMode(EntityMovementModes newValue)
        {
            switch (newValue)
            {
                case EntityMovementModes.Flying:
                    _gravityInfluence = _gravityInfluenceFlying;
                    break;
                case EntityMovementModes.Walking:
                    _gravityInfluence = _gravityInfluenceOnGround;
                    break;
                case EntityMovementModes.Swiming:
                    _gravityInfluence = _gravityInfluenceSwimming;
                    break;
                default:
                    _gravityInfluence = _gravityInfluenceOnGround;
                    break;
            }

            // Moving from flight behavior to first person behavior.
            // Need to ignore camera roll, but retain existing pitch and heading.
            if (_movementMode == EntityMovementModes.Flying && newValue == EntityMovementModes.Walking)
            {
                RefreshRotation(WorldEyePosition.AsVector3(), WorldEyePosition.AsVector3() + _entityEyeZAxis, Vector3.UnitY);
            }

            _movementMode = newValue;
        }

        #region Rotation
        private void RefreshRotation(Vector3 eye, Vector3 target, Vector3 up)
        {
            Matrix viewMatrix;
            Matrix.LookAtLH(ref eye, ref target, ref up, out viewMatrix);

            // Extract the pitch angle from the view matrix.
            _accumPitchDegrees = (float)Math.Asin(viewMatrix.M23);
            //Set Rotation for both Eye View and Body rotation
            Quaternion.RotationMatrix(ref viewMatrix, out _eyeOrientation);
            _moveOrientation = _eyeOrientation;
        }

        private void EntityRotation(float elapsedTime)
        {
            float headingDegrees = 0.0f;
            float pitchDegree = 0.0f;
            float rollDegree = 0.0f;
            if (_inputsManager.MouseManager.MouseCapture)
            {
                switch (_movementMode)
                {
                    case EntityMovementModes.Flying:
                    case EntityMovementModes.Walking:
                    case EntityMovementModes.Swiming:
                        headingDegrees = _inputsManager.MouseManager.MouseMoveDelta.X;
                        pitchDegree = _inputsManager.MouseManager.MouseMoveDelta.Y;
                        rollDegree = 0.0f;
                        break;
                    case EntityMovementModes.FreeFlying:
                        //Get the movement direction from Keyboard input
                        _entityMoveVector = ExtractMovementVector();

                        headingDegrees = _entityMoveVector.X * 100 * elapsedTime;
                        pitchDegree = _inputsManager.MouseManager.MouseMoveDelta.Y;
                        rollDegree = _inputsManager.MouseManager.MouseMoveDelta.X;
                        break;
                    default:
                        break;
                }

                Rotate(headingDegrees, pitchDegree, rollDegree);
            }
        }

        private void Rotate(float headingDegrees, float pitchDegrees, float rollDegrees)
        {
            //If not movement => Do nothing
            if (headingDegrees == 0 && pitchDegrees == 0 && rollDegrees == 0) return;

            //Affect mouse sensibility stored in Delta to the mouvement that has been realized
            headingDegrees *= _rotationDelta;
            pitchDegrees *= _rotationDelta;
            rollDegrees *= _rotationDelta;

            Rotate(headingDegrees, pitchDegrees);
        }

        private void Rotate(float headingDegrees, float pitchDegrees)
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

            //To Gradiant
            float heading = MathHelper.ToRadians(headingDegrees);
            float pitch = MathHelper.ToRadians(pitchDegrees);

            Quaternion rotation;

            // Rotate camera about the world y axis.
            // Note the order the quaternions are multiplied. That is important!
            if (heading != 0.0f)
            {
                Quaternion.RotationAxis(ref MVector3.Up, heading, out rotation);
                Quaternion.Multiply(ref rotation, ref _eyeOrientation, out _eyeOrientation);
                Quaternion.Multiply(ref rotation, ref _moveOrientation, out _moveOrientation);
            }

            // Rotate camera about its local x axis.
            // Note the order the quaternions are multiplied. That is important!
            if (pitch != 0.0f)
            {
                Quaternion.RotationAxis(ref MVector3.Right, pitch, out rotation);
                Quaternion.Multiply(ref _eyeOrientation, ref rotation, out _eyeOrientation);
            }

        }

        #endregion

        #region Movements
        private void EntityMovement(float elapsedTime)
        {
            //Get movement Vector only if not freeFlying (Was done at the rotation time)
            _entityMoveVector = ExtractMovementVector();
        }
        #endregion
        #endregion
    }
}
