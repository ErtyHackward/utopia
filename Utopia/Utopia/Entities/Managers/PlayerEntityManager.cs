using System;
using System.Security.Cryptography;
using SharpDX;
using Utopia.Entities.Managers.Interfaces;
using Utopia.Entities.Renderer;
using Utopia.Entities.Voxel;
using Utopia.GUI;
using Utopia.GUI.Inventory;
using Utopia.Network;
using Utopia.Shared.Chunks;
using Utopia.Shared.Entities;
using Utopia.Shared.Entities.Concrete;
using Utopia.Shared.Entities.Dynamic;
using Utopia.Shared.Entities.Interfaces;
using Utopia.Shared.Entities.Inventory;
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
using S33M3CoreComponents.Cameras;
using S33M3CoreComponents.Inputs;
using S33M3CoreComponents.Physics.Verlet;
using SharpDX.Direct3D11;
using S33M3DXEngine.Debug.Interfaces;
using Utopia.Entities.EntityMovement;
using Utopia.Shared.World;
using Utopia.PostEffects;
using S33M3CoreComponents.GUI;
using S33M3CoreComponents.Sound;
using Utopia.Particules;
using S33M3CoreComponents.Timers;

namespace Utopia.Entities.Managers
{
    /// <summary>
    /// Character entity manager. Allows to control a single character in the world.
    /// Responsible for:
    /// 1) current player physics interaction with the world. 
    /// 2) player movement input handling
    /// 3) picking of the block
    /// </summary>
    public partial class PlayerEntityManager : GameComponent, IPlayerManager, IVisualVoxelEntityContainer, IDebugInfo
    {
        private static readonly NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        private bool _putMode;
        
        private IEntity _lockedEntity;

        #region Private variables
        // Engine System variables
        private CameraManager<ICameraFocused> _cameraManager;
        private InputsManager _inputsManager;
        private SingleArrayChunkContainer _cubesHolder;
        private LandscapeBufferManager _bufferManager;
        private readonly ILandscapeManager _landscapeManager;
        private readonly ChatComponent _chatComponent;
        private readonly PostEffectComponent _postEffectComponent;
        private ISoundEngine _soundEngine;
        private TimerManager.GameTimer _energyUpdateTimer;
        private Random random;

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
        private float _moveDelta;
        private IEntityPickingManager _entityPickingManager;
        private bool _stopMovedAction = false;

        private VisualWorldParameters _visualWorldParameters;
        private readonly EntityFactory _factory;
        private ItemMessageTranslator _itemMessageTranslator;

        // Event related variables
        private double _fallMaxHeight;

        private TerraCubeWithPosition _groundCube;
        private BlockProfile _groundCubeProgile;

        // Will be used to compute entity rotation movements
        private EntityRotations _entityRotations;
        private InventoryComponent _inventoryComponent;

        private Faction _faction;
        private PlayerCharacter _playerCharacter;
        private GuiManager _guiManager;

        #endregion

        #region Public variables/properties

        public EntityRotations EntityRotations
        {
            get { return _entityRotations; }
            set { _entityRotations = value; }
        }

        /// <summary>
        /// The Player
        /// </summary>
        public PlayerCharacter PlayerCharacter
        {
            get { return _playerCharacter; }
            set {
                if (_playerCharacter != value)
                {
                    var ea = new PlayerEntityChangedEventArgs();

                    if (_playerCharacter != null)
                    {
                        ea.PreviousCharacter = _playerCharacter;
                        _playerCharacter.Equipment.ItemEquipped -= Equipment_ItemEquipped;
                        _playerCharacter.HealthStateChanged -= playerCharacter_HealthStateChanged;
                        _playerCharacter.HealthChanged -= _playerCharacter_HealthChanged;
                        _playerCharacter.DisplacementModeChanged -= _playerCharacter_DisplacementModeChanged;

                    }
                    _playerCharacter = value;

                    if (_playerCharacter != null)
                    {
                        ea.PlayerCharacter = _playerCharacter;
                        _playerCharacter.Equipment.ItemEquipped += Equipment_ItemEquipped;
                        _playerCharacter.HealthStateChanged += playerCharacter_HealthStateChanged;
                        _playerCharacter.HealthChanged += _playerCharacter_HealthChanged;
                        _playerCharacter.DisplacementModeChanged += _playerCharacter_DisplacementModeChanged;


                        var rightTool = _playerCharacter.Equipment.RightTool;
                        PutMode = !(rightTool is ITool);
                    }

                    OnPlayerEntityChanged(ea);
                }
            }
        }

        public ICharacterEntity Player { get { return PlayerCharacter; } }

        public Faction Faction { get { return _faction; } }

        /// <summary>
        /// Gets active player tool or null
        /// </summary>
        public IItem ActiveTool { get { return PlayerCharacter.Equipment.RightTool ?? PlayerCharacter.HandTool; } }

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

        public bool HasMouseFocus { get; set; }
        
        public bool MousepickDisabled { get; set; }
        
        public SingleArrayChunkContainer CubesHolder
        {
            get { return _cubesHolder; }
        }

        /// <summary>
        /// if put mode is active Put method will be executed instead of use
        /// and ghosted item will appear in the world poining the future place 
        /// of the item
        /// </summary>
        public bool PutMode
        {
            get { return _putMode; }
            set
            {
                _putMode = value;
                if (GhostedEntityRenderer != null)
                    GhostedEntityRenderer.Display = _putMode;
            }
        }

        /// <summary>
        /// Contains current locked container or null
        /// </summary>
        public Container LockedContainer { get; set; }

        public UtopiaParticuleEngine UtopiaParticuleEngine { get; set; }

        #endregion

        #region Dependenices
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
            set
            {
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
            set
            {
                _inventoryComponent = value;
                _inventoryComponent.SwitchInventory += InventoryComponentSwitchInventory;
            }
        }

        [Inject]
        public GhostedEntityRenderer GhostedEntityRenderer { get; set; }

        [Inject]
        public IWorldChunks WorldChunks { get; set; }

        [Inject]
        public IPickingRenderer PickingRenderer { get; set; }

        #endregion

        #region Events

        public delegate void LandingGround(double fallHeight, TerraCubeWithPosition landedCube);
        public event LandingGround OnLanding;

        /// <summary>
        /// Occurs when the inventory screen should be shown
        /// Indicates that we have the lock and can perform transfer operations with container
        /// </summary>
        public event EventHandler<InventoryEventArgs> NeedToShowInventory;

        protected virtual void OnNeedToShowInventory(InventoryEventArgs e)
        {
            var handler = NeedToShowInventory;
            if (handler != null) handler(this, e);
        }

        public event EventHandler<PlayerEntityChangedEventArgs> PlayerEntityChanged;

        protected virtual void OnPlayerEntityChanged(PlayerEntityChangedEventArgs e)
        {
            var handler = PlayerEntityChanged;
            if (handler != null) handler(this, e);
        }

        #endregion

        public PlayerEntityManager(CameraManager<ICameraFocused> cameraManager,
                                   InputsManager inputsManager,
                                   SingleArrayChunkContainer cubesHolder,
                                   ServerComponent server,
                                   VoxelModelManager voxelModelManager,
                                   VisualWorldParameters visualWorldParameters,
                                   EntityFactory factory,
                                   LandscapeBufferManager bufferManager,
                                   ILandscapeManager landscapeManager,
                                   ChatComponent chatComponent,
                                   PostEffectComponent postEffectComponent,
                                   GuiManager guiManager,
                                   ISoundEngine soundEngine,
                                   TimerManager timerManager
            )
        {

            _cameraManager = cameraManager;
            _inputsManager = inputsManager;
            _soundEngine = soundEngine;
            _cubesHolder = cubesHolder;
            _visualWorldParameters = visualWorldParameters;
            _factory = factory;
            _bufferManager = bufferManager;
            _landscapeManager = landscapeManager;
            _chatComponent = chatComponent;
            _postEffectComponent = postEffectComponent;
            OnLanding += PlayerEntityManager_OnLanding;
            _guiManager = guiManager;

            PlayerCharacter = (PlayerCharacter)server.Player;
            
            ShowDebugInfo = true;

            // Create a visualVoxelEntity (== Assign a voxel body to the PlayerCharacter)
            VisualVoxelEntity = new VisualVoxelEntity(PlayerCharacter, null, voxelModelManager);

            //Add a new Timer trigger
            _energyUpdateTimer = timerManager.AddTimer(1000); //A timer that will be raised every second
            _energyUpdateTimer.OnTimerRaised += energyUpdateTimer_OnTimerRaised;

            HasMouseFocus = Updatable;
            UpdateOrder = 0;
            
            // create "real" random
            var entropySource = RNGCryptoServiceProvider.Create();
            var bytes = new byte[4];
            entropySource.GetBytes(bytes);
            random = new Random(BitConverter.ToInt32(bytes,0));
        }

        void Equipment_ItemEquipped(object sender, CharacterEquipmentEventArgs e)
        {
            if (e.Slot == EquipmentSlotType.Hand && e.EquippedItem != null)
            {
                var item = e.EquippedItem.Item;

                if (item != null)
                {
                    PutMode = !(item is ITool);
                }
                else PutMode = false;
            }
            else
            {
                PutMode = false;
            }
        }

        private void _playerCharacter_DisplacementModeChanged(object sender, Shared.Entities.Events.EntityDisplacementModeEventArgs e)
        {
            _entityRotations.SetDisplacementMode(e.CurrentDisplacement, _worldPosition + _entityEyeOffset);
#if DEBUG
            logger.Info("{0} is now {1}", PlayerCharacter.CharacterName, e.CurrentDisplacement.ToString());
#endif
            if (e.CurrentDisplacement == EntityDisplacementModes.Walking || e.CurrentDisplacement == EntityDisplacementModes.Swiming)
            {
                _fallMaxHeight = double.MinValue;
                _physicSimu.StartSimulation(_worldPosition);
                _physicSimu.ConstraintOnlyMode = false;
            }
            else if (e.CurrentDisplacement == EntityDisplacementModes.Dead)
            {
                _physicSimu.StartSimulation(ref _worldPosition, ref _worldPosition);
                _physicSimu.ConstraintOnlyMode = true;
            }
            //Collision detection not activated oustide debug mode when flying !
#if !DEBUG
                else if (e.CurrentDisplacement == EntityDisplacementModes.Flying)
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
        
        void InventoryComponentSwitchInventory(object sender, InventorySwitchEventArgs e)
        {
            if (e.Closing && _lockedEntity is Container)
            {
                _itemMessageTranslator.ReleaseLock();
                _lockedEntity = null;
                _itemMessageTranslator.Container = null;
                LockedContainer = null;
            }
        }

        void EntityMessageTranslatorEntityLockFailed(object sender, EventArgs e)
        {
            _chatComponent.AddMessage(" -- Can't use the entity, it is busy!");
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
                LockedContainer = container;
                OnNeedToShowInventory(new InventoryEventArgs { Container = container });
            }
            else
            {
                PlayerCharacter.EntityState.IsEntityPicked = true;
                PlayerCharacter.EntityState.IsBlockPicked = false;
                PlayerCharacter.EntityState.PickedEntityLink = _lockedEntity.GetLink();
                PlayerCharacter.HandUse();
            }
        }

        public override void BeforeDispose()
        {
            OnLanding -= PlayerEntityManager_OnLanding;
            _energyUpdateTimer.OnTimerRaised -= energyUpdateTimer_OnTimerRaised;

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
            _physicSimu.ConstraintFct += EntityPickingManager.isCollidingWithEntity;       //Check against entities first
            _physicSimu.ConstraintFct += _landscapeManager.IsCollidingWithTerrain;         //Landscape checking after

            _entityRotations = new EntityRotations(_inputsManager, _physicSimu);
            _entityRotations.EntityRotationSpeed = Player.RotationSpeed;
            _entityRotations.SetOrientation(Player.HeadRotation, _worldPosition + _entityEyeOffset);
            
            // Set displacement mode
            _playerCharacter.DisplacementMode = Player.DisplacementMode;
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
            EnergyFTSUpdate(timeSpend);

            // wait until landscape being loaded
            if (!WorldChunks.IsInitialLoadCompleted) 
                return;

            Player.EntityState.Entropy = random.Next();

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

        protected override void OnUpdatableChanged(object sender, EventArgs args)
        {
            if (Updatable)
            {
                GhostedEntityRenderer.Display = _putMode;
            }

            base.OnUpdatableChanged(sender, args);
        }

        public override void VTSUpdate(double interpolationHd, float interpolationLd, float elapsedTime)
        {
            CheckHeadUnderWater();      //Under water head test
        }

        #endregion

        // Debug Info interface
        public bool ShowDebugInfo { get; set; }
        
        public string GetDebugInfo()
        {
            var chunk = _landscapeManager.GetChunkFromBlock(new Vector3I(Player.Position.X, Player.Position.Y, Player.Position.Z));
            return string.Format("Player {0} Pos: [{1:000}; {2:000}; {3:000}] Chunk : {4}", PlayerCharacter.CharacterName,
                                                                                  Math.Round(Player.Position.X, 1),
                                                                                  Math.Round(Player.Position.Y, 1),
                                                                                  Math.Round(Player.Position.Z, 1),
                                                                                  chunk == null ?"" : chunk.Position.ToString() 
                                                                                  );            
        }
    }

    public class PlayerEntityChangedEventArgs : EventArgs
    {
        public PlayerCharacter PlayerCharacter { get; set; }
        public PlayerCharacter PreviousCharacter { get; set; }
    }

    public class InventoryEventArgs : EventArgs
    {
        public Container Container { get; set; }
    }
}
