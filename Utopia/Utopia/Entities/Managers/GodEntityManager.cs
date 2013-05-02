using System;
using S33M3CoreComponents.Cameras;
using S33M3CoreComponents.Cameras.Interfaces;
using S33M3CoreComponents.Inputs;
using S33M3CoreComponents.Inputs.Actions;
using S33M3DXEngine.Main;
using S33M3Resources.Structs;
using SharpDX;
using Utopia.Shared.Chunks;
using Utopia.Shared.Entities.Dynamic;

namespace Utopia.Entities.Managers
{
    /// <summary>
    /// Responsible for player input handling in god mode.
    /// Supports different levels and allows to switch between them.
    /// Handles entity picking from the camera position.
    /// Should be used only with 3rd person camera.
    /// </summary>
    public class GodEntityManager : GameComponent, ICameraPlugin
    {
        private readonly InputsManager _inputsManager;
        private readonly SingleArrayChunkContainer _cubesHolder;
        private readonly CameraManager<ICameraFocused> _cameraManager;

        private Vector3 _moveVector;
        
        /// <summary>
        /// Gets or sets current focus point. In level mode this entity could move only in horisontal plane.
        /// If level mode is disabled the entity will move over the top surface of the chunk.
        /// </summary>
        public PlayerFocusEntity FocusEntity { get; set; }

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
                                CameraManager<ICameraFocused> cameraManager)
        {
            if (playerEntity == null) throw new ArgumentNullException("playerEntity");
            if (inputsManager == null) throw new ArgumentNullException("inputsManager");
            if (cubesHolder == null) throw new ArgumentNullException("cubesHolder");
            if (cameraManager == null) throw new ArgumentNullException("cameraManager");

            FocusEntity = playerEntity;
            _inputsManager = inputsManager;
            _cubesHolder = cubesHolder;
            _cameraManager = cameraManager;
        }

        public override void VTSUpdate(double interpolationHd, float interpolationLd, float elapsedTime)
        {
            #region handle movement of the focus point

            InputHandling();

            // apply movement
            FocusEntity.Position += _moveVector * elapsedTime;

            // validate new position if not in level mode
            if (!LevelMode)
            {
                // slide by camera lookat vector
                var lookVector = Vector3.Transform(Vector3.UnitZ, FocusEntity.HeadRotation);

                //_cubesHolder.GetCube(FocusEntity.Position, false)

                // TODO: check entity for surface collision, take into account camera position and view angle
            }

            #endregion

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

            _moveVector = Vector3.Transform(moveVector, FocusEntity.HeadRotation);
        }



    }
}
