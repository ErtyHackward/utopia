using System;
using System.IO;
using S33M3CoreComponents.Config;
using S33M3Resources.Structs;
using Utopia.Server;
using Utopia.Server.Managers;
using Utopia.Server.Sample;
using Utopia.Server.Services;
using Utopia.Shared.ClassExt;
using Utopia.Shared.Configuration;
using Utopia.Shared.Entities;
using Utopia.Shared.Entities.Concrete;
using Utopia.Shared.Entities.Concrete.Collectible;
using Utopia.Shared.Entities.Dynamic;
using Utopia.Shared.Entities.Interfaces;
using Utopia.Shared.Entities.Inventory;
using Utopia.Shared.Structs;
using Utopia.Shared.World;
using Utopia.Shared.World.Processors.Utopia;

namespace Realms.Client.Components
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

        public LocalServer(RuntimeVariables vars)
        {
            _vars = vars;
        }

        public void InitSinglePlayerServer(WorldParameters worldParam)
        {
            if (_server != null)
                throw new InvalidOperationException("Already initialized");

            _serverFactory = new EntityFactory(null);
            var dbPath = Path.Combine(_vars.ApplicationDataPath, "Server", "Singleplayer", worldParam.WorldName, "ServerWorld.db");

            _serverSqliteStorageSinglePlayer = new SQLiteStorageManager(dbPath, _serverFactory, worldParam);
            _serverSqliteStorageSinglePlayer.Register("local", "qwe123".GetSHA1Hash(), UserRole.Administrator);

            var settings = new XmlSettingsManager<ServerSettings>(@"Server\localServer.config");
            settings.Load();
            settings.Save();

            //Utopia New Landscape Test
            var utopiaProcessor = new UtopiaProcessor(worldParam, _serverFactory);
            var worldGenerator = new WorldGenerator(worldParam, utopiaProcessor);

            //Old s33m3 landscape
            //IWorldProcessor processor1 = new s33m3WorldProcessor(worldParam);
            //IWorldProcessor processor2 = new LandscapeLayersProcessor(worldParam, _serverFactory);
            //var worldGenerator = new WorldGenerator(worldParam, processor1, processor2);

            //Vlad Generator
            //var planProcessor = new PlanWorldProcessor(wp, _serverFactory);
            //var worldGenerator = new WorldGenerator(wp, planProcessor);
            settings.Settings.ChunksCountLimit = 1024 * 3; // better use viewRange * viewRange * 3

            _server = new Server(settings, worldGenerator, _serverSqliteStorageSinglePlayer, _serverSqliteStorageSinglePlayer, _serverSqliteStorageSinglePlayer, _serverFactory);
            _serverFactory.LandscapeManager = _server.LandscapeManager;
            _server.ConnectionManager.LocalMode = true;
            _server.ConnectionManager.Listen();
            _server.LoginManager.PlayerEntityNeeded += LoginManagerPlayerEntityNeeded;
            _server.LoginManager.GenerationParameters = default(Utopia.Shared.World.PlanGenerator.GenerationParameters); // planProcessor.WorldPlan.Parameters;
            _server.Clock.SetCurrentTimeOfDay(TimeSpan.FromHours(12));
            _server.Services.Add(new WaterDynamicService());
            _server.Services.Add(new TestNpcService());
        }



        void LoginManagerPlayerEntityNeeded(object sender, NewPlayerEntityNeededEventArgs e)
        {
            var dEntity = new PlayerCharacter();
            dEntity.DynamicId = e.EntityId;
            dEntity.DisplacementMode = EntityDisplacementModes.Walking;
            dEntity.Position = _server.LandscapeManager.GetHighestPoint(new Vector3D(10, 0, 10));
            dEntity.CharacterName = "Local player";
            ContainedSlot outItem;

            var adder = _server.EntityFactory.CreateEntity<CubeResource>();
            adder.CubeId = RealmConfiguration.CubeId.DynamicWater;//looting a terraincube will create a new blockadder instance or add to the stack

            dEntity.Equipment.Equip(EquipmentSlotType.Hand, new EquipmentSlot<ITool> { Item = adder }, out outItem);

            foreach (var cubeId in RealmConfiguration.CubeId.All())
            {
                if (cubeId == RealmConfiguration.CubeId.Air)
                    continue;

                var item3 = _server.EntityFactory.CreateEntity<CubeResource>();
                item3.CubeId = cubeId;
                dEntity.Inventory.PutItem(item3);
            }

            dEntity.Inventory.PutItem(_server.EntityFactory.CreateEntity<SideLightSource>());

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