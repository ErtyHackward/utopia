using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using S33M3CoreComponents.Inputs;
using S33M3DXEngine.Main;
using SharpDX;
using S33M3CoreComponents.Inputs.Actions;
using S33M3CoreComponents.Maths;

namespace S33M3CoreComponents.EntityMovement
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
        private float _gravityInfluenceOnGround = 1.0;
        private float _gravityInfluenceFlying = 3.0;
        private float _gravityInfluenceSwimming = 1.5;

        private float _entityMoveSpeed;
        private float _entityRotationSpeed;
        private Vector3 _entityMoveVector;

        private Vector3 _entityEyeXAxis, _entityEyeYAxis, _entityEyeZAxis;
        private Vector3 _entityXAxis, _entityYAxis, _entityZAxis;
        private Vector3 _lookAt;
        private Quaternion _eyeOrientation, _moveOrientation;

        private float _accumPitchDegrees;

        private float _moveDelta;
        private float _rotationDelta;
        #endregion

        #region Public Properties
        public EntityMovementModes MovementMode
        {
            get { return _movementMode; }
            set
            {
                switch (value)
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
                _movementMode = value;
            }
        }
    
        public float GravityInfluenceOnGround { get { return _gravityInfluenceOnGround; } set { _gravityInfluenceOnGround = value; } }
        public float GravityInfluenceFlying { get { return _gravityInfluenceFlying; } set { _gravityInfluenceFlying = value; } }
        public float GravityInfluenceSwimming { get { return _gravityInfluenceSwimming; } set { _gravityInfluenceSwimming = value; } }
        public float EntityMoveSpeed { get { return _entityMoveSpeed; } set { _entityMoveSpeed = value; } }
        public float EntityRotationSpeed { get { return _entityRotationSpeed; } set { _entityRotationSpeed = value; } }
        public Vector3 LookAt { get { return _lookAt; } }
        #endregion

        public EntityMovements(InputsManager inputManager)
        {
            _inputsManager = inputManager;
        }

        #region Public Methods
        public void Update(GameTime timeSpent)
        {
            //Compute the deltas following the time elapsed : Speed * Time = Distance (Over the elapsed time).
            _moveDelta = _entityMoveSpeed * _gravityInfluence * timeSpent.ElapsedGameTimeInS_LD;
            _rotationDelta = _entityRotationSpeed * timeSpent.ElapsedGameTimeInS_LD;

            //Rotation with mouse
            EntityRotations(timeSpent.ElapsedGameTimeInS_LD);

            //Movement
            EntityMovements(timeSpent.ElapsedGameTimeInS_LD);
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

        #region ROTATION
        private void EntityRotations(float elapsedTime)
        {
            double headingDegrees = 0.0;
            double pitchDegree = 0.0;
            double rollDegree = 0.0;
            if (_inputsManager.MouseManager.MouseCapture)
            {
                switch (_movementMode)
                {
                    case EntityMovementModes.Flying:
                    case EntityMovementModes.Walking:
                    case EntityMovementModes.Swiming:
                        headingDegrees = _inputsManager.MouseManager.MouseMoveDelta.X;
                        pitchDegree = _inputsManager.MouseManager.MouseMoveDelta.Y * -1;
                        rollDegree = 0.0;
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

        }
        #endregion

        #region Movements
        private void EntityMovements(float elapsedTime)
        {
            //Get movement Vector only if not freeFlying (Was done at the rotation time)
            if (_movementMode != EntityMovementModes.FreeFlying)
            {
                _entityMoveVector = ExtractMovementVector();
            }


        }
        #endregion
        #endregion
    }
}
