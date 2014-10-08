using System;
using System.IO;
using System.Linq;
using S33M3CoreComponents.Config;
using S33M3Resources.Structs;
using SharpDX;
using Utopia.Shared.ClassExt;
using Utopia.Shared.Configuration;
using Utopia.Shared.Entities;
using Utopia.Shared.Entities.Dynamic;
using Utopia.Shared.Net.Connections;
using Utopia.Shared.Server;
using Utopia.Shared.Server.Managers;
using Utopia.Shared.Services;
using Utopia.Shared.Structs;
using Utopia.Shared.World;
using Utopia.Shared.World.Processors.Utopia;
using Utopia.Shared.Interfaces;
using Utopia.Shared.World.Processors;
using Utopia.Shared.Chunks;

namespace Realms.Client.Components
{
    /// <summary>
    /// Server wrapper for single player
    /// </summary>
    public class LocalServer : IDisposable
    {
        private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        private readonly RealmRuntimeVariables _vars;
        private ServerCore _server;
        private EntityFactory _serverFactory;
        private SqliteStorageManager _serverSqliteStorageSinglePlayer;
        private WorldParameters _worldParam;
        private LandscapeBufferManager _landscapeEntityManager;

        public bool IsDisposed
        {
            get { return Server == null; }
        }

        public ServerCore Server
        {
            get { return _server; }
        }


        public LocalServer(RealmRuntimeVariables vars, LandscapeBufferManager landscapeEntityManager)
        {
            _vars = vars;
            _landscapeEntityManager = landscapeEntityManager;
        }

        public void InitSinglePlayerServer(WorldParameters worldParam)
        {
            if (Server != null)
                throw new InvalidOperationException("Already initialized");

            _worldParam = worldParam;

            _serverFactory = new EntityFactory();
            _serverFactory.Config = _worldParam.Configuration;
            var dbPath = Path.Combine(_vars.ApplicationDataPath, "Server", "Singleplayer", worldParam.WorldName, "ServerWorld.db");

            logger.Info("Local world db path is {0}", dbPath);

            _serverSqliteStorageSinglePlayer = new SqliteStorageManager(dbPath, _serverFactory, worldParam);
            _serverSqliteStorageSinglePlayer.Register("local", "qwe123".GetSHA1Hash(), UserRole.Administrator);

            var settings = new XmlSettingsManager<ServerSettings>(@"Server\localServer.config");
            settings.Load();
            settings.Save();

            //Utopia New Landscape Test

            IWorldProcessor processor = null;
            IEntitySpawningControler entitySpawningControler = null;
            switch (worldParam.Configuration.WorldProcessor)
            {
                case WorldConfiguration.WorldProcessors.Flat:
                    processor = new FlatWorldProcessor();
                    break;
                case WorldConfiguration.WorldProcessors.Utopia:
                    processor = new UtopiaProcessor(worldParam, _serverFactory, _landscapeEntityManager);
                    entitySpawningControler = new UtopiaEntitySpawningControler((UtopiaWorldConfiguration)worldParam.Configuration);
                    break;
                default:
                    break;
            }

            var worldGenerator = new WorldGenerator(worldParam, processor);
            worldGenerator.EntitySpawningControler = entitySpawningControler;

            //Old s33m3 landscape
            //IWorldProcessor processor1 = new s33m3WorldProcessor(worldParam);
            //IWorldProcessor processor2 = new LandscapeLayersProcessor(worldParam, _serverFactory);
            //var worldGenerator = new WorldGenerator(worldParam, processor1, processor2);

            //Vlad Generator
            //var planProcessor = new PlanWorldProcessor(wp, _serverFactory);
            //var worldGenerator = new WorldGenerator(wp, planProcessor);
            settings.Settings.ChunksCountLimit = 1024 * 3; // better use viewRange * viewRange * 3

            var port = 4815;

            while (!TcpConnectionListener.IsPortFree(port))
            {
                port++;
            }
            settings.Settings.ServerPort = port;

            _server = new ServerCore(settings, worldGenerator, _serverSqliteStorageSinglePlayer, _serverSqliteStorageSinglePlayer, _serverSqliteStorageSinglePlayer, _serverSqliteStorageSinglePlayer, _serverFactory, worldParam);
            _serverFactory.LandscapeManager = Server.LandscapeManager;
            _serverFactory.DynamicEntityManager = Server.AreaManager;
            _serverFactory.GlobalStateManager = Server.GlobalStateManager;
            _serverFactory.ScheduleManager = Server.Scheduler;
            _serverFactory.ServerSide = true;

            _server.Initialize();

            Server.ConnectionManager.LocalMode = true;
            Server.ConnectionManager.Listen();
            Server.LoginManager.PlayerEntityNeeded += LoginManagerPlayerEntityNeeded;
            Server.LoginManager.GenerationParameters = default(Utopia.Shared.World.PlanGenerator.GenerationParameters); // planProcessor.WorldPlan.Parameters;
            Server.Clock.SetCurrentTimeOfDay(UtopiaTimeSpan.FromHours(12));
        }

        void LoginManagerPlayerEntityNeeded(object sender, NewPlayerEntityNeededEventArgs e)
        {
            var dEntity = new PlayerCharacter();
            dEntity.DynamicId = e.EntityId;
            dEntity.DisplacementMode = EntityDisplacementModes.Walking;
            dEntity.Position = Server.LandscapeManager.GetHighestPoint(new Vector3D(10, 0, 10));
            dEntity.CharacterName = "Local player";

            dEntity.Health.MaxValue = 100;
            dEntity.Stamina.MaxValue = 100;
            dEntity.Oxygen.MaxValue = 100;

            dEntity.Health.CurrentValue = 100;
            dEntity.Stamina.CurrentValue = 100;
            dEntity.Oxygen.CurrentValue = 100;


            // give start items to the player
            var startSetName = _worldParam.Configuration.StartSet;
            if (!string.IsNullOrEmpty(startSetName))
            {
                _serverFactory.FillContainer(startSetName, dEntity.Inventory);
            }

            e.PlayerEntity = dEntity;
        }

        public void Dispose()
        {
            if (Server != null)
            {
                Server.LoginManager.PlayerEntityNeeded -= LoginManagerPlayerEntityNeeded;

                Server.Dispose();
                _server = null;
            }
            if (_serverSqliteStorageSinglePlayer != null)
            {
                _serverSqliteStorageSinglePlayer.Dispose();
                _serverSqliteStorageSinglePlayer = null;
            }
        }
    }
}