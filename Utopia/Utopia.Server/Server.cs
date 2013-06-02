using System;
using Utopia.Server.Interfaces;
using Utopia.Server.Managers;
using Utopia.Server.Utils;
using Utopia.Shared.Entities;
using Utopia.Shared.Entities.Interfaces;
using Utopia.Shared.Interfaces;
using Utopia.Shared.Services;
using Utopia.Shared.Services.Interfaces;
using Utopia.Shared.Structs;
using Utopia.Shared.World;
using S33M3CoreComponents.Config;

namespace Utopia.Server
{
    /// <summary>
    /// Main Utopia server class
    /// </summary>
    public class Server : IServer
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
        IAreaManager IServer.AreaManager { get { return AreaManager; } }

        /// <summary>
        /// Gets server game services
        /// </summary>
        public ServiceManager Services { get; private set; }

        /// <summary>
        /// Gets landscape manager
        /// </summary>
        public ServerLandscapeManager LandscapeManager { get; private set; }
        IServerLandscapeManager IServer.LandscapeManager { get { return LandscapeManager; } }

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
        ICommandsManager IServer.CommandsManager { get { return CommandsManager; } }

        /// <summary>
        /// Gets chat manager
        /// </summary>
        public ChatManager ChatManager { get; private set; }
        IChatManager IServer.ChatManager { get { return ChatManager; } }

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
        /// Gets Weather and time Manager
        /// </summary>
        public WeatherManager Weather { get; private set; }

        /// <summary>
        /// Contains global gameplay variables, like factions list
        /// </summary>
        public GlobalStateManager GlobalStateManager { get; private set; }
        IGlobalStateManager IServer.GlobalStateManager { get { return GlobalStateManager; } }

        public WorldParameters WorldParameters { get; private set; }

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
            EntityFactory entityFactory,
            WorldParameters wp
            )
        {
            // dependency injection
            SettingsManager = settingsManager;
            UsersStorage = usersStorage;
            EntityStorage = entityStorage;
            EntityFactory = entityFactory;
            WorldParameters = wp;

            if (SettingsManager.Settings == null)
                SettingsManager.Load();

            var settings = SettingsManager.Settings;

            ConnectionManager = new ConnectionManager(SettingsManager.Settings.ServerPort);

            Clock = new Clock(DateTime.Now, TimeSpan.FromMinutes(20));

            Scheduler = new ScheduleManager(Clock);

            LandscapeManager = new ServerLandscapeManager(this, chunksStorage, worldGenerator, EntityFactory, settings.ChunkLiveTimeMinutes, settings.CleanUpInterval, settings.SaveInterval, settings.ChunksCountLimit, wp);

            EntityManager = new EntityManager(this);

            AreaManager = new AreaManager(this);

            DynamicIdHelper.SetMaxExistsId(EntityStorage.GetMaximumId());
            
            Services = new ServiceManager(this);
            
            PerformanceManager = new PerformanceManager(AreaManager);

            CommandsManager = new CommandsManager(this);

            ChatManager = new ChatManager(this);

            Weather = new WeatherManager(this);

            GlobalStateManager = new GlobalStateManager(this);
            
            LoginManager = new LoginManager(this, EntityFactory);

            Services.Initialize();
        }

        /// <summary>
        /// Stops the server and releases all related resources
        /// </summary>
        public void Dispose()
        {
            AreaManager.Dispose();
            ConnectionManager.Dispose();
            LandscapeManager.Dispose();
            Services.Dispose();
            GlobalStateManager.Dispose();
            Scheduler.Dispose();
        }
    }
}
