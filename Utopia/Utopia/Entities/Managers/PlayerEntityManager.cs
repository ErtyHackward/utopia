using System;
using Ninject;
using Nuclex.UserInterface;
using S33M3Engines;
using S33M3Engines.Cameras;
using S33M3Engines.D3D;
using S33M3Engines.D3D.DebugTools;
using S33M3Engines.Maths;
using S33M3Engines.Shared.Math;
using S33M3Engines.Shared.Sprites;
using S33M3Engines.Struct;
using S33M3Engines.WorldFocus;
using S33M3Physics;
using S33M3Physics.Verlet;
using SharpDX;
using Utopia.Action;
using Utopia.Entities.Managers.Interfaces;
using Utopia.Entities.Renderer;
using Utopia.Entities.Voxel;
using Utopia.GUI.D3D.Inventory;
using Utopia.InputManager;
using Utopia.Shared.Chunks;
using Utopia.Shared.Chunks.Entities;
using Utopia.Shared.Cubes;
using Utopia.Shared.Structs;
using Utopia.Shared.Structs.Landscape;

namespace Utopia.Entities.Managers
{
    public class PlayerEntityManager : DrawableGameComponent, ICameraPlugin, IVisualEntityContainer, IDebugInfo
    {
        #region Private variables
        //Engine System variables
        private D3DEngine _d3DEngine;
        private CameraManager _cameraManager;
        private WorldFocusManager _worldFocusManager;
        private ActionsManager _actions;
        private InputsManager _inputsManager;
        private SingleArrayChunkContainer _cubesHolder;

        //Block Picking variables
        private TerraCubeWithPosition _pickedCube;

        //Head UnderWater test
        private int _headCubeIndex;
        private TerraCube _headCube;

        //Player Visual characteristics (Not insde the PlayerCharacter object)
        private IVisualEntityContainer _pickedUpEntity;
        private Vector3D _pickedUpEntityPosition;
        
        private FTSValue<Vector3D> _worldPosition = new FTSValue<Vector3D>();         //World Position
        private FTSValue<Quaternion> _lookAtDirection = new FTSValue<Quaternion>();   //LookAt angle
        private FTSValue<Quaternion> _moveDirection = new FTSValue<Quaternion>();     //Real move direction (derived from LookAt, but will depend the mode !)
        private Vector3D _lookAt;
        private Vector3 _entityEyeOffset;                                     //Offset of the camera Placement inside the entity, from entity center point.

        //Mouvement handling variables
        private VerletSimulator _physicSimu;
        private EntityDisplacementModes _displacementMode;
        private double _accumPitchDegrees;
        private double _gravityInfluence;
        private float _groundBelowEntity;
        private double _rotationDelta;
        private double _moveDelta;
        private Matrix _headRotation;
        private Matrix _entityRotation;
        private Vector3D _entityHeadXAxis, _entityHeadYAxis, _entityHeadZAxis;
        private Vector3D _entityXAxis, _entityYAxis, _entityZAxis;
        private IPickingRenderer _pickingRenderer;
        private IEntityPickingManager _entityPickingManager;
        private readonly Screen _screen;

        //Drawing component
        private IEntitiesRenderer _playerRenderer;
        #endregion

        #region Public variables/properties
        /// <summary>
        /// The Player
        /// </summary>
        public readonly PlayerCharacter Player;

        private InventoryWindow _inventoryUi;
        private SpriteTexture _backgroundTex;

        /// <summary>
        /// The Player Voxel body
        /// </summary>
        public VisualEntity VisualEntity { get; set; }

        //Implement the interface Needed when a Camera is "plugged" inside this entity
        public virtual Vector3D CameraWorldPosition { get { return _worldPosition.Value + _entityEyeOffset; } }
        public virtual Quaternion CameraOrientation { get { return _lookAtDirection.Value; } }

        public bool IsHeadInsideWater { get; set; }

        public EntityDisplacementModes DisplacementMode
        {
            get { return _displacementMode; }
            set
            {
                _displacementMode = value;
                if (_displacementMode == EntityDisplacementModes.Walking)
                {
                    _physicSimu.StartSimulation(ref _worldPosition.Value, ref _worldPosition.Value);
                }
                else
                {
                    _physicSimu.StopSimulation();
                }
            }
        }
        #endregion


        public PlayerEntityManager(D3DEngine engine,
                                   CameraManager cameraManager,
                                   WorldFocusManager worldFocusManager,
                                   ActionsManager actions,
                                   InputsManager inputsManager,
                                   SingleArrayChunkContainer cubesHolder,
                                   VisualEntity visualEntity,
                                   PlayerCharacter player,
                                   [Named("PlayerEntityRenderer")] IEntitiesRenderer playerRenderer,
                                   IPickingRenderer pickingRenderer,
                                   IEntityPickingManager entityPickingManager,
                                   Screen screen
            )
        {
            _d3DEngine = engine;
            _cameraManager = cameraManager;
            _worldFocusManager = worldFocusManager;
            _actions = actions;
            _inputsManager = inputsManager;
            _cubesHolder = cubesHolder;
            _playerRenderer = playerRenderer;
            _pickingRenderer = pickingRenderer;
            _entityPickingManager = entityPickingManager;
            _screen = screen;


            entityPickingManager.Player = this;
            this.Player = player;
            this.VisualEntity = visualEntity;

            //Give the Renderer acces to the Voxel buffers, ...
            _playerRenderer.VisualEntity = this;
        }

        public override void Dispose()
        {
            this.VisualEntity.Dispose();
            // _playerRenderer.Dispose(); ==> REgistered with Ninject
            _backgroundTex.Dispose();
        }

        #region Private Methods

        #region Player InputHandling
        /// <summary>
        /// Handle Player input handling - Movement and rotation input are not handled here
        /// </summary>
        private void inputHandler()
        {

            if (_actions.isTriggered(Actions.Move_Mode))
            {
                if (_displacementMode == EntityDisplacementModes.Flying)
                {
                    DisplacementMode = EntityDisplacementModes.Walking;
                }
                else
                {
                    DisplacementMode = EntityDisplacementModes.Flying;
                }
            }

            if (_actions.isTriggered(Actions.Use_Left))
            {
                if (Player.EntityState.IsPickingActive)
                {
                    Player.LeftToolUse();
                }
            }

            if (_actions.isTriggered(Actions.Use_Right))
            {
                //Avoid the player to add a block where he is located !
                if (Player.EntityState.IsPickingActive)
                {
                    BoundingBox playerPotentialNewBlock;
                    ComputeBlockBoundingBox(ref Player._entityState.NewBlockPosition, out playerPotentialNewBlock);

                    if (!MBoundingBox.Intersects(ref VisualEntity.WorldBBox, ref playerPotentialNewBlock))
                    {
                        Player.RightToolUse();
                    }
                }
            }

            if (_actions.isTriggered(Actions.EntityUse))
            {
                //TODO implement use 'picked' entity (picked here means entity is in world having cursor over it, not in your hand or pocket) 
                //like opening a chest or a door  
            }


            if (_actions.isTriggered(Actions.EntityThrow))
            {
                //TODO unequip left item and throw it on the ground, (version 0 = place it at newCubeplace, animation later)                
                // and next, throw the right tool if left tool is already thrown
            }

            if (_actions.isTriggered(Actions.OpenInventory))
            {
                 if (_screen.Desktop.Children.Contains(_inventoryUi))
                 {
                     _screen.Desktop.Children.Remove(_inventoryUi);
                 }
                else
                 {
                     _inventoryUi.Refresh();
                     _screen.Desktop.Children.Add(_inventoryUi);
                 }
            }

        }
        #endregion

        #region Player Block Picking
        private void GetSelectedEntity()
        {
            bool newpicking;

            if (!_d3DEngine.UnlockedMouse)
            {
                Vector3D pickingPointInLine = _worldPosition.Value + _entityEyeOffset;
                newpicking = RefreshPicking(ref pickingPointInLine, ref _lookAt, 1);
            }
            else
            {
                Vector3D mouseWorldPosition;
                Vector3D mouseLookAtPosition;
                _inputsManager.UnprojectMouseCursor(out mouseWorldPosition, out mouseLookAtPosition);
                newpicking = RefreshPicking(ref mouseWorldPosition, ref mouseLookAtPosition, 2);
            }

            if(newpicking)
            {
                //A new Block has been pickedup
                if (Player._entityState.PickedEntityId == 0)
                {
                    _pickingRenderer.SetPickedBlock(ref Player._entityState.PickedBlockPosition);
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

            Player._entityState.IsPickingActive = false;

            //Sample 500 points in the view direction vector
            for (int ptNbr = 0; ptNbr < 500; ptNbr++)
            {
                pickingWorldPosition += pickingLookAt * 0.02;

                //Check if a block is picked up !
                if (_cubesHolder.isPickable(ref pickingWorldPosition, out _pickedCube))
                {
                    Player._entityState.PickedBlockPosition = _pickedCube.Position;

                    //Find the Potential new block place, by roling back !
                    while (ptNbr > 0)
                    {
                        pickingWorldPosition -= pickingLookAt * 0.02;

                        if (_cubesHolder.isPickable(ref pickingWorldPosition, out _pickedCube) == false)
                        {
                            Player._entityState.NewBlockPosition = _pickedCube.Position;
                            break;
                        }
                        ptNbr--;
                    }

                    if (_pickedCube.Position == Player._entityState.PickedBlockPosition)
                    {
                        Player._entityState.PickedEntityId = 0;
                        Player._entityState.IsPickingActive = true;
                        return false;
                    }

                    Player._entityState.PickedEntityId = 0;
                    Player._entityState.IsPickingActive = true;
                    break;
                }

                //Check if an entity is picked up HERE !
                if (_entityPickingManager.CheckEntityPicking(ref pickingWorldPosition, out _pickedUpEntity))
                {
                    //if (Player._entityState.PickedEntityId == _pickedUpEntity.VisualEntity.VoxelEntity.EntityId && _pickedUpEntityPosition == _pickedUpEntity.VisualEntity.Position)
                    //{
                    //    Player._entityState.IsPickingActive = true;
                    //    return false;
                    //}

                    _pickedUpEntityPosition = _pickedUpEntity.VisualEntity.Position;
                    Player._entityState.PickedEntityId = _pickedUpEntity.VisualEntity.VoxelEntity.EntityId;
                    Player._entityState.IsPickingActive = true;
                    break;
                }

            }

            return Player._entityState.IsPickingActive; //Return true if a new block or Entity has been picked up !
        }

        private void ComputeBlockBoundingBox(ref Vector3I BlockPlace, out BoundingBox BlockBoundingBox)
        {
            BlockBoundingBox = new BoundingBox(new Vector3(BlockPlace.X, BlockPlace.Y, BlockPlace.Z), new Vector3(BlockPlace.X + 1, BlockPlace.Y + 1, BlockPlace.Z + 1));
        }
        #endregion

        #region UnderWaterTest
        private void CheckHeadUnderWater()
        {
            if (_cubesHolder.IndexSafe(MathHelper.Fastfloor(CameraWorldPosition.X), MathHelper.Fastfloor(CameraWorldPosition.Y), MathHelper.Fastfloor(CameraWorldPosition.Z), out _headCubeIndex))
            {
                //Get the cube at the camera position !
                _headCube = _cubesHolder.Cubes[_headCubeIndex];
                if (_headCube.Id == CubeId.Water || _headCube.Id == CubeId.WaterSource)
                {
                    //TODO Take into account the Offseting in case of Offseted Water !
                    IsHeadInsideWater = true;
                }
                else
                {
                    IsHeadInsideWater = false;
                }
            }
        }
        #endregion

        #region Physic simulation for Collision detection
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

        /// <summary>
        /// Check if the player is on the ground, or not.
        /// </summary>
        /// <param name="TimeSpend"></param>
        private void PhysicSimulation(ref GameTime TimeSpend)
        {
            TerraCubeWithPosition groundCube;
            Vector3I GroundDirection = new Vector3I(0, -1, 0);
            Vector3D newWorldPosition;

            _cubesHolder.GetNextSolidBlockToPlayer(ref VisualEntity.WorldBBox, ref GroundDirection, out groundCube);
            _groundBelowEntity = groundCube.Position.Y + 1;

            _physicSimu.Simulate(ref TimeSpend, out newWorldPosition);
            _worldPosition.Value = newWorldPosition;

            if (_worldPosition.Value.Y > _groundBelowEntity)
            {
                _physicSimu.OnGround = false;
            }
        }

        /// <summary>
        /// Validate player move against surrending landscape, if move not possible, it will be "rollbacked"
        /// </summary>
        /// <param name="newPosition2Evaluate"></param>
        /// <param name="previousPosition"></param>
        private void isCollidingWithTerrain(ref Vector3D newPosition2Evaluate, ref Vector3D previousPosition)
        {
            BoundingBox _boundingBox2Evaluate;
            Vector3D newPositionWithColliding = previousPosition;
            //Create a Bounding box with my new suggested position, taking only the X that has been changed !
            //X Testing
            newPositionWithColliding.X = newPosition2Evaluate.X;

            VisualEntity.ComputeWorldBoundingBox(ref newPositionWithColliding, out _boundingBox2Evaluate);
            if (_cubesHolder.IsSolidToPlayer(ref _boundingBox2Evaluate))
                newPositionWithColliding.X = previousPosition.X;

            //Y Testing
            newPositionWithColliding.Y = newPosition2Evaluate.Y;

            //My Position raise  ==> If I were on the ground, I'm no more
            if (previousPosition.Y < newPositionWithColliding.Y && _physicSimu.OnGround) _physicSimu.OnGround = false;

            VisualEntity.ComputeWorldBoundingBox(ref newPositionWithColliding, out _boundingBox2Evaluate);
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
            VisualEntity.ComputeWorldBoundingBox(ref newPositionWithColliding, out _boundingBox2Evaluate);
            if (_cubesHolder.IsSolidToPlayer(ref _boundingBox2Evaluate))
                newPositionWithColliding.Z = previousPosition.Z;

            newPosition2Evaluate = newPositionWithColliding;
        }
        #endregion

        #region Entity Movement + rotation

        private void RefreshEntityMovementAndRotation(ref GameTime timeSpent)
        {
            switch (_displacementMode)
            {
                case EntityDisplacementModes.Flying:
                    _gravityInfluence = 3;  // We will move 6 times faster if flying
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
            _moveDelta = Player.MoveSpeed * _gravityInfluence * timeSpent.ElapsedGameTimeInS_HD;
            _rotationDelta = Player.RotationSpeed * timeSpent.ElapsedGameTimeInS_HD;

            //Backup previous values
            _lookAtDirection.BackUpValue();
            _worldPosition.BackUpValue();

            //Rotation with mouse
            EntityRotationsOnEvents(_displacementMode);

            //Movement
            EntityMovementsOnEvents(_displacementMode, ref timeSpent);

            //Physic simulation !
            PhysicOnEntity(_displacementMode, ref timeSpent);

            //Send the Actual Position to the Entity object only of it has change !!!
            //The Change check is done at DynamicEntity level
            Player.Position = _worldPosition.Value;
            Player.Rotation = _lookAtDirection.Value;
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
                _worldPosition.Value += _lookAt * _moveDelta;

            if (_actions.isTriggered(Actions.Move_Backward))
                _worldPosition.Value -= _lookAt * _moveDelta;

            if (_actions.isTriggered(Actions.Move_StrafeLeft))
                _worldPosition.Value -= _entityHeadXAxis * _moveDelta;

            if (_actions.isTriggered(Actions.Move_StrafeRight))
                _worldPosition.Value += _entityHeadXAxis * _moveDelta;

            if (_actions.isTriggered(Actions.Move_Down))
                _worldPosition.Value += Vector3D.Down * _moveDelta;

            if (_actions.isTriggered(Actions.Move_Up))
                _worldPosition.Value += Vector3D.Up * _moveDelta;
        }

        private void WalkingFirstPerson(ref GameTime TimeSpend)
        {
            _physicSimu.Freeze(true, false, true);

            //Move 3 time slower if not touching ground
            if (!_physicSimu.OnGround) _moveDelta /= 2f;

            if (_actions.isTriggered(Actions.Move_Forward))
                if (_actions.isTriggered(Actions.Move_Run)) _physicSimu.PrevPosition += _entityZAxis * _moveDelta * 2f; //Running makes the entity go twice faster
                else _physicSimu.PrevPosition += _entityZAxis * _moveDelta;

            if (_actions.isTriggered(Actions.Move_Backward))
                _physicSimu.PrevPosition -= _entityZAxis * _moveDelta;

            if (_actions.isTriggered(Actions.Move_StrafeLeft))
                _physicSimu.PrevPosition += _entityXAxis * _moveDelta;

            if (_actions.isTriggered(Actions.Move_StrafeRight))
                _physicSimu.PrevPosition -= _entityXAxis * _moveDelta;

            if (_physicSimu.OnGround && _actions.isTriggered(Actions.Move_Jump))
                _physicSimu.Impulses.Add(new Impulse(ref TimeSpend) { ForceApplied = new Vector3D(0, 300, 0) });
        }
        #endregion

        #region Head + Body Rotation management
        private void EntityRotationsOnEvents(EntityDisplacementModes mode)
        {
            if (_d3DEngine.UnlockedMouse == false)
            {
                Rotate(_inputsManager.MouseMoveDelta.X, _inputsManager.MouseMoveDelta.Y, 0.0f, mode);
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
                    RotateLookAt(-headingDegrees, -pitchDegrees);
                    RotateMove(-headingDegrees);
                    break;
                case EntityDisplacementModes.Walking:
                    RotateLookAt(-headingDegrees, -pitchDegrees);
                    RotateMove(-headingDegrees);
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
            Matrix.Transpose(ref _entityRotation, out _entityRotation);

            _entityXAxis = new Vector3D(_entityRotation.M11, _entityRotation.M21, _entityRotation.M31);
            _entityYAxis = new Vector3D(_entityRotation.M12, _entityRotation.M22, _entityRotation.M32);
            _entityZAxis = new Vector3D(_entityRotation.M13, _entityRotation.M23, _entityRotation.M33);
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
                _lookAtDirection.Value = _lookAtDirection.Value * rotation;
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
            Matrix.RotationQuaternion(ref _lookAtDirection.Value, out _headRotation);
            Matrix.Transpose(ref _headRotation, out _headRotation);

            _entityHeadXAxis = new Vector3D(_headRotation.M11, _headRotation.M21, _headRotation.M31);
            _entityHeadYAxis = new Vector3D(_headRotation.M12, _headRotation.M22, _headRotation.M32);
            _entityHeadZAxis = new Vector3D(_headRotation.M13, _headRotation.M23, _headRotation.M33);

            _lookAt = new Vector3D(-_entityHeadZAxis.X, -_entityHeadZAxis.Y, -_entityHeadZAxis.Z);
            _lookAt.Normalize();
        }
        #endregion
        #endregion

        #endregion

        #region Public Methods
        public override void Initialize()
        {
            //Init Velret physic simulator
            _physicSimu = new VerletSimulator(ref VisualEntity.WorldBBox) { WithCollisionBounsing = false };
            _physicSimu.ConstraintFct += isCollidingWithTerrain;
            _physicSimu.ConstraintFct += _entityPickingManager.isCollidingWithEntity;

            //Set displacement mode
            DisplacementMode = Player.DisplacementMode;

            //Compute the Eye position into the entity
            _entityEyeOffset = new Vector3(0, Player.Size.Y / 100 * 80, 0);

            ////Will be used to update the bounding box with world coordinate when the entity is moving
            //VisualEntity.LocalBBox.Minimum = new Vector3(-(Player.Size.X / 2.0f), 0, -(Player.Size.Z / 2.0f));
            //VisualEntity.LocalBBox.Maximum = new Vector3(+(Player.Size.X / 2.0f), Player.Size.Y, +(Player.Size.Z / 2.0f));

            //Compute the initial Player world bounding box
            VisualEntity.RefreshWorldBoundingBox(ref _worldPosition.Value);

            //Set Position
            //Set the entity world position following the position received from server
            _worldPosition.Value = Player.Position;
            _worldPosition.ValuePrev = Player.Position;

            //Set LookAt
            //Take back only the saved server Yaw rotation (Or Heading) and only using it;
            _lookAtDirection.Value = Player.Rotation;

            double playerSavedYaw = MQuaternion.getYaw(ref _lookAtDirection.Value);
            Quaternion.RotationAxis(ref MVector3.Up, (float)playerSavedYaw, out _lookAtDirection.Value);
            _lookAtDirection.ValuePrev = _lookAtDirection.Value;

            //Set Move direction = to LookAtDirection
            _moveDirection.Value = _lookAtDirection.Value;
        }

        /// <summary>
        /// The allocated object here must be disposed
        /// </summary>
        public override void LoadContent()
        {
             _backgroundTex = new SpriteTexture(_d3DEngine.Device, @"Textures\charactersheet.png", new Vector2(0, 0));
             _inventoryUi = new InventoryWindow(_backgroundTex, Player);
        }

        public override void Update(ref GameTime timeSpent)
        {
            inputHandler();             //Input handling

            GetSelectedEntity();         //Player Block Picking handling

            CheckHeadUnderWater();      //Under water head test

            RefreshEntityMovementAndRotation(ref timeSpent);   //Refresh player Movement + rotation

            //Refresh the player Bounding box
            VisualEntity.RefreshWorldBoundingBox(ref _worldPosition.Value);
            
            _playerRenderer.Update(ref timeSpent);
        }

        public override void Interpolation(ref double interpolationHd, ref float interpolationLd)
        {
            Quaternion.Slerp(ref _lookAtDirection.ValuePrev, ref _lookAtDirection.Value, interpolationLd, out _lookAtDirection.ValueInterp);
            Vector3D.Lerp(ref _worldPosition.ValuePrev, ref _worldPosition.Value, interpolationHd, out _worldPosition.ValueInterp);

            _playerRenderer.Interpolation(ref interpolationHd, ref interpolationLd);

            //TODO To remove when Voxel Entity merge will done with Entity
            //Update the position and World Matrix of the Voxel body of the Entity.
            Vector3 entityCenteredPosition = _worldPosition.ValueInterp.AsVector3();
            //entityCenteredPosition.X -= Player.Size.X / 2;
            //entityCenteredPosition.Z -= Player.Size.Z / 2;
            VisualEntity.World = Matrix.Scaling(Player.Size) * Matrix.RotationQuaternion(_lookAtDirection.ValueInterp) * Matrix.Translation(entityCenteredPosition);
            //VisualEntity.World = Matrix.Scaling(Player.Size) * Matrix.Translation(entityCenteredPosition);
            //===================================================================================================================================
        }

        public override void Draw(int Index)
        {
            _playerRenderer.Draw(Index);
        }

        #endregion

        public string GetInfo()
        {
            return string.Format("Player {0} Pos:[{1}; {2}; {3}] PickedBlock:{4}; NewBlockPlace:{5}", Player.CharacterName,
                                                                                  Math.Round(Player.Position.X, 1),
                                                                                  Math.Round(Player.Position.Y, 1),
                                                                                  Math.Round(Player.Position.Z, 1),
                                                                                  Player._entityState.IsPickingActive ? Player._entityState.PickedBlockPosition.ToString() : "None",
                                                                                  Player._entityState.IsPickingActive ? Player._entityState.NewBlockPosition.ToString() : "None"
                                                                                  );
        }
    }
}
