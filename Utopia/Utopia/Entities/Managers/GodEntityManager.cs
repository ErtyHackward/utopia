using System.Linq;
using Ninject;
using S33M3CoreComponents.Cameras;
using S33M3CoreComponents.Cameras.Interfaces;
using S33M3CoreComponents.Inputs;
using S33M3CoreComponents.Inputs.Actions;
using S33M3CoreComponents.Maths;
using S33M3DXEngine;
using S33M3DXEngine.Main;
using S33M3Resources.Structs;
using SharpDX;
using System;
using Utopia.Action;
using Utopia.Entities.Managers.Interfaces;
using Utopia.Shared.Chunks;
using Utopia.Shared.Entities;
using Utopia.Shared.Entities.Dynamic;
using Utopia.Shared.Entities.Interfaces;
using Utopia.Shared.Structs;
using Utopia.Shared.Structs.Helpers;
using Utopia.Shared.World;
using Utopia.Worlds.Chunks;
using ButtonState = S33M3CoreComponents.Inputs.MouseHandler.ButtonState;

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
        private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        private readonly D3DEngine _engine;
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

        private bool _selectionNow;
        private Vector3I _selectionStart;

        private Faction _faction;

        /// <summary>
        /// Gets or sets current focus point. In level mode this entity could move only in horisontal plane.
        /// If level mode is disabled the entity will move over the top surface of the chunk.
        /// </summary>
        public GodEntity GodEntity { get; set; }

        /// <summary>
        /// Gets main player entity (character or PlayerFocusEntity)
        /// </summary>
        public ICharacterEntity Player { get { return GodEntity; } }

        /// <summary>
        /// If camera is inside water
        /// </summary>
        public bool IsHeadInsideWater { get { return false; } }

        /// <summary>
        /// Gets active player tool or null
        /// </summary>
        public IItem ActiveTool { get { return GodEntity.GodHand; } }

        /// <summary>
        /// Gets player faction
        /// </summary>
        public Faction Faction { get { return _faction; } }
        
        #region ICameraPlugin

        // this region contain camera specific properties
        // these properties controlls the ThirdPerson camera position and rotation

        public Vector3D CameraWorldPosition { get { return GodEntity.Position; } }
        public Quaternion CameraOrientation { get { return GodEntity.HeadRotation; } }
        public Quaternion CameraYAxisOrientation { get { return GodEntity.BodyRotation; } }
        public int CameraUpdateOrder { get; private set; }
        #endregion

        /// <summary>
        /// Gets range of block that player currently picking
        /// </summary>
        public Range3I? HoverRange
        {
            get
            {
                if (!_selectionNow || !GodEntity.EntityState.IsBlockPicked)
                    return null;
                return Range3I.FromTwoVectors(_selectionStart, GodEntity.EntityState.PickedBlockPosition);
            }
        }

        /// <summary>
        /// Gets value indicating if player selects new blocks (otherwise means deselect operation)
        /// </summary>
        public bool Selection
        {
            get
            {
                if (!_selectionNow)
                    return false;

                return !Faction.Designations.OfType<DigDesignation>().Any(d => d.BlockPosition == _selectionStart);
            }
        }
        
        [Inject]
        public IWorldChunks2D Chunks { get; set; }

        public GodEntityManager(D3DEngine engine,
                                GodEntity playerEntity, 
                                InputsManager inputsManager, 
                                SingleArrayChunkContainer cubesHolder,
                                CameraManager<ICameraFocused> cameraManager,
                                LandscapeBufferManager bufferManager,
                                VisualWorldParameters visParameters,
                                GlobalStateManager globalStateManager)
        {
            if (engine == null) throw new ArgumentNullException("engine");
            if (playerEntity  == null) throw new ArgumentNullException("playerEntity");
            if (inputsManager == null) throw new ArgumentNullException("inputsManager");
            if (cubesHolder   == null) throw new ArgumentNullException("cubesHolder");
            if (cameraManager == null) throw new ArgumentNullException("cameraManager");
            if (bufferManager == null) throw new ArgumentNullException("bufferManager");
            if (visParameters == null) throw new ArgumentNullException("visParameters");

            GodEntity = playerEntity;

            _faction = globalStateManager.GlobalState.Factions[GodEntity.FactionId];

            _eyeOrientation  = GodEntity.HeadRotation;
            _bodyOrientation = GodEntity.BodyRotation;

            _engine = engine;
            _inputsManager = inputsManager;
            _cubesHolder   = cubesHolder;
            _cameraManager = cameraManager;
            _bufferManager = bufferManager;
            _visParameters = visParameters;
        }

        public override void FTSUpdate(GameTime timeSpent)
        {
            // wait untill all chunks are loaded
            if (!Chunks.IsInitialLoadCompleted)
                return;

            #region handle movement of the focus point

            InputHandling();

            _rotationDelta = 6f * timeSpent.ElapsedGameTimeInS_LD;

            EntityRotation(timeSpent.ElapsedGameTimeInS_LD);

            var camera = _cameraManager.ActiveCamera as ThirdPersonCameraWithFocus;

            var speed = timeSpent.ElapsedGameTimeInS_LD * 500 * camera.Distance / camera.MaxDistance;

            UpdateCameraPosition();

            // apply movement
            GodEntity.FinalPosition += _moveVector * speed;

            GodEntity.Position = Vector3D.SmoothStep(GodEntity.Position, GodEntity.FinalPosition, timeSpent.ElapsedGameTimeInS_LD * 10);
            
            #endregion

            _bufferManager.CleanUpClient(BlockHelper.EntityToChunkPosition(GodEntity.Position), _visParameters);
            
            base.FTSUpdate(timeSpent);
        }

        private void UpdateCameraPosition()
        {
            if (Chunks.SliceValue != -1)
            {
                if (GodEntity.Position.Y != Chunks.SliceValue)
                    GodEntity.Position = new Vector3D(GodEntity.Position.X, Chunks.SliceValue, GodEntity.Position.Z);
                return;
            }

            // in non-level mode we need to validate focus entity position depending on 
            // the look vector of the camera untill it will cross with the surface

            var camera = _cameraManager.ActiveCamera as ThirdPersonCameraWithFocus;

            if (camera == null)
                return;

            var pos = camera.CameraPosition;

            var inverted = GodEntity.HeadRotation;
            inverted.Invert();
            
            var checkVector = Vector3.Transform(Vector3.UnitZ, inverted);
            checkVector.Normalize();

            var posCalc = GodEntity.FinalPosition - checkVector * camera.Distance;


            if (checkVector.Y >= 0) 
                return;

            // check from the camera pos to the focused entity (fly back)

            var entityPos = GodEntity.FinalPosition.AsVector3();
            
            for (var d = 0f; d < camera.Distance; d += 0.1f)
            {
                var cube = _cubesHolder.GetCube((Vector3I)(pos + checkVector * d));
                if (cube.Cube.Id != 0)
                {
                    GodEntity.FinalPosition = new Vector3D(pos + checkVector * (d - 0.1f));
                    return;
                }
            }

            // falling down until we find the surface

            var distance = 0f;
            while (true)
            {
                distance += 0.1f;

                var cubePos = entityPos + checkVector * distance;

                if (cubePos.Y <= 0)
                {
                    cubePos.Y = 0;
                }

                var cube = _cubesHolder.GetCube((Vector3I)cubePos);

                if (cube.Cube.Id != 0)
                {
                    break;
                }

                entityPos = entityPos + checkVector * (distance);

                if (cubePos.Y == 0)
                {
                    break;
                }
            }

            GodEntity.FinalPosition = new Vector3D(entityPos);
        }


        private void InputHandling()
        {
            var moveVector = Vector3.Zero;

            if (_inputsManager.MouseManager.StrategyMode)
            {
                var ms = _inputsManager.MouseManager.CurMouseState;
                
                if (ms.X == 0)
                    moveVector.X = -1;

                if (ms.X == _engine.ViewPort.Width - 1)
                    moveVector.X = 1;

                if (ms.Y == 0)
                    moveVector.Z = 1;

                if (ms.Y == _engine.ViewPort.Height - 1)
                    moveVector.Z = -1;
            }


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

            Quaternion inv = GodEntity.HeadRotation;

            inv.Invert();

            inv.X = 0;
            inv.Z = 0;

            inv.Normalize();

            _moveVector = Vector3.Transform(moveVector, inv);

            var godHandToolState = GodEntity.EntityState.ToolState as GodHandToolState;

            if (godHandToolState == null)
            {
                godHandToolState = new GodHandToolState();
                GodEntity.EntityState.ToolState = godHandToolState;
            }
            
            godHandToolState.SliceValue = Chunks.SliceValue;
            godHandToolState.DesignationBlueprintId = GodEntity.DesignationBlueprintId;
            
            if (_inputsManager.ActionsManager.isTriggered(UtopiaActions.UseLeft))
            {
                GodEntity.EntityState.MouseUp = _inputsManager.MouseManager.CurMouseState.LeftButton == ButtonState.Released;
                GodEntity.EntityState.MouseButton = MouseButton.LeftButton;

                if (!GodEntity.EntityState.MouseUp)
                {
                    if (GodEntity.EntityState.IsBlockPicked)
                    {
                        _selectionStart = GodEntity.EntityState.PickedBlockPosition;
                        _selectionNow = true;
                    }
                }
                else
                    _selectionNow = false;

                GodEntity.ToolUse();
                
            }

            if (_inputsManager.ActionsManager.isTriggered(UtopiaActions.UseRight))
            {
                GodEntity.EntityState.MouseUp = _inputsManager.MouseManager.CurMouseState.RightButton == ButtonState.Released;
                GodEntity.EntityState.MouseButton = MouseButton.RightButton;

                GodEntity.ToolUse();
            }
        }

        private void EntityRotation(float elapsedTime)
        {
            float headingDegrees = 0.0f;
            float pitchDegree = 0.0f;
            float rollDegree = 0.0f;
            bool hasRotated = false;

            if (_inputsManager.MouseManager.CurMouseState.MiddleButton == ButtonState.Pressed)
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

            GodEntity.HeadRotation = _eyeOrientation;
            GodEntity.BodyRotation = _bodyOrientation;
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
                Quaternion.Multiply(ref _eyeOrientation, ref rotation, out _eyeOrientation);
                Quaternion.Multiply(ref _bodyOrientation, ref rotation, out _bodyOrientation);
            }

            // Rotate camera about its local x axis.
            // Note the order the quaternions are multiplied. That is important!
            if (pitch != 0.0f)
            {
                Quaternion.RotationAxis(ref MVector3.Right, pitch, out rotation);
                Quaternion.Multiply(ref rotation, ref _eyeOrientation, out _eyeOrientation);
            }

            return true;
        }


        public EntityMovement.EntityRotations EntityRotations
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }


        public event EventHandler<PlayerEntityChangedEventArgs> PlayerEntityChanged;

        public event PlayerEntityManager.LandingGround OnLanding;


        public Particules.UtopiaParticuleEngine UtopiaParticuleEngine
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }
    }
}
