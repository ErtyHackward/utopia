﻿using System;
using S33M3CoreComponents.Cameras;
using S33M3CoreComponents.Cameras.Interfaces;
using S33M3CoreComponents.Inputs;
using S33M3CoreComponents.Inputs.Actions;
using S33M3CoreComponents.Maths;
using S33M3DXEngine.Main;
using S33M3Resources.Structs;
using SharpDX;
using Utopia.Entities.Managers.Interfaces;
using Utopia.Shared.Chunks;
using Utopia.Shared.Entities;
using Utopia.Shared.Entities.Dynamic;
using Utopia.Shared.Entities.Interfaces;
using Utopia.Shared.Structs.Helpers;
using Utopia.Shared.World;

namespace Utopia.Entities.Managers
{
    /// <summary>
    /// Responsible for player input handling in god mode.
    /// Supports different levels and allows to switch between them.
    /// Handles entity picking from the camera position.
    /// Should be used only with 3rd person camera.
    /// </summary>
    public class GodEntityManager : GameComponent, IPlayerManager
    {
        private readonly InputsManager _inputsManager;
        private readonly SingleArrayChunkContainer _cubesHolder;
        private readonly CameraManager<ICameraFocused> _cameraManager;
        private readonly LandscapeBufferManager _bufferManager;
        private readonly VisualWorldParameters _visParameters;

        private Vector3 _moveVector;

        private Quaternion _eyeOrientation;
        private Quaternion _bodyOrientation;

        private float _accumPitchDegrees;
        private float _rotationDelta;
        private float _rotationDeltaAcum;

        /// <summary>
        /// Gets or sets current focus point. In level mode this entity could move only in horisontal plane.
        /// If level mode is disabled the entity will move over the top surface of the chunk.
        /// </summary>
        public PlayerFocusEntity FocusEntity { get; set; }

        public IDynamicEntity Player { get { return FocusEntity; } }

        public bool IsHeadInsideWater { get { return false; } }

        /// <summary>
        /// If enabled uses certain level for entity/block picking.
        /// Otherwise uses top most one.
        /// </summary>
        public bool LevelMode { get; set; }
        
        #region ICameraPlugin

        // this region contain camera specific properties
        // these properties controlls the ThirdPerson camera position and rotation

        public Vector3D CameraWorldPosition { get { return FocusEntity.Position; } }
        public Quaternion CameraOrientation { get { return FocusEntity.HeadRotation; } }
        public Quaternion CameraYAxisOrientation { get { return FocusEntity.BodyRotation; } }
        public int CameraUpdateOrder { get; private set; }
        #endregion

        public GodEntityManager(PlayerFocusEntity playerEntity, 
                                InputsManager inputsManager, 
                                SingleArrayChunkContainer cubesHolder,
                                CameraManager<ICameraFocused> cameraManager,
                                LandscapeBufferManager bufferManager,
                                VisualWorldParameters visParameters)
        {
            if (playerEntity  == null) throw new ArgumentNullException("playerEntity");
            if (inputsManager == null) throw new ArgumentNullException("inputsManager");
            if (cubesHolder   == null) throw new ArgumentNullException("cubesHolder");
            if (cameraManager == null) throw new ArgumentNullException("cameraManager");
            if (bufferManager == null) throw new ArgumentNullException("bufferManager");
            if (visParameters == null) throw new ArgumentNullException("visualWorldParameters");

            FocusEntity = playerEntity;

            _eyeOrientation  = FocusEntity.HeadRotation;
            _bodyOrientation = FocusEntity.BodyRotation;

            _inputsManager = inputsManager;
            _cubesHolder   = cubesHolder;
            _cameraManager = cameraManager;
            _bufferManager = bufferManager;
            _visParameters = visParameters;
        }

        public override void VTSUpdate(double interpolationHd, float interpolationLd, float elapsedTime)
        {
            #region handle movement of the focus point

            InputHandling();

            _rotationDelta = 2f * elapsedTime;

            EntityRotation(elapsedTime);

            var speed = elapsedTime * 30;

            // apply movement
            FocusEntity.Position += _moveVector * speed;

            // validate new position if not in level mode
            if (!LevelMode)
            {
                // slide by camera lookat vector
                var lookVector = Vector3.Transform(Vector3.UnitZ, FocusEntity.HeadRotation);

                //_cubesHolder.GetCube(FocusEntity.Position, false)

                // TODO: check entity for surface collision, take into account camera position and view angle
            }

            #endregion

            _bufferManager.CleanUpClient(BlockHelper.EntityToChunkPosition(FocusEntity.Position), _visParameters);

            base.VTSUpdate(interpolationHd, interpolationLd, elapsedTime);
        }

        private void InputHandling()
        {
            var moveVector = Vector3.Zero;

            if (_inputsManager.ActionsManager.isTriggered(Actions.Move_Forward))
            {
                moveVector.Z = 1;
            }

            if (_inputsManager.ActionsManager.isTriggered(Actions.Move_Backward))
            {
                moveVector.Z = -1;
            }

            if (_inputsManager.ActionsManager.isTriggered(Actions.Move_StrafeLeft))
            {
                moveVector.X = -1;
            }

            if (_inputsManager.ActionsManager.isTriggered(Actions.Move_StrafeRight))
            {
                moveVector.X = 1;
            }

            moveVector.Normalize();

            Quaternion inv = FocusEntity.HeadRotation;

            inv.Invert();

            inv.X = 0;
            inv.Z = 0;

            inv.Normalize();

            _moveVector = Vector3.Transform(moveVector, inv);
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

                headingDegrees = _inputsManager.MouseManager.MouseMoveDelta.X;
                pitchDegree = _inputsManager.MouseManager.MouseMoveDelta.Y;

                hasRotated = Rotate2Axes(headingDegrees, pitchDegree);


                if (hasRotated)
                {
                    UpdateLookAt();
                    _rotationDeltaAcum = 0;
                }
            }
        }

        private void UpdateLookAt()
        {
            Matrix orientation;

            //Normalize the Camera Quaternion rotation
            Quaternion.Normalize(ref _eyeOrientation, out _eyeOrientation);
            //Extract the Rotation Matrix
            Matrix.RotationQuaternion(ref _eyeOrientation, out orientation);
            
            //Normalize the Camera Quaternion rotation
            Quaternion.Normalize(ref _bodyOrientation, out _bodyOrientation);
            //Extract the Rotation Matrix
            Matrix.RotationQuaternion(ref _bodyOrientation, out orientation);

            FocusEntity.HeadRotation = _eyeOrientation;
            FocusEntity.BodyRotation = _bodyOrientation;
        }

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
                Quaternion.Multiply(ref rotation, ref _eyeOrientation, out _eyeOrientation);
                Quaternion.Multiply(ref rotation, ref _bodyOrientation, out _bodyOrientation);
            }

            // Rotate camera about its local x axis.
            // Note the order the quaternions are multiplied. That is important!
            if (pitch != 0.0f)
            {
                Quaternion.RotationAxis(ref MVector3.Right, pitch, out rotation);
                Quaternion.Multiply(ref _eyeOrientation, ref rotation, out _eyeOrientation);
            }

            return true;
        }
    }
}
