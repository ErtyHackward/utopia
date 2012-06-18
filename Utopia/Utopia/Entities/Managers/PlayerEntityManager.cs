﻿using System;
using System.Drawing;
using System.Windows.Forms;
using SharpDX;
using Utopia.Entities.Managers.Interfaces;
using Utopia.Entities.Renderer;
using Utopia.Entities.Voxel;
using Utopia.Network;
using Utopia.Shared.Chunks;
using Utopia.Shared.Cubes;
using Utopia.Shared.Entities;
using Utopia.Shared.Entities.Dynamic;
using Utopia.Shared.Entities.Interfaces;
using Utopia.Shared.Interfaces;
using Utopia.Shared.Structs;
using Utopia.Shared.Structs.Landscape;
using Utopia.Entities.Renderer.Interfaces;
using Ninject;
using Utopia.Worlds.Chunks;
using Utopia.Worlds.Cubes;
using Utopia.Shared.Settings;
using S33M3Resources.Structs;
using S33M3DXEngine.Main;
using S33M3CoreComponents.Cameras.Interfaces;
using S33M3DXEngine;
using S33M3CoreComponents.Cameras;
using S33M3CoreComponents.Inputs.Actions;
using S33M3CoreComponents.WorldFocus;
using S33M3CoreComponents.Inputs;
using S33M3CoreComponents.Physics.Verlet;
using S33M3CoreComponents.Maths;
using SharpDX.Direct3D11;
using Utopia.Action;
using S33M3CoreComponents.Physics;
using S33M3DXEngine.Debug.Interfaces;
using Utopia.Entities.EntityMovement;

namespace Utopia.Entities.Managers
{
    public class PlayerEntityManager : DrawableGameComponent, ICameraPlugin, IVisualEntityContainer, IDebugInfo
    {
        private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        #region Private variables
        //Engine System variables
        private D3DEngine _d3DEngine;
        private CameraManager<ICameraFocused> _cameraManager;
        private WorldFocusManager _worldFocusManager;
        private InputsManager _inputsManager;
        private SingleArrayChunkContainer _cubesHolder;

        //Block Picking variables
        public TerraCubeWithPosition PickedCube;
        public TerraCubeWithPosition NewCube;

        //Head UnderWater test
        private int _headCubeIndex;
        private TerraCube _headCube;

        //Player Visual characteristics (Not insde the PlayerCharacter object)
        private VisualEntity _pickedUpEntity;
        private Vector3D _pickedUpEntityPosition;
        
        private FTSValue<Vector3D> _worldPosition = new FTSValue<Vector3D>();         //World Position
        private FTSValue<Quaternion> _lookAtDirection = new FTSValue<Quaternion>();   //LookAt angle
        private FTSValue<Quaternion> _cameraYAxisOrientation = new FTSValue<Quaternion>();   //LookAtYAxis angle
        private FTSValue<Quaternion> _moveDirection = new FTSValue<Quaternion>();     //Real move direction (derived from LookAt, but will depend the mode !)
        private Vector3D _lookAt;
        private Vector3 _entityEyeOffset;                                     //Offset of the camera Placement inside the entity, from entity center point.

        //Mouvement handling variables
        private VerletSimulator _physicSimu;
        private double _accumPitchDegrees;
        private double _gravityInfluence;
        private double _groundBelowEntity;
        private double _rotationDelta;
        private double _moveDelta;
        private Matrix _headRotation;
        private Matrix _entityRotation;
        private Vector3D _entityHeadXAxis, _entityHeadYAxis, _entityHeadZAxis;
        private Vector3D _entityXAxis, _entityYAxis, _entityZAxis;
        private IPickingRenderer _pickingRenderer;
        private IEntityPickingManager _entityPickingManager;
        private readonly IGameStateToolManager _gameStateToolManager;
        private bool _stopMovedAction = false;

        //Drawing component
        private IEntitiesRenderer _playerRenderer;

        //Event related variables
        private double _fallMaxHeight;

        private TerraCubeWithPosition _groundCube;
        private CubeProfile _groundCubeProgile;


        private EntityMovements _movement;
        #endregion

        #region Public variables/properties

        public Vector3D LookAt
        {
            get { return _lookAt; }
            set { _lookAt = value; }
        }

        /// <summary>
        /// The Player
        /// </summary>
        public readonly PlayerCharacter Player;

        /// <summary>
        /// The Player Voxel body
        /// </summary>
        public VisualVoxelEntity VisualEntity { get; set; }

        //Implement the interface Needed when a Camera is "plugged" inside this entity
        public virtual Vector3D CameraWorldPosition { get { return _worldPosition.ValueInterp + _entityEyeOffset; } }
        public virtual Quaternion CameraOrientation { get { return _lookAtDirection.ValueInterp; } }
        public virtual Quaternion CameraYAxisOrientation { get { return _cameraYAxisOrientation.ValueInterp; } }
        public virtual int CameraUpdateOrder { get { return this.UpdateOrder; } }

        public bool IsHeadInsideWater { get; set; }

        public bool CatchExclusiveAction { get; set; }

        public EntityDisplacementModes DisplacementMode
        {
            get { return Player.DisplacementMode; }
            set
            {
                Player.DisplacementMode = value;
#if DEBUG
                logger.Info("{0} is now {1}", Player.CharacterName, value.ToString());
#endif
                if (value == EntityDisplacementModes.Walking || value == EntityDisplacementModes.Swiming)
                {
                    _physicSimu.StartSimulation(ref _worldPosition.Value, ref _worldPosition.Value);
                }
                else
                {
                    _physicSimu.StopSimulation();
                }
            }
        }

        public bool HasMouseFocus { get; set; }

        public float PlayerOnOffsettedBlock { get; set; }
        public double GroundBelowEntity
        {
            get { return _groundBelowEntity; }
            set { _groundBelowEntity = value; }
        }

        public IWorldChunks WorldChunks { get; set; }
        public bool MousepickDisabled { get; set; }

        private bool _landscapeInitiazed;
        public bool LandscapeInitiazed
        {
            get { return _landscapeInitiazed; }
            set { _landscapeInitiazed = value; }
        }

        public delegate void LandingGround(double fallHeight, TerraCubeWithPosition landedCube);
        public event LandingGround OnLanding;

        public float OffsetBlockHitted { get; set; }
        #endregion

        public PlayerEntityManager(D3DEngine engine,
                                   CameraManager<ICameraFocused> cameraManager,
                                   WorldFocusManager worldFocusManager,
                                   InputsManager inputsManager,
                                   SingleArrayChunkContainer cubesHolder,
                                   PlayerCharacter player,
                                   [Named("PlayerEntityRenderer")] IEntitiesRenderer playerRenderer,
                                   IPickingRenderer pickingRenderer,
                                   IEntityPickingManager entityPickingManager,
                                   VoxelModelManager voxelModelManager
            )
        {
            _d3DEngine = engine;
            _cameraManager = cameraManager;
            _worldFocusManager = worldFocusManager;
            _inputsManager = inputsManager;
            _cubesHolder = cubesHolder;
            _playerRenderer = playerRenderer;
            _pickingRenderer = pickingRenderer;
            _entityPickingManager = entityPickingManager;

            entityPickingManager.Player = this;
            Player = player;

            this.ShowDebugInfo = true;

            VisualEntity = new VisualVoxelEntity(player, voxelModelManager);

            //Give the Renderer acces to the Voxel buffers, ...
            _playerRenderer.VisualEntity = this;

            HasMouseFocus = Updatable;
            UpdateOrder = 0;

            _movement = new EntityMovements(_inputsManager, EntityMovementModes.Walking);
            _movement.EntityMoveSpeed = player.MoveSpeed;
            _movement.EntityRotationSpeed = player.RotationSpeed;
        }

        public override void BeforeDispose()
        {
            //Clean Up event Delegates
            if (OnLanding != null)
            {
                //Remove all Events associated to this Event (That haven't been unsubscribed !)
                foreach (Delegate d in OnLanding.GetInvocationList())
                {
                    OnLanding -= (LandingGround)d;
                }
            }

        }

        #region Private Methods

        #region Player InputHandling
        /// <summary>
        /// Handle Player Actions - Movement and rotation input are not handled here
        /// </summary>
        private void inputHandler()
        {
            if (_inputsManager.ActionsManager.isTriggered(UtopiaActions.Move_Mode, CatchExclusiveAction))
            {
                if (Player.DisplacementMode == EntityDisplacementModes.Flying)
                {
                    DisplacementMode = EntityDisplacementModes.Walking;
                }
                else
                {
                    DisplacementMode = EntityDisplacementModes.Flying;
                }
            }
            
            if (!HasMouseFocus) return; //the editor(s) can acquire the mouseFocus

            if (_inputsManager.ActionsManager.isTriggered(UtopiaActions.Use_Left, CatchExclusiveAction))
            {
                if ((Player.EntityState.IsBlockPicked || Player.EntityState.IsEntityPicked) && Player.Equipment.LeftTool!=null)
                {
                    //sends the client server event that does tool.use on server
                    Player.LeftToolUse(ToolUseMode.LeftMouse);

                    //client invocation to keep the client inventory in synch
                    Player.Equipment.LeftTool.Use(Player, ToolUseMode.LeftMouse);
                }
            }

            if (_inputsManager.ActionsManager.isTriggered(UtopiaActions.Use_Right, CatchExclusiveAction))
            {
                if ((Player.EntityState.IsBlockPicked || Player.EntityState.IsEntityPicked) && Player.Equipment.LeftTool != null)
                {
                    //Avoid the player to add a block where he is located !            
                    BoundingBox playerPotentialNewBlock;
                    ComputeBlockBoundingBox(ref Player._entityState.NewBlockPosition, out playerPotentialNewBlock);

                    if (!MBoundingBox.Intersects(ref VisualEntity.WorldBBox, ref playerPotentialNewBlock))
                    {
                        //sends the client server event that does tool.use on server
                        Player.LeftToolUse(ToolUseMode.RightMouse);

                        //client invocation to keep the client inventory in synch
                        Player.Equipment.LeftTool.Use(Player, ToolUseMode.RightMouse);
                    }
                }
            }

            if (_inputsManager.ActionsManager.isTriggered(UtopiaActions.EntityUse, CatchExclusiveAction))
            {
                //TODO implement use 'picked' entity (picked here means entity is in world having cursor over it, not in your hand or pocket) 
                //like opening a chest or a door  
            }

            if (_inputsManager.ActionsManager.isTriggered(UtopiaActions.EntityThrow, CatchExclusiveAction))
            {
                //TODO unequip left item and throw it on the ground, (version 0 = place it at newCubeplace, animation later)                
                // and next, throw the right tool if left tool is already thrown
            }
            
        }
        #endregion

        #region Player Picking
        private void GetSelectedEntity()
        {
            bool newpicking;

            if (MousepickDisabled || _inputsManager.MouseManager.MouseCapture)
            {
                Vector3D pickingPointInLine = _worldPosition.Value + _entityEyeOffset;
                newpicking = RefreshPicking(ref pickingPointInLine, ref _lookAt, 1);
            }
            else
            {
                Vector3D mouseWorldPosition;
                Vector3D mouseLookAtPosition;
                _inputsManager.MouseManager.UnprojectMouseCursor(_cameraManager.ActiveCamera, out mouseWorldPosition, out mouseLookAtPosition);
                newpicking = RefreshPicking(ref mouseWorldPosition, ref mouseLookAtPosition, 2);
            }

            if(newpicking)
            {
                //A new Block has been pickedup
                if (Player._entityState.IsEntityPicked == false)
                {
                    _pickingRenderer.SetPickedBlock(ref Player._entityState.PickedBlockPosition, GameSystemSettings.Current.Settings.CubesProfile[PickedCube.Cube.Id].YBlockOffset);
                }
                else
                {
                    _pickingRenderer.SetPickedEntity(_pickedUpEntity);
                }
            }
        }


        //Will return true if a new Item has been picked up !
        private bool RefreshPicking(ref Vector3D pickingWorldPosition, ref Vector3D pickingLookAt, int rounding)
        {
            Player._entityState.IsBlockPicked = false;

            //Check the Ray against all entity.
            Ray pickingRay = new Ray(pickingWorldPosition.AsVector3(), pickingLookAt.AsVector3());
            if (_entityPickingManager.CheckEntityPicking(ref pickingRay, out _pickedUpEntity))
            {
                _pickedUpEntityPosition = _pickedUpEntity.Entity.Position;
                Player._entityState.PickedEntityPosition = _pickedUpEntity.Entity.Position;
                Player._entityState.PickedEntityLink = _pickedUpEntity.Entity.GetLink();
                Player._entityState.IsEntityPicked = true;
                Player._entityState.IsBlockPicked = false;
                return true;
            }

            //Sample 500 points in the view direction vector
            for (int ptNbr = 0; ptNbr < 500; ptNbr++)
            {
                pickingWorldPosition += pickingLookAt * 0.02;

                //Check if a block is picked up !
                if (_cubesHolder.isPickable(ref pickingWorldPosition, out PickedCube))
                {
                    Player._entityState.PickedBlockPosition = PickedCube.Position;
                    
                    bool newPlacechanged = false;
                    
                    //Find the potential new block place, by rolling back !
                    while (ptNbr > 0)
                    {
                        pickingWorldPosition -= pickingLookAt * 0.02;
                        
                        if (_cubesHolder.isPickable(ref pickingWorldPosition, out NewCube) == false)
                        {
                            Player._entityState.NewBlockPosition = NewCube.Position;
                            newPlacechanged = true;
                            break;
                        }
                        ptNbr--;
                    }

                    Player._entityState.IsEntityPicked = false;
                    Player._entityState.IsBlockPicked = true;
                    if (PickedCube.Position == Player._entityState.PickedBlockPosition)
                    {
                        if (! newPlacechanged) return false;
                    }
                    break;
                }
            }

            return Player._entityState.IsBlockPicked; //Return true if a new block or Entity has been picked up !
        }

        private void ComputeBlockBoundingBox(ref Vector3I blockPlace, out BoundingBox blockBoundingBox)
        {
            blockBoundingBox = new BoundingBox(new Vector3(blockPlace.X, blockPlace.Y, blockPlace.Z), new Vector3(blockPlace.X + 1, blockPlace.Y + 1, blockPlace.Z + 1));
        }
        #endregion

        #region Actions after new position defined
        /// <summary>
        /// Serie of check that needs to be done when the new position is defined
        /// </summary>
        private void CheckAfterNewPosition()
        {
            CheckForEventRaising();
        }

        private void CheckHeadUnderWater()
        {
            if (_cubesHolder.IndexSafe(MathHelper.Fastfloor(CameraWorldPosition.X), MathHelper.Fastfloor(CameraWorldPosition.Y), MathHelper.Fastfloor(CameraWorldPosition.Z), out _headCubeIndex))
            {
                //Get the cube at the camera position !
                _headCube = _cubesHolder.Cubes[_headCubeIndex];

                //Get Feet block
                int feetBlockIdx = _cubesHolder.FastIndex(_headCubeIndex, MathHelper.Fastfloor(CameraWorldPosition.Y), SingleArrayChunkContainer.IdxRelativeMove.Y_Minus1);
                TerraCube feetBlock = _cubesHolder.Cubes[feetBlockIdx];
                TerraCube BelowfeetBlock = _cubesHolder.Cubes[_cubesHolder.FastIndex(feetBlockIdx, MathHelper.Fastfloor(CameraWorldPosition.Y) - 1, SingleArrayChunkContainer.IdxRelativeMove.Y_Minus1)];

                if (GameSystemSettings.Current.Settings.CubesProfile[feetBlock.Id].CubeFamilly == Shared.Enums.enuCubeFamilly.Liquid &&
                   (GameSystemSettings.Current.Settings.CubesProfile[BelowfeetBlock.Id].CubeFamilly == Shared.Enums.enuCubeFamilly.Liquid || GameSystemSettings.Current.Settings.CubesProfile[_headCube.Id].CubeFamilly == Shared.Enums.enuCubeFamilly.Liquid))
                {
                    if (DisplacementMode == EntityDisplacementModes.Walking)
                    {
                        DisplacementMode = EntityDisplacementModes.Swiming;
                    }
                }
                else
                {
                    if (DisplacementMode == EntityDisplacementModes.Swiming) DisplacementMode = EntityDisplacementModes.Walking;
                }

                //Eyes under water (Used to change view Color)
                if (_headCube.Id == CubeId.StillWater || _headCube.Id == CubeId.DynamicWater)
                {
                    int AboveHead = _cubesHolder.FastIndex(_headCubeIndex, MathHelper.Fastfloor(CameraWorldPosition.Y), SingleArrayChunkContainer.IdxRelativeMove.Y_Plus1);
                    if (_cubesHolder.Cubes[AboveHead].Id == CubeId.Air)
                    {
                        //Check the offset of the water
                        var Offset = CameraWorldPosition.Y - MathHelper.Fastfloor(CameraWorldPosition.Y);
                        if (Offset >= 1 - GameSystemSettings.Current.Settings.CubesProfile[_headCube.Id].YBlockOffset)
                        {
                            IsHeadInsideWater = false;
                            return;
                        }
                    }

                    IsHeadInsideWater = true;
                }
                else
                {
                    IsHeadInsideWater = false;
                }
            }
        }

        private void CheckForEventRaising()
        {
            //Landing on ground after falling event
            if (_physicSimu.OnGround == false)
            {
                //New "trigger"
                if (_worldPosition.Value.Y > _fallMaxHeight) _fallMaxHeight = _worldPosition.Value.Y;
            }
            else
            {
                if (_physicSimu.OnGround == true && _fallMaxHeight != int.MinValue)
                {
                    if (OnLanding != null) 
                    {
                        OnLanding(_fallMaxHeight - _worldPosition.Value.Y, _groundCube);
                    }
#if DEBUG
                    logger.Debug("OnLandingGround event fired with height value : {0} m, cube type : {1} ", _fallMaxHeight - _worldPosition.Value.Y, CubeId.GetCubeTypeName(_groundCube.Cube.Id));
#endif           
                        _fallMaxHeight = int.MinValue;
                }
            }


        }
        #endregion

        #region Physic simulation for Collision detection
        private void PhysicOnEntity(EntityDisplacementModes mode, ref GameTime timeSpent)
        {
            if (_stopMovedAction && _groundCubeProgile.SlidingValue == 0)
            {
                _stopMovedAction = false;
            }

            switch (mode)
            {
                case EntityDisplacementModes.Flying:
                    _physicSimu.Friction = 0f;
                    break;
                case EntityDisplacementModes.Swiming:
                    _physicSimu.Friction = 0.3f;
                    PhysicSimulation(ref timeSpent);
                    break;
                case EntityDisplacementModes.Walking:
                    if (_physicSimu.OnGround)
                    {
                        if (_stopMovedAction == false)
                        {
                            _physicSimu.Friction = _groundCubeProgile.Friction; //0.25f;
                        }
                        else
                        {
                            //I did stop to move, but I'm on a sliding block => Will slide a little before ending movement
                            _physicSimu.Friction = _groundCubeProgile.SlidingValue;
                        }
                    }
                    else
                    {
                        _physicSimu.Friction = 0f;
                    }
                    PhysicSimulation(ref timeSpent);
                    break;
                default:
                    break;
            }
        }

        /// <summary>
        /// Check if the player is on the ground, or not.
        /// </summary>
        /// <param name="timeSpent"></param>
        private void PhysicSimulation(ref GameTime timeSpent)
        {
            Vector3I GroundDirection = new Vector3I(0, -1, 0);
            Vector3D newWorldPosition;
            double BlockOffset;

            _cubesHolder.GetNextSolidBlockToPlayer(ref VisualEntity.WorldBBox, ref GroundDirection, out _groundCube);
            //Half cube below me ??
            _groundCubeProgile = GameSystemSettings.Current.Settings.CubesProfile[_groundCube.Cube.Id];
            BlockOffset = _groundCubeProgile.YBlockOffset;
            _groundBelowEntity = _groundCube.Position.Y + (1 - BlockOffset);
            PlayerOnOffsettedBlock = (float)BlockOffset;//BlockOffset != 0;

            _physicSimu.Simulate(ref timeSpent, out newWorldPosition);
            _worldPosition.Value = newWorldPosition;

            //After physic simulation, I'm still in the air ?
            if (_worldPosition.Value.Y > _groundBelowEntity)
            {
                _physicSimu.OnGround = false;
            }
        }
        #endregion

        #region Entity Movement + rotation

        private void RefreshEntityMovementAndRotation(ref GameTime timeSpent)
        {
            switch (Player.DisplacementMode)
            {
                case EntityDisplacementModes.Flying:
                    _gravityInfluence = 3;  // We will move 6 times faster if flying
                    break;
                case EntityDisplacementModes.Walking:
                    _gravityInfluence = 1;
                    break;
                case EntityDisplacementModes.Swiming:
                    _gravityInfluence = 1f / 2; // We will move 2 times slower when swimming
                    break;
                default:
                    break;
            }

            //Compute the delta following the time elapsed : Speed * Time = Distance (Over the elapsed time).
            _moveDelta = Player.MoveSpeed * _gravityInfluence * timeSpent.ElapsedGameTimeInS_HD;
            _rotationDelta = Player.RotationSpeed * timeSpent.ElapsedGameTimeInS_HD;

            //Backup previous values
            _lookAtDirection.BackUpValue();
            _worldPosition.BackUpValue();
            _cameraYAxisOrientation.BackUpValue();

            //Rotation with mouse
            //EntityRotationsOnEvents(Player.DisplacementMode);
            _movement.Update(timeSpent);
            _lookAtDirection.Value = _movement.EyeOrientation;
            _worldPosition.Value = _movement.WorldPosition;
            //Movement
            //EntityMovementsOnEvents(Player.DisplacementMode, ref timeSpent);

            //Physic simulation !
            PhysicOnEntity(Player.DisplacementMode, ref timeSpent);

            //Send the Actual Position to the Entity object only of it has change !!!
            //The Change check is done at DynamicEntity level
            Player.Position = _worldPosition.Value;
            Player.HeadRotation = _lookAtDirection.Value;
        }

        #region Movement Management
        private void EntityMovementsOnEvents(EntityDisplacementModes mode, ref GameTime timeSpent)
        {
            switch (mode)
            {
                case EntityDisplacementModes.Swiming:
                    SwimmingFreeFirstPersonMove(ref timeSpent);
                    break;
                case EntityDisplacementModes.Flying:
                    FreeFirstPersonMove();
                    break;
                case EntityDisplacementModes.Walking:
                    if (_physicSimu.OnGround)
                    {
                        WalkingFirstPersonOnGround(ref timeSpent);
                    }
                    else
                    {
                        WalkingFirstPersonNotOnGround(ref timeSpent);
                    }
                    break;
                default:
                    break;
            }
        }

        private void SwimmingFreeFirstPersonMove(ref GameTime timeSpent)
        {
            float jumpPower;

            if (_inputsManager.ActionsManager.isTriggered(UtopiaActions.Move_Jump, out jumpPower))
                _physicSimu.Impulses.Add(new Impulse(ref timeSpent) { ForceApplied = new Vector3D(0, 11 + (2 * jumpPower), 0) });

            if (_inputsManager.ActionsManager.isTriggered(UtopiaActions.Move_Forward, CatchExclusiveAction))
                _physicSimu.Impulses.Add(new Impulse(ref timeSpent) { ForceApplied = 1 * (_lookAt * _moveDelta) * 20 });

            if (_inputsManager.ActionsManager.isTriggered(UtopiaActions.Move_Backward, CatchExclusiveAction))
                _physicSimu.Impulses.Add(new Impulse(ref timeSpent) { ForceApplied = -1 * (_lookAt * _moveDelta) * 20 });

            if (_inputsManager.ActionsManager.isTriggered(UtopiaActions.Move_StrafeLeft, CatchExclusiveAction))
                _physicSimu.Impulses.Add(new Impulse(ref timeSpent) { ForceApplied = -1 * (_entityHeadXAxis * _moveDelta) * 20 });

            if (_inputsManager.ActionsManager.isTriggered(UtopiaActions.Move_StrafeRight, CatchExclusiveAction))
                _physicSimu.Impulses.Add(new Impulse(ref timeSpent) { ForceApplied = 1 * (_entityHeadXAxis * _moveDelta) * 20 });
        }

        private void FreeFirstPersonMove()
        {
            Vector3D moveVector = Vector3D.Zero;

            if (_inputsManager.ActionsManager.isTriggered(UtopiaActions.Move_Forward))
                moveVector += _lookAt;

            if (_inputsManager.ActionsManager.isTriggered(UtopiaActions.Move_Backward))
                moveVector -= _lookAt;

            if (_inputsManager.ActionsManager.isTriggered(UtopiaActions.Move_StrafeLeft))
                moveVector -= _entityHeadXAxis;

            if (_inputsManager.ActionsManager.isTriggered(UtopiaActions.Move_StrafeRight))
                moveVector += _entityHeadXAxis;

            if (_inputsManager.ActionsManager.isTriggered(UtopiaActions.Move_Down))
                moveVector += Vector3D.Down;

            if (_inputsManager.ActionsManager.isTriggered(UtopiaActions.Move_Up))
                moveVector += Vector3D.Up;

            moveVector.Normalize();
            _worldPosition.Value += moveVector * _moveDelta;
        }

        private void WalkingFirstPersonOnGround(ref GameTime timeSpent)
        {
            Vector3D moveVector = Vector3D.Zero;
            float moveModifier = 1;

            float jumpPower;

            if (_inputsManager.ActionsManager.isTriggered(UtopiaActions.EndMove_Forward) ||
                _inputsManager.ActionsManager.isTriggered(UtopiaActions.EndMove_Backward) ||
                _inputsManager.ActionsManager.isTriggered(UtopiaActions.EndMove_StrafeLeft) ||
                _inputsManager.ActionsManager.isTriggered(UtopiaActions.EndMove_StrafeRight))
            {
                _stopMovedAction = true;
            }
            else
            {

                //Move 2 time slower if not touching ground
                if (!_physicSimu.OnGround) _moveDelta /= 2f;

                //Do a small "Jump" of hitted a offset wall
                if (OffsetBlockHitted > 0 && _physicSimu.OnGround)
                {
                    //Force of 8 for 0.5 offset
                    //Force of 2 for 0.1 offset
                    _physicSimu.Impulses.Add(new Impulse(ref timeSpent) { ForceApplied = new Vector3D(0, MathHelper.FullLerp(2, 3.8f, 0.1, 0.5, OffsetBlockHitted), 0) });
                    OffsetBlockHitted = 0;
                }

                if ((_physicSimu.OnGround || _physicSimu.PrevPosition == _physicSimu.CurPosition) && _inputsManager.ActionsManager.isTriggered(UtopiaActions.Move_Jump, out jumpPower))
                    _physicSimu.Impulses.Add(new Impulse(ref timeSpent) { ForceApplied = new Vector3D(0, 7 + (2 * jumpPower), 0) });

                if (_inputsManager.ActionsManager.isTriggered(UtopiaActions.Move_Forward))
                    moveVector += _entityZAxis;

                if (_inputsManager.ActionsManager.isTriggered(UtopiaActions.Move_Backward))
                    moveVector -= _entityZAxis;

                if (_inputsManager.ActionsManager.isTriggered(UtopiaActions.Move_StrafeLeft))
                    moveVector += _entityXAxis;

                if (_inputsManager.ActionsManager.isTriggered(UtopiaActions.Move_StrafeRight))
                    moveVector -= _entityXAxis;
            }

            moveVector.Normalize();
            //Run only if Move forward and run button pressed at the same time.
            if (_inputsManager.ActionsManager.isTriggered(UtopiaActions.Move_Forward) && (_inputsManager.ActionsManager.isTriggered(UtopiaActions.Move_Run)))
            {
                moveModifier = 1.5f;
            }

            if (moveVector != Vector3D.Zero) _stopMovedAction = false;
            _physicSimu.Impulses.Add(new Impulse(ref timeSpent) { ForceApplied = moveVector * 2 * moveModifier });

        }

        private void WalkingFirstPersonNotOnGround(ref GameTime timeSpent)
        {
            Vector3D moveVector = Vector3D.Zero;
            float moveModifier = 1;

            float jumpPower;
            _physicSimu.Freeze(true, false, true); //Trick to easy ground deplacement, it will nullify all accumulated forced being applied on the entity (Except the Y ones)

            //Move 2 time slower if not touching ground
            if (!_physicSimu.OnGround) _moveDelta /= 2f;

            //Do a small "Jump" of hitted a offset wall
            if (OffsetBlockHitted > 0 && _physicSimu.OnGround)
            {
                //Force of 8 for 0.5 offset
                //Force of 2 for 0.1 offset
                _physicSimu.Impulses.Add(new Impulse(ref timeSpent) { ForceApplied = new Vector3D(0, MathHelper.FullLerp(2, 3.8f, 0.1, 0.5, OffsetBlockHitted), 0) });
                OffsetBlockHitted = 0;
            }

            if ((_physicSimu.OnGround || _physicSimu.PrevPosition == _physicSimu.CurPosition) && _inputsManager.ActionsManager.isTriggered(UtopiaActions.Move_Jump, out jumpPower))
                _physicSimu.Impulses.Add(new Impulse(ref timeSpent) { ForceApplied = new Vector3D(0, 7 + (2 * jumpPower), 0) });

            if (_inputsManager.ActionsManager.isTriggered(UtopiaActions.Move_Forward))
                moveVector -= _entityZAxis;

            if (_inputsManager.ActionsManager.isTriggered(UtopiaActions.Move_Backward))
                moveVector += _entityZAxis;

            if (_inputsManager.ActionsManager.isTriggered(UtopiaActions.Move_StrafeLeft))
                moveVector -= _entityXAxis;

            if (_inputsManager.ActionsManager.isTriggered(UtopiaActions.Move_StrafeRight))
                moveVector += _entityXAxis;

            moveVector.Normalize();
            //Run only if Move forward and run button pressed at the same time.
            if (_inputsManager.ActionsManager.isTriggered(UtopiaActions.Move_Forward) && (_inputsManager.ActionsManager.isTriggered(UtopiaActions.Move_Run)))
            {
                moveModifier = 2;
            }
            _physicSimu.PrevPosition += moveVector * _moveDelta * moveModifier;

        }
        #endregion

        #region Head + Body Rotation management
        private void EntityRotationsOnEvents(EntityDisplacementModes mode)
        {
            if (_inputsManager.MouseManager.MouseCapture)
            {
                Rotate(_inputsManager.MouseManager.MouseMoveDelta.X, _inputsManager.MouseManager.MouseMoveDelta.Y, 0.0f, mode);
            }
        }

        private void Rotate(double headingDegrees, double pitchDegrees, double rollDegrees, EntityDisplacementModes mode)
        {
            if (headingDegrees == 0 && pitchDegrees == 0 && rollDegrees == 0) return;

            //Affect mouse sensibility stored in Delta to the mouvement that has been realized
            headingDegrees *= _rotationDelta;
            pitchDegrees *= _rotationDelta;
            rollDegrees *= _rotationDelta;

            switch (mode)
            {
                case EntityDisplacementModes.Walking:
                case EntityDisplacementModes.Flying:
                case EntityDisplacementModes.Swiming:
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
                _moveDirection.Value = _moveDirection.Value * rotation;
            }

            _moveDirection.Value.Normalize();

            UpdateEntityData();
        }
        private void UpdateEntityData()
        {
            Matrix.RotationQuaternion(ref _moveDirection.Value, out _entityRotation);

            _entityXAxis = new Vector3D(_entityRotation.M11, _entityRotation.M21, _entityRotation.M31);
            _entityYAxis = new Vector3D(_entityRotation.M12, _entityRotation.M22, _entityRotation.M32);
            _entityZAxis = new Vector3D(_entityRotation.M13, _entityRotation.M23, _entityRotation.M33) * -1;
        }

        private void RotateLookAt(double headingDegrees, double pitchDegrees)
        {
            //To avoid the Camera to make full loop on the Pitch axis
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

            //Inverse the mouse move impact on the rotation
            double heading = MathHelper.ToRadians(headingDegrees) * -1;
            double pitch = MathHelper.ToRadians(pitchDegrees);
            Quaternion rotation;

            // Rotate the camera about the world Y axis.
            if (heading != 0.0f)
            {
                Quaternion.RotationAxis(ref MVector3.Up, (float)heading, out rotation); //Transform the rotation angle from mouse into a quaternion
                _lookAtDirection.Value = _lookAtDirection.Value * rotation;             //Add this value to the existing Entity quaternion rotation
                _cameraYAxisOrientation.Value = _cameraYAxisOrientation.Value * rotation;
            }

            // Rotate the camera about its local X axis.
            if (pitch != 0.0f)
            {
                Quaternion.RotationAxis(ref MVector3.Right, (float)pitch, out rotation);

                _lookAtDirection.Value = rotation * _lookAtDirection.Value;
            }


            _lookAtDirection.Value.Normalize();
            UpdateHeadData();
        }

        private void UpdateHeadData()
        {
            //Get the lookAt vector
            _headRotation = Matrix.RotationQuaternion(Quaternion.Conjugate(_lookAtDirection.Value));

            _entityHeadXAxis = new Vector3D(_headRotation.M11, _headRotation.M21, _headRotation.M31) * -1;
            _entityHeadYAxis = new Vector3D(_headRotation.M12, _headRotation.M22, _headRotation.M32);
            _entityHeadZAxis = new Vector3D(_headRotation.M13, _headRotation.M23, _headRotation.M33) * -1;

            _lookAt = new Vector3D(_entityHeadZAxis.X, _entityHeadZAxis.Y, _entityHeadZAxis.Z);
            _lookAt.Normalize();
        }
        #endregion
        #endregion
        #endregion

        #region Public Methods
        public override void Initialize()
        {
            //Compute the Eye position into the entity
            _entityEyeOffset = new Vector3(0, Player.Size.Y / 100 * 80, 0);

            //Set Position
            //Set the entity world position following the position received from server
            _worldPosition.Value = Player.Position;
            _worldPosition.ValuePrev = Player.Position;

            //Compute the initial Player world bounding box
            VisualEntity.RefreshWorldBoundingBox(ref _worldPosition.Value);

            //Init Velret physic simulator
            _physicSimu = new VerletSimulator(ref VisualEntity.LocalBBox) { WithCollisionBouncing = false };
            _physicSimu.ConstraintFct += WorldChunks.isCollidingWithTerrain;
            _physicSimu.ConstraintFct += _entityPickingManager.isCollidingWithEntity;

            //Set displacement mode
            DisplacementMode = Player.DisplacementMode;

            //Set LookAt
            //Take back only the saved server Yaw rotation (Or Heading) and only using it;
            _lookAtDirection.Value = Player.HeadRotation;

            double playerSavedYaw = MQuaternion.getYaw(ref _lookAtDirection.Value);
            Quaternion.RotationAxis(ref MVector3.Up, (float)playerSavedYaw, out _lookAtDirection.Value);            
            _lookAtDirection.ValuePrev = _lookAtDirection.Value;

            _cameraYAxisOrientation.Value = _lookAtDirection.Value;
            _cameraYAxisOrientation.ValuePrev = _lookAtDirection.Value;

            //Set Move direction = to LookAtDirection
            _moveDirection.Value = _lookAtDirection.Value;

            _playerRenderer.Initialize();
        }

        /// <summary>
        /// The allocated object here must be disposed
        /// </summary>
        public override void LoadContent(DeviceContext context)
        {
            _playerRenderer.LoadContent(context);
        }

        public override void UnloadContent()
        {
            this.DisableComponent();
            _playerRenderer.UnloadContent();
            this.IsInitialized = false;
        }

        //string[] test = new string[100];
        //int i;
        //long from, to;
        public override void Update( GameTime timeSpend)
        {
            //from = System.Diagnostics.Stopwatch.GetTimestamp();
            if (_landscapeInitiazed == false) return;

            inputHandler();             //Input handling

            GetSelectedEntity();         //Player Block Picking handling

            RefreshEntityMovementAndRotation(ref timeSpend);   //Refresh player Movement + rotation

            CheckAfterNewPosition();

            //Refresh the player Bounding box
            VisualEntity.RefreshWorldBoundingBox(ref _worldPosition.Value);

            //to = System.Diagnostics.Stopwatch.GetTimestamp() - from;
            //test[i] = to.ToString();
            //i++; if (i >= test.Length) i = 0;
        }


        public override void Interpolation(double interpolationHd, float interpolationLd, long timePassed)
        {
            //TODO FIXME NAsty bug here, not a number float arithmetic exception sometimes - surely a server side fix to do !
            Quaternion.Slerp(ref _lookAtDirection.ValuePrev, ref _lookAtDirection.Value, interpolationLd, out _lookAtDirection.ValueInterp);
            Quaternion.Slerp(ref _cameraYAxisOrientation.ValuePrev, ref _cameraYAxisOrientation.Value, interpolationLd, out _cameraYAxisOrientation.ValueInterp);
            Vector3D.Lerp(ref _worldPosition.ValuePrev, ref _worldPosition.Value, interpolationHd, out _worldPosition.ValueInterp);

            //TODO To remove when Voxel Entity merge will done with Entity
            //Update the position and World Matrix of the Voxel body of the Entity.
            Vector3 entityCenteredPosition = _worldPosition.ValueInterp.AsVector3();
            //entityCenteredPosition.X -= Player.Size.X / 2;
            //entityCenteredPosition.Z -= Player.Size.Z / 2;
            VisualEntity.World = Matrix.RotationQuaternion(_lookAtDirection.ValueInterp) * Matrix.Translation(entityCenteredPosition);
            //VisualEntity.World = Matrix.Scaling(Player.Size) * Matrix.Translation(entityCenteredPosition);
            //===================================================================================================================================

            CheckHeadUnderWater();      //Under water head test
        }

        public override void Draw(DeviceContext context, int index)
        {
            _playerRenderer.Draw(context, index);
        }

        #endregion

        //Debug Info interface
        public bool ShowDebugInfo { get; set; }
        public string GetDebugInfo()
        {
            return string.Format("Player {0} Pos: [{1:000}; {2:000}; {3:000}] PickedBlock: {4}; NewBlockPlace: {5}", Player.CharacterName,
                                                                                  Math.Round(Player.Position.X, 1),
                                                                                  Math.Round(Player.Position.Y, 1),
                                                                                  Math.Round(Player.Position.Z, 1),
                                                                                  Player._entityState.IsBlockPicked ? Player._entityState.PickedBlockPosition.ToString() : "None",
                                                                                  Player._entityState.IsBlockPicked ? Player._entityState.NewBlockPosition.ToString() : "None"
                                                                                  );            
        }
    }
}
