using Ninject;
using S33M3CoreComponents.Cameras;
using S33M3CoreComponents.Cameras.Interfaces;
using S33M3CoreComponents.Inputs;
using S33M3CoreComponents.Inputs.Actions;
using S33M3CoreComponents.Maths;
using S33M3DXEngine.Main;
using S33M3Resources.Structs;
using SharpDX;
using System;
using Utopia.Entities.Managers.Interfaces;
using Utopia.Shared.Chunks;
using Utopia.Shared.Entities.Concrete;
using Utopia.Shared.Entities.Dynamic;
using Utopia.Shared.Entities.Interfaces;
using Utopia.Shared.Structs.Helpers;
using Utopia.Shared.World;
using Utopia.Worlds.Chunks;

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

        private GodHandTool _handTool = new GodHandTool();

        /// <summary>
        /// Gets or sets current focus point. In level mode this entity could move only in horisontal plane.
        /// If level mode is disabled the entity will move over the top surface of the chunk.
        /// </summary>
        public PlayerFocusEntity FocusEntity { get; set; }

        /// <summary>
        /// Gets main player entity (character or PlayerFocusEntity)
        /// </summary>
        public IDynamicEntity Player { get { return FocusEntity; } }

        /// <summary>
        /// If camera is inside water
        /// </summary>
        public bool IsHeadInsideWater { get { return false; } }

        /// <summary>
        /// Gets active player tool or null
        /// </summary>
        public IItem ActiveTool { get { return _handTool; } }

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

        [Inject]
        public IWorldChunks Chunks { get; set; }

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
            // wait untill all chunks are loaded
            if (!Chunks.IsInitialLoadCompleted)
                return;

            #region handle movement of the focus point

            InputHandling();

            _rotationDelta = 2f * elapsedTime;

            EntityRotation(elapsedTime);

            var speed = elapsedTime * 500;

            // apply movement
            FocusEntity.FinalPosition += _moveVector * speed;

            UpdateCameraPosition();
            
            FocusEntity.Position = Vector3D.SmoothStep(FocusEntity.Position, FocusEntity.FinalPosition, elapsedTime * 10);

            #endregion

            _bufferManager.CleanUpClient(BlockHelper.EntityToChunkPosition(FocusEntity.Position), _visParameters);

            base.VTSUpdate(interpolationHd, interpolationLd, elapsedTime);
        }

        private void UpdateCameraPosition()
        {
            if (LevelMode) 
                return;

            // in non-level mode we need to validate focus entity position depending on 
            // the look vector of the camera untill it will cross with the surface

            var camera = _cameraManager.ActiveCamera as ThirdPersonCameraWithFocus;

            if (camera == null) 
                return;

            var pos = camera.CameraPosition;

            // camera look vector
            var checkVector = FocusEntity.FinalPosition.AsVector3() - pos;
            checkVector.Normalize();

            if (checkVector.Y >= 0 ) 
                return;

            // check from the camera pos to the focused entity

            for (float d = 0; d < camera.Distance; d += 0.1f)
            {
                var cube = _cubesHolder.GetCube((Vector3I)(pos + checkVector * d), false);
                if (cube.Cube.Id != 0)
                {
                    FocusEntity.FinalPosition = new Vector3D(pos + checkVector * (d - 0.1f));
                    break;
                }
            }

            var distance = 0f;

            while (true)
            {
                distance += 0.1f;
                var cubePos = FocusEntity.FinalPosition + checkVector * distance;

                if (cubePos.Y <= 0)
                {
                    cubePos.Y = 0;
                }

                var cube = _cubesHolder.GetCube(cubePos);

                if (cube.Cube.Id != 0)
                {
                    break;
                }

                FocusEntity.FinalPosition = cubePos;

                if (cubePos.Y == 0)
                {
                    break;
                }
            }
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
