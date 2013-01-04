using System;
using SharpDX;
using Utopia.Entities.Managers.Interfaces;
using Utopia.Entities.Voxel;
using Utopia.GUI.Inventory;
using Utopia.Network;
using Utopia.Shared.Chunks;
using Utopia.Shared.Entities;
using Utopia.Shared.Entities.Concrete;
using Utopia.Shared.Entities.Dynamic;
using Utopia.Shared.Entities.Interfaces;
using Utopia.Shared.Interfaces;
using Utopia.Shared.Structs;
using Utopia.Shared.Structs.Landscape;
using Utopia.Entities.Renderer.Interfaces;
using Ninject;
using Utopia.Worlds.Chunks;
using Utopia.Shared.Settings;
using S33M3Resources.Structs;
using S33M3DXEngine.Main;
using S33M3CoreComponents.Cameras.Interfaces;
using S33M3DXEngine;
using S33M3CoreComponents.Cameras;
using S33M3CoreComponents.WorldFocus;
using S33M3CoreComponents.Inputs;
using S33M3CoreComponents.Physics.Verlet;
using SharpDX.Direct3D11;
using S33M3DXEngine.Debug.Interfaces;
using Utopia.Entities.EntityMovement;
using Utopia.Shared.Configuration;
using Utopia.Shared.World;
using S33M3CoreComponents.Particules;
using Utopia.Components;
using Utopia.Particules;

namespace Utopia.Entities.Managers
{
    /// <summary>
    /// Responsible for:
    /// 1) current player physics interaction with the world. 
    /// 2) player movement input handling
    /// 3) picking of the block
    /// </summary>
    public partial class PlayerEntityManager : GameComponent, ICameraPlugin, IVisualVoxelEntityContainer, IDebugInfo
    {
        private static readonly NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        private IEntity _lockedEntity;

        #region Private variables
        // Engine System variables
        private D3DEngine _d3DEngine;
        private CameraManager<ICameraFocused> _cameraManager;
        private WorldFocusManager _worldFocusManager;
        private InputsManager _inputsManager;
        private SingleArrayChunkContainer _cubesHolder;

        // Block Picking variables
        public TerraCubeWithPosition PickedCube;
        public TerraCubeWithPosition NewCube;

        // Head UnderWater test
        private int _headCubeIndex;
        private TerraCube _headCube;

        // Player Visual characteristics (Not insde the PlayerCharacter object)
        private VisualEntity _pickedUpEntity;
        private Vector3D _pickedUpEntityPosition;

        private Vector3D _worldPosition;         //World Position
        // private Vector3D _lookAt;
        private Vector3 _entityEyeOffset;        //Offset of the camera Placement inside the entity, from entity center point.

        // Mouvement handling variables
        private VerletSimulator _physicSimu;
        private float _gravityInfluence;
        private double _groundBelowEntity;
        private float _moveDelta;
        private IPickingRenderer _pickingRenderer;
        private IEntityPickingManager _entityPickingManager;
        private bool _stopMovedAction = false;

        private VisualWorldParameters _visualWorldParameters;
        private readonly ILandscapeManager2D _landscapeManager;
        private ItemMessageTranslator _itemMessageTranslator;

        // Event related variables
        private double _fallMaxHeight;

        private TerraCubeWithPosition _groundCube;
        private CubeProfile _groundCubeProgile;

        // Will be used to compute entity rotation movements
        private EntityRotations _entityRotations;
        private InventoryComponent _inventoryComponent;
        #endregion

        #region Public variables/properties

        /// <summary>
        /// The Player
        /// </summary>
        public readonly PlayerCharacter Player;

        /// <summary>
        /// The Player Voxel body, its a class that will wrap the player character object with a Voxel Body
        /// </summary>
        public VisualVoxelEntity VisualVoxelEntity { get; set; }

        // Implement the interface Needed when a Camera is "plugged" inside this entity
        public virtual Vector3D CameraWorldPosition { get { return _worldPosition + _entityEyeOffset; } }

        public virtual Quaternion CameraOrientation { get { return _entityRotations.EyeOrientation; } }

        public virtual Quaternion CameraYAxisOrientation { get { return _entityRotations.BodyOrientation; } }

        public virtual int CameraUpdateOrder { get { return UpdateOrder; } }

        public bool IsHeadInsideWater { get; set; }

        public bool CatchExclusiveAction { get; set; }

        public EntityDisplacementModes DisplacementMode
        {
            get { return Player.DisplacementMode; }
            set
            {
                Player.DisplacementMode = value;
                _entityRotations.SetDisplacementMode(Player.DisplacementMode, _worldPosition + _entityEyeOffset);
#if DEBUG
                logger.Info("{0} is now {1}", Player.CharacterName, value.ToString());
#endif
                if (value == EntityDisplacementModes.Walking || value == EntityDisplacementModes.Swiming)
                {
                    _physicSimu.StartSimulation(ref _worldPosition, ref _worldPosition);
                    _physicSimu.ConstraintOnlyMode = false;
                }
//Collision detection not activated oustide debug mode when flying !
#if !DEBUG
                else if (value == EntityDisplacementModes.Flying)
                {
                    _physicSimu.StartSimulation(ref _worldPosition, ref _worldPosition);
                    _physicSimu.ConstraintOnlyMode = true;
                }
#endif
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
        
        public float OffsetBlockHitted { get; set; }
        
        public SingleArrayChunkContainer CubesHolder
        {
            get { return _cubesHolder; }
        }

        [Inject]
        public IEntityPickingManager EntityPickingManager
        {
            get { return _entityPickingManager; }
            set
            {
                _entityPickingManager = value;
                _entityPickingManager.Player = this;
            }
        }


        [Inject]
        public ItemMessageTranslator EntityMessageTranslator
        {
            get { return _itemMessageTranslator; }
            set {
                if (_itemMessageTranslator != null)
                    throw new InvalidOperationException("Already initialized");

                _itemMessageTranslator = value;

                if (_itemMessageTranslator != null)
                {
                    _itemMessageTranslator.EntityLocked += EntityMessageTranslatorEntityLocked;
                    _itemMessageTranslator.EntityLockFailed += EntityMessageTranslatorEntityLockFailed;
                }
            }
        }

        [Inject]
        public InventoryComponent InventoryComponent
        {
            get { return _inventoryComponent; }
            set { 
                _inventoryComponent = value;
                _inventoryComponent.SwitchInventory += InventoryComponentSwitchInventory;
            }
        }

        #endregion

        #region Events

        public delegate void LandingGround(double fallHeight, TerraCubeWithPosition landedCube);
        public event LandingGround OnLanding;

        #endregion

        public PlayerEntityManager(D3DEngine engine,
                                   CameraManager<ICameraFocused> cameraManager,
                                   WorldFocusManager worldFocusManager,
                                   InputsManager inputsManager,
                                   SingleArrayChunkContainer cubesHolder,
                                   PlayerCharacter player,
                                   IPickingRenderer pickingRenderer,
                                   VoxelModelManager voxelModelManager,
                                   VisualWorldParameters visualWorldParameters,
                                   ILandscapeManager2D landscapeManager
            )
        {
            _d3DEngine = engine;
            _cameraManager = cameraManager;
            _worldFocusManager = worldFocusManager;
            _inputsManager = inputsManager;
            _cubesHolder = cubesHolder;
            _pickingRenderer = pickingRenderer;
            _visualWorldParameters = visualWorldParameters;
            _landscapeManager = landscapeManager;

            Player = player;
            
            ShowDebugInfo = true;

            // Create a visualVoxelEntity (== Assign a voxel body to the PlayerCharacter)
            VisualVoxelEntity = new VisualVoxelEntity(player, voxelModelManager);


            HasMouseFocus = Updatable;
            UpdateOrder = 0;
        }
        
        void InventoryComponentSwitchInventory(object sender, InventorySwitchEventArgs e)
        {
            if (e.Closing && _lockedEntity != null && _lockedEntity is Container)
            {
                _itemMessageTranslator.ReleaseLock();
                _lockedEntity = null;
                _itemMessageTranslator.Container = null;
            }
        }

        void EntityMessageTranslatorEntityLockFailed(object sender, EventArgs e)
        {
            //TODO: inform that player can't use the item he want
            _lockedEntity = null;
        }

        void EntityMessageTranslatorEntityLocked(object sender, EventArgs e)
        {
            // we have the lock, if we can use, we will use
            // if we can't we still use, no one can stop us!!!

            if (_lockedEntity is Container)
            {
                var container = _lockedEntity as Container;
                _itemMessageTranslator.Container = container.Content;
                _inventoryComponent.ShowInventory(container);
            }
            else if (_lockedEntity is IUsableEntity)
            {
                var usable = _lockedEntity as IUsableEntity;
                // send use message to the server
                Player.EntityUse();

                usable.Use();
            }


        }

        public override void BeforeDispose()
        {
            // Clean Up event Delegates
            if (OnLanding != null)
            {
                // Remove all Events associated to this Event (That haven't been unsubscribed !)
                foreach (var d in OnLanding.GetInvocationList())
                {
                    OnLanding -= (LandingGround)d;
                }
            }

        }

        #region Public Methods
        public override void Initialize()
        {
            // Compute the Eye position into the entity
            _entityEyeOffset = new Vector3(0, Player.DefaultSize.Y / 100 * 80, 0);

            // Set Position
            // Set the entity world position following the position received from server
            _worldPosition = Player.Position;

            // Compute the initial Player world bounding box
            VisualVoxelEntity.RefreshWorldBoundingBox(ref _worldPosition);

            // Init Velret physic simulator
            _physicSimu = new VerletSimulator(ref VisualVoxelEntity.LocalBBox) { WithCollisionBouncing = false };
            _physicSimu.ConstraintFct += EntityPickingManager.isCollidingWithEntity; //Check against entities first
            _physicSimu.ConstraintFct += WorldChunks.isCollidingWithTerrain;         //Landscape cheking after

            _entityRotations = new EntityRotations(_inputsManager, _physicSimu);
            _entityRotations.EntityRotationSpeed = Player.RotationSpeed;
            _entityRotations.SetOrientation(Player.HeadRotation, _worldPosition + _entityEyeOffset);

            // Set displacement mode
            DisplacementMode = Player.DisplacementMode;
        }

        /// <summary>
        /// The allocated object here must be disposed
        /// </summary>
        public override void LoadContent(DeviceContext context)
        {

        }

        public override void UnloadContent()
        {
            DisableComponent();
            IsInitialized = false;
        }

        public override void FTSUpdate( GameTime timeSpend)
        {
            if (_landscapeInitiazed == false) return;

            // Input handling
            inputHandler();

            // Picking
            GetSelectedEntity();

            // Refresh player Movement + rotation
            UpdateEntityMovementAndRotation(ref timeSpend);   
            CheckAfterNewPosition();

            // Refresh the player Bounding box
            VisualVoxelEntity.RefreshWorldBoundingBox(ref _worldPosition);
            

        }


        public override void VTSUpdate(double interpolationHd, float interpolationLd, long timePassed)
        {
            CheckHeadUnderWater();      //Under water head test
        }

        #endregion

        // Debug Info interface
        public bool ShowDebugInfo { get; set; }
        
        public string GetDebugInfo()
        {
            return string.Format("Player {0} Pos: [{1:000}; {2:000}; {3:000}] PickedBlock: {4}; NewBlockPlace: {5}", Player.CharacterName,
                                                                                  Math.Round(Player.Position.X, 1),
                                                                                  Math.Round(Player.Position.Y, 1),
                                                                                  Math.Round(Player.Position.Z, 1),
                                                                                  Player.EntityState.IsBlockPicked ? Player.EntityState.PickedBlockPosition.ToString() : "None",
                                                                                  Player.EntityState.IsBlockPicked ? Player.EntityState.NewBlockPosition.ToString() : "None"
                                                                                  );            
        }
    }
}
