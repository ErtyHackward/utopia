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
using S33M3CoreComponents.Physics.Verlet;
using Utopia.Shared.Entities;
using Utopia.Action;

namespace Utopia.Entities.EntityMovement
{
    /// <summary>
    /// Helper class that will transform Input and environments signals into movements
    /// </summary>
    public class EntityRotations
    {
        #region Private Variables
        private EntityDisplacementModes _displacementMode;
        private InputsManager _inputsManager;

        private float _entityRotationSpeed;
        private VerletSimulator _physicSimu;

        private Vector3 _entityEyeXAxis = Vector3.UnitX;
        private Vector3 _entityEyeYAxis = Vector3.UnitY;
        private Vector3 _entityEyeZAxis = Vector3.UnitZ;
        private Vector3 _entityBodyXAxis = Vector3.UnitX;
        private Vector3 _entityBodyYAxis = Vector3.UnitY;
        private Vector3 _entityBodyZAxis = Vector3.UnitZ;
        private Vector3 _lookAt = new Vector3(0 , 0, 1);
        private Quaternion _eyeOrientation = new Quaternion();
        private Quaternion _bodyOrientation = new Quaternion();

        private float _accumPitchDegrees;
        private float _rotationDelta;
        private float _rotationDeltaAcum;
        #endregion

        #region Public Properties
        public float EntityRotationSpeed { get { return _entityRotationSpeed; } set { _entityRotationSpeed = value; } }
        public Vector3 LookAt { get { return _lookAt; } }
        public EntityDisplacementModes DisplacementMode { get { return _displacementMode; } }
        public Quaternion BodyOrientation { get { return _bodyOrientation; } }
        public Quaternion EyeOrientation { get { return _eyeOrientation; } }
        public Vector3 EntityMoveVector;
        #endregion

        public EntityRotations(InputsManager inputManager, VerletSimulator physicSimu)
        {
            _inputsManager = inputManager;
            _physicSimu = physicSimu;
        }

        #region Public Methods
        public void Update(GameTime timeSpent)
        {
            //Compute the deltas following the time elapsed : Speed * Time = Distance (Over the elapsed time).
            _rotationDelta = _entityRotationSpeed * timeSpent.ElapsedGameTimeInS_LD;

            //Rotation
            EntityRotation(timeSpent.ElapsedGameTimeInS_LD);

            //Movement Vector deduction from Rotation and Input handling
            EntityMovement();
        }

        public void SetDisplacementMode(EntityDisplacementModes newValue, Vector3D WorldEyePosition)
        {
            // Moving from flight behavior to first person behavior.
            // Need to ignore camera roll, but retain existing pitch and heading.
            if (_displacementMode == EntityDisplacementModes.FreeFlying && newValue == EntityDisplacementModes.Walking)
            {
                InitRotation(WorldEyePosition.AsVector3(), WorldEyePosition.AsVector3() + _entityEyeZAxis, Vector3.UnitY);
            }

            _displacementMode = newValue;
        }

        //Get the initial value of _eyeOrientation/_moveOrientation quaternion based on Quaternion value
        //This will generation a rotation without any roll if the movement mode is not Flying
        public void SetOrientation(Quaternion rotationValue, Vector3D WorldEyePosition)
        {
            if (_displacementMode == EntityDisplacementModes.FreeFlying)
            {
                Matrix rotationMatrix;
                Matrix.RotationQuaternion(ref rotationValue, out rotationMatrix);
                _eyeOrientation = rotationValue;
                _bodyOrientation = rotationValue;
                _accumPitchDegrees = (float)MathHelper.ToDegrees(Math.Asin(rotationMatrix.M23));
            }
            else
            {
                InitRotation(WorldEyePosition.AsVector3(), WorldEyePosition.AsVector3() + _lookAt, Vector3.UnitY);
            }
        }

        #endregion

        #region Private Methods

        #region Rotation
        //Get the initial value of _eyeOrientation/_moveOrientation quaternion based in position/lookat values
        //This will generation a rotation without any roll.
        private void InitRotation(Vector3 eye, Vector3 target, Vector3 up)
        {
            Matrix viewMatrix;
            Matrix.LookAtLH(ref eye, ref target, ref up, out viewMatrix);

            // Extract the pitch angle from the view matrix.
            _accumPitchDegrees = (float)MathHelper.ToDegrees(Math.Asin(viewMatrix.M23));

            //Set Rotation for both Eye View and Body rotation
            Quaternion.RotationMatrix(ref viewMatrix, out _eyeOrientation);
            _bodyOrientation = _eyeOrientation;
        }

        private void EntityRotation(float elapsedTime)
        {
            float headingDegrees = 0.0f;
            float pitchDegree = 0.0f;
            float rollDegree = 0.0f;
            bool hasRotated = false;

            if (_inputsManager.MouseManager.MouseCapture)
            {
                _rotationDeltaAcum += _rotationDelta; //Accumulate time
                if (_rotationDeltaAcum > 0.2f) _rotationDeltaAcum = _rotationDelta;

                switch (_displacementMode)
                {
                    case EntityDisplacementModes.Dead:
                    case EntityDisplacementModes.God:
                    case EntityDisplacementModes.Flying:
                    case EntityDisplacementModes.Walking:
                    case EntityDisplacementModes.Swiming:
                        headingDegrees = _inputsManager.MouseManager.MouseMoveDelta.X;
                        pitchDegree = _inputsManager.MouseManager.MouseMoveDelta.Y;

                        hasRotated = Rotate2Axes(headingDegrees, pitchDegree);
                        break;
                    case EntityDisplacementModes.FreeFlying:
                        //Get the movement direction from Keyboard input
                        EntityMoveVector = ExtractMovementVector();

                        headingDegrees = EntityMoveVector.X * 100 * elapsedTime;
                        pitchDegree = _inputsManager.MouseManager.MouseMoveDelta.Y;
                        rollDegree = _inputsManager.MouseManager.MouseMoveDelta.X;

                        hasRotated = Rotate3Axes(headingDegrees, pitchDegree, rollDegree);
                        break;
                    default:
                        break;
                }

                if (hasRotated)
                {
                    UpdateLookAt();
                    _rotationDeltaAcum = 0;
                }
            }
        }

        //Flying rotation
        private bool Rotate3Axes(float headingDegrees, float pitchDegrees, float rollDegrees)
        {
            if (headingDegrees == 0 && pitchDegrees == 0 && rollDegrees == 0) return false;

            headingDegrees *= _rotationDeltaAcum;
            pitchDegrees *= _rotationDeltaAcum;
            rollDegrees *= _rotationDeltaAcum;

            _accumPitchDegrees += pitchDegrees;

            if (_accumPitchDegrees > 360.0f) _accumPitchDegrees -= 360.0f;
            if (_accumPitchDegrees < -90.0f) _accumPitchDegrees += 360.0f;

            //To Gradiant
            float heading = MathHelper.ToRadians(headingDegrees);
            float pitch = MathHelper.ToRadians(pitchDegrees);
            float roll = MathHelper.ToRadians(rollDegrees);

            Quaternion rotation;
            Quaternion.RotationYawPitchRoll(heading, pitch, roll, out rotation);
            Quaternion.Multiply(ref rotation, ref _eyeOrientation, out _eyeOrientation);

            return true;
        }

        //First Person rotation
        //Free flying rotation
        //Swimming rotation
        private bool Rotate2Axes(float headingDegrees, float pitchDegrees)
        {
            if (headingDegrees == 0 && pitchDegrees == 0) return false;

            headingDegrees *= _rotationDeltaAcum;
            pitchDegrees *= _rotationDeltaAcum;

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
                Quaternion.Multiply(ref _eyeOrientation, ref rotation, out _eyeOrientation);
                Quaternion.Multiply(ref _bodyOrientation, ref rotation, out _bodyOrientation);
            }

            // Rotate camera about its local x axis.
            // Note the order the quaternions are multiplied. That is important!
            if (pitch != 0.0f)
            {
                Quaternion.RotationAxis(ref MVector3.Right, pitch, out rotation);
                Quaternion.Multiply(ref rotation , ref _eyeOrientation, out _eyeOrientation);
            }

            return true;
        }

        private void UpdateLookAt()
        {
            Matrix orientation;

            //Normalize the Camera Quaternion rotation
            Quaternion.Normalize(ref _eyeOrientation, out _eyeOrientation);
            //Extract the Rotation Matrix
            Matrix.RotationQuaternion(ref _eyeOrientation, out orientation);

            //Extract the 3 axis from the RotationMatrix
            _entityEyeXAxis = new Vector3(orientation.M11, orientation.M21, orientation.M31);
            _entityEyeYAxis = new Vector3(orientation.M12, orientation.M22, orientation.M32);
            _entityEyeZAxis = new Vector3(orientation.M13, orientation.M23, orientation.M33);

            //Extract the LookAtVector
            _lookAt = _entityEyeZAxis;

            //Normalize the Camera Quaternion rotation
            Quaternion.Normalize(ref _bodyOrientation, out _bodyOrientation);
            //Extract the Rotation Matrix
            Matrix.RotationQuaternion(ref _bodyOrientation, out orientation);

            //Extract the 3 axis from the RotationMatrix
            _entityBodyXAxis = new Vector3(orientation.M11, orientation.M21, orientation.M31);
            _entityBodyYAxis = new Vector3(orientation.M12, orientation.M22, orientation.M32);
            _entityBodyZAxis = new Vector3(orientation.M13, orientation.M23, orientation.M33);
        }
        #endregion

        #region Movements vector computation

        private void EntityMovement()
        {
            //Get movement Vector only if not freeFlying (Was done at the rotation time)
            EntityMoveVector = ExtractMovementVector();
        }

        private Vector3 ExtractMovementVector()
        {
            Vector3 entityMoveVector = Vector3.Zero;
            if (_inputsManager.ActionsManager.isTriggered(Actions.Move_Forward))
                switch (_displacementMode)
                {
                    case EntityDisplacementModes.God:
                    case EntityDisplacementModes.Swiming:
                    case EntityDisplacementModes.Dead:
                    case EntityDisplacementModes.Flying:
                        entityMoveVector += _lookAt;
                        break;
                    case EntityDisplacementModes.Walking:
                        entityMoveVector += _physicSimu.isInContactWithLadder ? _lookAt : _entityBodyZAxis;
                        break;
                    case EntityDisplacementModes.FreeFlying:
                        entityMoveVector.Z += 1.0f;
                        break;
                }

            if (_inputsManager.ActionsManager.isTriggered(Actions.Move_Backward))
                switch (_displacementMode)
                {
                    case EntityDisplacementModes.God:
                    case EntityDisplacementModes.Swiming:
                    case EntityDisplacementModes.Flying:
                    case EntityDisplacementModes.Dead:
                        entityMoveVector -= _lookAt;
                        break;
                    case EntityDisplacementModes.Walking:
                        entityMoveVector -= _physicSimu.isInContactWithLadder ? _lookAt : _entityBodyZAxis;
                        break;
                    case EntityDisplacementModes.FreeFlying:
                        entityMoveVector.Z -= 1.0f;
                        break;
                }

            if (_inputsManager.ActionsManager.isTriggered(Actions.Move_StrafeRight))
                switch (_displacementMode)
                {
                    case EntityDisplacementModes.FreeFlying:
                        entityMoveVector.X += 1.0f;
                        break;
                    case EntityDisplacementModes.God:
                    case EntityDisplacementModes.Flying:
                    case EntityDisplacementModes.Dead:
                        entityMoveVector += _entityEyeXAxis;
                        break;
                    default:
                        entityMoveVector += _physicSimu.isInContactWithLadder ? _entityEyeXAxis : _entityBodyXAxis;
                        break;
                }

            if (_inputsManager.ActionsManager.isTriggered(Actions.Move_StrafeLeft))
                switch (_displacementMode)
                {
                    case EntityDisplacementModes.FreeFlying:
                        entityMoveVector.X -= 1.0f;
                        break;
                    case EntityDisplacementModes.God:
                    case EntityDisplacementModes.Flying:
                    case EntityDisplacementModes.Dead:
                        entityMoveVector -= _entityEyeXAxis;
                        break;
                    default:
                        entityMoveVector -= _physicSimu.isInContactWithLadder ? _entityEyeXAxis : _entityBodyXAxis;
                        break;
                }

            if (_inputsManager.ActionsManager.isTriggered(Actions.Move_Up))
                switch (_displacementMode)
                {
                    case EntityDisplacementModes.God:
                    case EntityDisplacementModes.Flying:
                    case EntityDisplacementModes.Dead:
                        entityMoveVector += Vector3.UnitY;
                        break;                    
                    case EntityDisplacementModes.FreeFlying:
                        entityMoveVector.Y += 1.0f;
                        break;
                    case EntityDisplacementModes.Walking:
                        if (_physicSimu.isInContactWithLadder)
                        {
                            entityMoveVector.Y += 1.0f;
                        }
                        break;
                }

            if (_inputsManager.ActionsManager.isTriggered(Actions.Move_Down))
                switch (_displacementMode)
                {
                    case EntityDisplacementModes.God:
                    case EntityDisplacementModes.Flying:
                    case EntityDisplacementModes.Dead:
                        entityMoveVector -= Vector3.UnitY;
                        break;
                    case EntityDisplacementModes.FreeFlying:
                        entityMoveVector.Y -= 1.0f;
                        break;
                    case EntityDisplacementModes.Walking:
                        if (_physicSimu.isInContactWithLadder)
                        {
                            entityMoveVector.Y -= 1.0f;
                        }
                        break;
                }

            return Vector3.Normalize(entityMoveVector);
        }

        #endregion
        #endregion
    }
}
