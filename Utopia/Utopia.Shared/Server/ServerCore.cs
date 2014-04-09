using System;
using S33M3CoreComponents.Config;
using Utopia.Shared.Entities;
using Utopia.Shared.Interfaces;
using Utopia.Shared.Server.Interfaces;
using Utopia.Shared.Server.Managers;
using Utopia.Shared.Server.Utils;
using Utopia.Shared.Structs;
using Utopia.Shared.World;

namespace Utopia.Shared.Server
{
    /// <summary>
    /// Main Utopia server class
    /// </summary>
    public class ServerCore
    {
       
        #region Properties
        /// <summary>
        /// Gets server settings manager
        /// </summary>
        public XmlSettingsManager<ServerSettings> SettingsManager { get; private set; }

        /// <summary>
        /// Gets server connection manager
        /// </summary>
        public ConnectionManager ConnectionManager { get; private set; }

        /// <summary>
        /// Gets main users storage
        /// </summary>
        public IUsersStorage UsersStorage { get; private set; }

        /// <summary>
        /// Gets main entity storage
        /// </summary>
        public IEntityStorage EntityStorage { get; private set; }
        
        /// <summary>
        /// Provides storage for various data (key value pairs)
        /// </summary>
        public ICustomStorage CustomStorage { get; private set; }

        /// <summary>
        /// Gets entity manager
        /// </summary>
        public AreaManager AreaManager { get; private set; }

        /// <summary>
        /// Gets server game services
        /// </summary>
        public ServiceManager Services { get; private set; }

        /// <summary>
        /// Gets landscape manager
        /// </summary>
        public ServerLandscapeManager LandscapeManager { get; private set; }

        /// <summary>
        /// Gets schedule manager for dalayed and periodic operations.
        /// </summary>
        public ScheduleManager Scheduler { get; private set; }

        /// <summary>
        /// Gets server clock
        /// </summary>
        public Clock Clock { get; private set; }

        /// <summary>
        /// Gets perfomance manager
        /// </summary>
        public PerformanceManager PerformanceManager { get; private set; }

        /// <summary>
        /// Gets command processor
        /// </summary>
        public CommandsManager CommandsManager { get; private set; }

        /// <summary>
        /// Gets chat manager
        /// </summary>
        public ChatManager ChatManager { get; private set; }

        /// <summary>
        /// Gets entity manager
        /// </summary>
        public EntityManager EntityManager { get; private set; }

        /// <summary>
        /// Gets login manager
        /// </summary>
        public LoginManager LoginManager { get; private set; }

        /// <summary>
        /// Gets or sets an entity factory
        /// </summary>
        public EntityFactory EntityFactory { get; private set; }

        /// <summary>
        /// Contains global gameplay variables, like factions list
        /// </summary>
        public GlobalStateManager GlobalStateManager { get; private set; }

        /// <summary>
        /// Contains the logic behind chunk's entities spawning.
        /// </summary>
        public EntitySpawningManager EntitySpawningManager { get; private set; }

        public WorldParameters WorldParameters { get; private set; }

        #endregion

        /// <summary>
        /// Create new instance of the Server class
        /// </summary>
        public ServerCore(
            XmlSettingsManager<ServerSettings> settingsManager,
            WorldGenerator worldGenerator,
            IUsersStorage usersStorage,
            IChunksStorage chunksStorage,
            IEntityStorage entityStorage,
            ICustomStorage customStorage,
            EntityFactory entityFactory,
            WorldParameters wp
            )
        {
            // dependency injection
            SettingsManager = settingsManager;
            UsersStorage = usersStorage;
            EntityStorage = entityStorage;
            CustomStorage = customStorage;
            EntityFactory = entityFactory;
            WorldParameters = wp;

            if (SettingsManager.Settings == null)
                SettingsManager.Load();

            var settings = SettingsManager.Settings;

            ConnectionManager = new ConnectionManager(SettingsManager.Settings.ServerPort);

            Scheduler = new ScheduleManager();

            UtopiaTime startTime = new UtopiaTime() { TotalSeconds = CustomStorage.GetVariable<long>("GameTimeElapsedSeconds", 0) };
            Clock = new Clock(this, startTime, TimeSpan.FromMinutes(20));

            LandscapeManager = new ServerLandscapeManager(
                this, 
                chunksStorage, 
                worldGenerator, 
                EntityFactory, 
                settings.ChunkLiveTimeMinutes, 
                settings.CleanUpInterval, 
                settings.SaveInterval, 
                settings.ChunksCountLimit, 
                wp);

            EntityManager = new EntityManager(this);

            AreaManager = new AreaManager(this);

            DynamicIdHelper.SetMaxExistsId(EntityStorage.GetMaximumId());
            
            Services = new ServiceManager(this);
            
            PerformanceManager = new PerformanceManager(AreaManager);

            CommandsManager = new CommandsManager(this);

            ChatManager = new ChatManager(this);

            GlobalStateManager = new GlobalStateManager(this);
            
            LoginManager = new LoginManager(this, EntityFactory);

            EntitySpawningManager = new EntitySpawningManager(this, worldGenerator.EntitySpawningControler);

            Services.Initialize();
        }

        /// <summary>
        /// Stops the server and releases all related resources
        /// </summary>
        public void Dispose()
        {
            Clock.Dispose();
            AreaManager.Dispose();
            ConnectionManager.Dispose();
            LandscapeManager.Dispose();
            Services.Dispose();
            GlobalStateManager.Dispose();
            Scheduler.Dispose();
        }
    }
}
