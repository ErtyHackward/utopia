using System;
using Utopia.Server.Interfaces;
using Utopia.Server.Managers;
using Utopia.Server.Services;
using Utopia.Server.Utils;
using Utopia.Shared.Entities;
using Utopia.Shared.Interfaces;
using Utopia.Shared.World;
using S33M3CoreComponents.Config;

namespace Utopia.Server
{
    /// <summary>
    /// Main Utopia server class
    /// </summary>
    public class Server : IDisposable
    {
        /// <summary>
        /// Modify this constant to actual value
        /// </summary>
        public const int ServerProtocolVersion = 1;
        
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

        #endregion

        /// <summary>
        /// Create new instance of the Server class
        /// </summary>
        public Server(
            XmlSettingsManager<ServerSettings> settingsManager,
            WorldGenerator worldGenerator,
            IUsersStorage usersStorage,
            IChunksStorage chunksStorage,
            IEntityStorage entityStorage,
            EntityFactory entityFactory
            )
        {
            // dependency injection
            SettingsManager = settingsManager;
            UsersStorage = usersStorage;
            EntityStorage = entityStorage;
            EntityFactory = entityFactory;

            if (SettingsManager.Settings == null)
                SettingsManager.Load();

            var settings = SettingsManager.Settings;


            Clock = new Clock(DateTime.Now, TimeSpan.FromMinutes(20));

            ConnectionManager = new ConnectionManager(SettingsManager.Settings.ServerPort);

            Scheduler = new ScheduleManager(Clock);

            LandscapeManager = new ServerLandscapeManager(this, chunksStorage, worldGenerator, EntityFactory, settings.ChunkLiveTimeMinutes, settings.CleanUpInterval, settings.SaveInterval, settings.ChunksCountLimit);
            
            AreaManager = new AreaManager(this);

            DynamicIdHelper.SetMaxExistsId(EntityStorage.GetMaximumId());
            
            Services = new ServiceManager(this);
            
            PerformanceManager = new PerformanceManager(AreaManager);

            CommandsManager = new CommandsManager(this);

            ChatManager = new ChatManager(this);

            EntityManager = new EntityManager(this);
            
            LoginManager = new LoginManager(this, EntityFactory);
        }

        /// <summary>
        /// Stops the server and releases all related resources
        /// </summary>
        public void Dispose()
        {
            ConnectionManager.Dispose();
            LandscapeManager.Dispose();
        }
    }
}
