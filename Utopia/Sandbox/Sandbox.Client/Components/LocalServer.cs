using System;
using System.IO;
using S33M3CoreComponents.Config;
using S33M3Resources.Structs;
using Utopia.Server;
using Utopia.Server.Managers;
using Utopia.Server.Services;
using Utopia.Shared.ClassExt;
using Utopia.Shared.Configuration;
using Utopia.Shared.Entities;
using Utopia.Shared.Entities.Concrete;
using Utopia.Shared.Entities.Dynamic;
using Utopia.Shared.Entities.Interfaces;
using Utopia.Shared.Entities.Inventory;
using Utopia.Shared.Settings;
using Utopia.Shared.Structs;
using Utopia.Shared.World;
using System.Linq;
using Utopia.Shared.World.Processors.Utopia;
using Utopia.Shared.Interfaces;
using Utopia.Shared.World.Processors;

namespace Sandbox.Client.Components
{
    /// <summary>
    /// Server wrapper for single player
    /// </summary>
    public class LocalServer : IDisposable
    {
        private readonly RuntimeVariables _vars;
        private Server _server;
        private EntityFactory _serverFactory;
        private SQLiteStorageManager _serverSqliteStorageSinglePlayer;
        private WorldParameters _worldParam;

        public LocalServer(RuntimeVariables vars)
        {
            _vars = vars;
        }

        public bool IsDisposed
        {
            get { return _server == null; }
        }

        public void InitSinglePlayerServer(WorldParameters worldParam)
        {
            if (_server != null)
                throw new InvalidOperationException("Already initialized");

            _worldParam = worldParam;

            _serverFactory = new EntityFactory(null);
            _serverFactory.Config = _worldParam.Configuration;
            var dbPath = Path.Combine(_vars.ApplicationDataPath, "Server", "Singleplayer", _worldParam.WorldName, "ServerWorld.db");

            _serverSqliteStorageSinglePlayer = new SQLiteStorageManager(dbPath, _serverFactory, _worldParam);
            _serverSqliteStorageSinglePlayer.Register("local", "qwe123".GetSHA1Hash(), UserRole.Administrator);

            var settings = new XmlSettingsManager<ServerSettings>(@"Server\localServer.config");
            settings.Load();
            settings.Save();

            IWorldProcessor processor = null;
            switch (worldParam.Configuration.WorldProcessor)
            {
                case WorldConfiguration.WorldProcessors.Flat:
                    processor = new FlatWorldProcessor();
                    break;
                case WorldConfiguration.WorldProcessors.Utopia:
                    processor = new UtopiaProcessor(worldParam, _serverFactory, new Utopia.Shared.LandscapeEntities.LandscapeEntityManager());
                    break;
                default:
                    break;
            }

            var worldGenerator = new WorldGenerator(worldParam, processor);
           
            settings.Settings.ChunksCountLimit = 1024 * 3; // better use viewRange * viewRange * 3

            _server = new Server(settings, worldGenerator, _serverSqliteStorageSinglePlayer, _serverSqliteStorageSinglePlayer, _serverSqliteStorageSinglePlayer, _serverFactory, worldParam);
            _serverFactory.LandscapeManager = _server.LandscapeManager;
            _server.ConnectionManager.LocalMode = true;
            _server.ConnectionManager.Listen();
            _server.LoginManager.PlayerEntityNeeded += LoginManagerPlayerEntityNeeded;
            _server.LoginManager.GenerationParameters = default(Utopia.Shared.World.PlanGenerator.GenerationParameters); // planProcessor.WorldPlan.Parameters;
            _server.Clock.SetCurrentTimeOfDay(TimeSpan.FromHours(12));
            //_server.Services.Add(new WaterDynamicService());
        }



        void LoginManagerPlayerEntityNeeded(object sender, NewPlayerEntityNeededEventArgs e)
        {
            var dEntity = new PlayerCharacter();
            dEntity.DynamicId = e.EntityId;
            dEntity.DisplacementMode = EntityDisplacementModes.Walking;
            dEntity.Position = _server.LandscapeManager.GetHighestPoint(new Vector3D(10, 0, 10));
            dEntity.CharacterName = "Local player";
            
            //Put each cube in the inventory
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