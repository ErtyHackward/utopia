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
using Utopia.Shared.Structs;
using Utopia.Shared.World;
using Utopia.Shared.World.Processors.Utopia;

namespace Sandbox.Client.Components
{
    /// <summary>
    /// Server wrapper for single player
    /// </summary>
    public class LocalServer : IDisposable
    {
        private readonly RuntimeVariables _vars;
        private Server _server;
        private SandboxEntityFactory _serverFactory;
        private SQLiteStorageManager _serverSqliteStorageSinglePlayer;
        private WorldParameters _worldParam;

        public LocalServer(RuntimeVariables vars)
        {
            _vars = vars;
        }

        public void InitSinglePlayerServer(WorldParameters worldParam)
        {
            if (_server != null)
                throw new InvalidOperationException("Already initialized");

            _worldParam = worldParam;

            _serverFactory = new SandboxEntityFactory(null);
            _serverFactory.Config = _worldParam.Configuration;
            var dbPath = Path.Combine(_vars.ApplicationDataPath, "Server", "Singleplayer", _worldParam.WorldName, "ServerWorld.db");

            _serverSqliteStorageSinglePlayer = new SQLiteStorageManager(dbPath, _serverFactory, _worldParam);
            _serverSqliteStorageSinglePlayer.Register("local", "qwe123".GetSHA1Hash(), UserRole.Administrator);

            var settings = new XmlSettingsManager<ServerSettings>(@"Server\localServer.config");
            settings.Load();
            settings.Save();

            //Utopia New Landscape Test
            var utopiaProcessor = new UtopiaProcessor(worldParam, _serverFactory);
            var worldGenerator = new WorldGenerator(worldParam, utopiaProcessor);
           
            settings.Settings.ChunksCountLimit = 1024 * 3; // better use viewRange * viewRange * 3

            _server = new Server(settings, worldGenerator, _serverSqliteStorageSinglePlayer, _serverSqliteStorageSinglePlayer, _serverSqliteStorageSinglePlayer, _serverFactory, worldParam.Configuration);
            _serverFactory.LandscapeManager = _server.LandscapeManager;
            _server.ConnectionManager.LocalMode = true;
            _server.ConnectionManager.Listen();
            _server.LoginManager.PlayerEntityNeeded += LoginManagerPlayerEntityNeeded;
            _server.LoginManager.GenerationParameters = default(Utopia.Shared.World.PlanGenerator.GenerationParameters); // planProcessor.WorldPlan.Parameters;
            _server.Clock.SetCurrentTimeOfDay(TimeSpan.FromHours(12));
            _server.Services.Add(new WaterDynamicService());
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
            adder.SetCube(WorldConfiguration.CubeId.DynamicWater, _worldParam.Configuration.CubeProfiles[WorldConfiguration.CubeId.DynamicWater].Name); //looting a terraincube will create a new blockadder instance or add to the stack

            dEntity.Equipment.Equip(EquipmentSlotType.Hand, new EquipmentSlot<ITool> { Item = adder }, out outItem);

            //Put each cube in the inventory
            foreach (var cubeId in WorldConfiguration.CubeId.All())
            {
                if (cubeId == WorldConfiguration.CubeId.Air)
                    continue;

                var item3 = _server.EntityFactory.CreateEntity<CubeResource>();
                item3.SetCube(cubeId, _worldParam.Configuration.CubeProfiles[cubeId].Name);
                dEntity.Inventory.PutItem(item3);
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