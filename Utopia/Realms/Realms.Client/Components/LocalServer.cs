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
using Utopia.Shared.Server;
using Utopia.Shared.Server.Managers;
using Utopia.Shared.Services;
using Utopia.Shared.Structs;
using Utopia.Shared.World;
using Utopia.Shared.World.Processors.Utopia;
using Utopia.Shared.Interfaces;
using Utopia.Shared.World.Processors;

namespace Realms.Client.Components
{
    /// <summary>
    /// Server wrapper for single player
    /// </summary>
    public class LocalServer : IDisposable
    {
        private readonly RealmRuntimeVariables _vars;
        private ServerCore _server;
        private EntityFactory _serverFactory;
        private SqliteStorageManager _serverSqliteStorageSinglePlayer;
        private WorldParameters _worldParam;
        private LandscapeBufferManager _landscapeEntityManager;

        public bool IsDisposed
        {
            get { return _server == null; }
        }


        public LocalServer(RealmRuntimeVariables vars, LandscapeBufferManager landscapeEntityManager)
        {
            _vars = vars;
            _landscapeEntityManager = landscapeEntityManager;
        }

        public void InitSinglePlayerServer(WorldParameters worldParam)
        {
            if (_server != null)
                throw new InvalidOperationException("Already initialized");

            _worldParam = worldParam;

            _serverFactory = new EntityFactory();
            _serverFactory.Config = _worldParam.Configuration;
            var dbPath = Path.Combine(_vars.ApplicationDataPath, "Server", "Singleplayer", worldParam.WorldName, "ServerWorld.db");

            _serverSqliteStorageSinglePlayer = new SqliteStorageManager(dbPath, _serverFactory, worldParam);
            _serverSqliteStorageSinglePlayer.Register("local", "qwe123".GetSHA1Hash(), UserRole.Administrator);

            var settings = new XmlSettingsManager<ServerSettings>(@"Server\localServer.config");
            settings.Load();
            settings.Save();

            //Utopia New Landscape Test

            IWorldProcessor processor = null;
            switch (worldParam.Configuration.WorldProcessor)
            {
                case WorldConfiguration.WorldProcessors.Flat:
                    processor = new FlatWorldProcessor();
                    break;
                case WorldConfiguration.WorldProcessors.Utopia:
                    processor = new UtopiaProcessor(worldParam, _serverFactory, _landscapeEntityManager);
                    break;
                default:
                    break;
            }

            var worldGenerator = new WorldGenerator(worldParam, processor);

            //Old s33m3 landscape
            //IWorldProcessor processor1 = new s33m3WorldProcessor(worldParam);
            //IWorldProcessor processor2 = new LandscapeLayersProcessor(worldParam, _serverFactory);
            //var worldGenerator = new WorldGenerator(worldParam, processor1, processor2);

            //Vlad Generator
            //var planProcessor = new PlanWorldProcessor(wp, _serverFactory);
            //var worldGenerator = new WorldGenerator(wp, planProcessor);
            settings.Settings.ChunksCountLimit = 1024 * 3; // better use viewRange * viewRange * 3

            _server = new ServerCore(settings, worldGenerator, _serverSqliteStorageSinglePlayer, _serverSqliteStorageSinglePlayer, _serverSqliteStorageSinglePlayer, _serverSqliteStorageSinglePlayer, _serverFactory, worldParam);
            _serverFactory.LandscapeManager = _server.LandscapeManager;
            _serverFactory.DynamicEntityManager = _server.AreaManager;
            _serverFactory.GlobalStateManager = _server.GlobalStateManager;
            _serverFactory.ScheduleManager = _server.Scheduler;
            _serverFactory.ServerSide = true;

            _server.ConnectionManager.LocalMode = true;
            _server.ConnectionManager.Listen();
            _server.LoginManager.PlayerEntityNeeded += LoginManagerPlayerEntityNeeded;
            _server.LoginManager.GenerationParameters = default(Utopia.Shared.World.PlanGenerator.GenerationParameters); // planProcessor.WorldPlan.Parameters;
            _server.Clock.SetCurrentTimeOfDay(TimeSpan.FromHours(12));
        }

        void LoginManagerPlayerEntityNeeded(object sender, NewPlayerEntityNeededEventArgs e)
        {
            var dEntity = new PlayerCharacter();
            dEntity.DynamicId = e.EntityId;
            dEntity.DisplacementMode = EntityDisplacementModes.Walking;
            dEntity.Position = _server.LandscapeManager.GetHighestPoint(new Vector3D(10, 0, 10));
            dEntity.CharacterName = "Local player";

            dEntity.Health.MaxValue = 100;
            dEntity.Stamina.MaxValue = 100;
            dEntity.Oxygen.MaxValue = 100;

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
            if (_server != null)
            {
                _server.LoginManager.PlayerEntityNeeded -= LoginManagerPlayerEntityNeeded;

                _server.Dispose();
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