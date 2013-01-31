using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Threading;
using Ninject;
using S33M3CoreComponents.Sound;
using Utopia.Server;
using Utopia.Server.Interfaces;
using Utopia.Server.Managers;
using Utopia.Server.Sample;
using Utopia.Shared.Configuration;
using Utopia.Shared.Entities;
using Utopia.Shared.Interfaces;
using Utopia.Shared.Net.Web;
using Utopia.Shared.Structs.Helpers;
using Utopia.Shared.World;
using Utopia.Shared.World.Processors;
using Utopia.Shared.World.Processors.Utopia;
using Utopia.Shared.World.WorldConfigs;
using Utopia.Shared.Settings;
using S33M3CoreComponents.Config;

namespace Realms.Server
{
    class Program
    {
        private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        public static Utopia.Server.Server Server
        {
            get { return _server; }
        }

        private static Utopia.Server.Server _server;
        private static IKernel _iocContainer;
        private static ServerGameplayProvider _gameplay;
        private static Timer _reportAliveTimer;

        static void IocBind(WorldParameters param)
        {

            var settingsManager = new XmlSettingsManager<ServerSettings>(@"Server\server.config");
            settingsManager.Load();

            if (string.IsNullOrEmpty(settingsManager.Settings.DatabasePath))
                settingsManager.Settings.DatabasePath = Path.Combine(XmlSettingsManager.GetFilePath("", SettingsStorage.ApplicationData), "Server", "MultiPlayer", param.Seed.ToString(), "ServerWorld.db");

            _iocContainer.Bind<ISoundEngine>().ToConstant<ISoundEngine>(null);

            var sqLiteStorageManager = new SQLiteStorageManager(settingsManager.Settings.DatabasePath, null, param);

            _iocContainer.Bind<WorldParameters>().ToConstant(param).InSingletonScope();
            _iocContainer.Bind<XmlSettingsManager<ServerSettings>>().ToConstant(settingsManager).InSingletonScope();
            _iocContainer.Bind<LandscapeBufferManager>().ToSelf();
            
            IWorldProcessor processor = null;
            switch (param.Configuration.WorldProcessor)
            {
                case Utopia.Shared.Configuration.WorldConfiguration.WorldProcessors.Flat:
                    processor = new FlatWorldProcessor();
                    break;
                case Utopia.Shared.Configuration.WorldConfiguration.WorldProcessors.Utopia:
                    processor = new UtopiaProcessor(param, _iocContainer.Get<EntityFactory>(), _iocContainer.Get<LandscapeBufferManager>());
                    break;
                default:
                    break;
            }

            _iocContainer.Rebind<WorldConfiguration>().ToConstant(param.Configuration);

            var worldGenerator = new WorldGenerator(param, processor);
            _iocContainer.Rebind<WorldGenerator>().ToConstant(worldGenerator).InSingletonScope();

            //_iocContainer.Bind<IWorldProcessorConfig>().To<s33m3WorldConfig>().InSingletonScope().Named("s33m3World");
            //_iocContainer.Bind<IWorldProcessor>().To<s33m3WorldProcessor>().Named("s33m3WorldProcessor");
            //_iocContainer.Bind<IWorldProcessor>().To<LandscapeLayersProcessor>().Named("LandscapeLayersProcessor");

            //_iocContainer.Bind<IWorldProcessorConfig>().To<ErtyHackwardWorldConfig>().InSingletonScope().Named("ErtyHackwardWorld");
            //_iocContainer.Bind<IWorldProcessor>().To<PlanWorldProcessor>().InSingletonScope().Named("ErtyHackwardPlanWorldProcessor");
            
            //_iocContainer.Bind<WorldGenerator>().ToSelf().WithConstructorArgument("worldParameters", param).WithConstructorArgument("processorsConfig", _iocContainer.Get<IWorldProcessorConfig>());

            _iocContainer.Bind<SQLiteStorageManager>().ToConstant(sqLiteStorageManager).InSingletonScope();
            _iocContainer.Bind<IUsersStorage>().To<ServerUsersStorage>().InSingletonScope();
            _iocContainer.Bind<IChunksStorage>().ToConstant(sqLiteStorageManager).InSingletonScope();
            _iocContainer.Bind<IEntityStorage>().ToConstant(sqLiteStorageManager).InSingletonScope();

            _iocContainer.Bind<ServerWebApi>().ToSelf().InSingletonScope();
        }

        static void Main(string[] args)
        {
            // redirect all trace into the console
            Trace.Listeners.Add(new TextWriterTraceListener(Console.Out));

            logger.Info("Utopia Realms game server v{1} Protocol: v{0}", Utopia.Server.Server.ServerProtocolVersion, Assembly.GetExecutingAssembly().GetName().Version);

            var path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "main.realm");

            if (!File.Exists(path))
            {
                if (args.Length == 0)
                {
                    logger.Fatal("Could not file realm to use. Specify realm configuration file path.");
                    return;
                }

                path = args[0];

                if (!File.Exists(path))
                {
                    logger.Fatal("Could not find the realm file at " + path);
                    return;
                }
            }

            var serverFactory = new EntityFactory(null);

            EntityFactory.InitializeProtobufInheritanceHierarchy();

            WorldConfiguration conf;
            try
            {
                conf = WorldConfiguration.LoadFromFile(path);
                serverFactory.Config = conf;
            }
            catch (Exception ex)
            {
                logger.Fatal("Exception when trying to load configuration:\n" + ex.Message);
                return;
            }

            _iocContainer = new StandardKernel(new NinjectSettings { AllowNullInjection = true });
            System.Net.ServicePointManager.Expect100Continue = false;
            _iocContainer.Bind<EntityFactory>().ToConstant(serverFactory).InSingletonScope();

            var wp = new WorldParameters
                {
                    WorldName = "Utopia",
                    SeedName = "",
                    Configuration = conf
                };

            IocBind(wp);

            var settings = _iocContainer.Get<XmlSettingsManager<ServerSettings>>();
            
            _server = new Utopia.Server.Server(
                _iocContainer.Get<XmlSettingsManager<ServerSettings>>(),
                _iocContainer.Get<WorldGenerator>(),
                _iocContainer.Get<IUsersStorage>(),
                _iocContainer.Get<IChunksStorage>(),
                _iocContainer.Get<IEntityStorage>(),
                serverFactory,
                wp
                );

            serverFactory.LandscapeManager = _server.LandscapeManager;
            
            _gameplay = new ServerGameplayProvider(_server, null);

            _server.Services.Add(new TestNpcService());
            _server.Services.Add(new BlueprintRecorderService());
            
            _server.ConnectionManager.Listen();
            _server.LoginManager.PlayerEntityNeeded += LoginManagerPlayerEntityNeeded;

            // send alive message each 5 minutes
            _reportAliveTimer = new Timer(CommitServerInfo, null, 0, 1000 * 60 * 5);

            while (Console.ReadLine() != "exit")
            {
                Console.WriteLine("Type 'exit' to quit");
            }
        }

        static void CommitServerInfo(object state)
        {
            var webApi = _iocContainer.Get<ServerWebApi>();
            var settings = _iocContainer.Get<XmlSettingsManager<ServerSettings>>();
            //webApi.AliveUpdateAsync(settings.Settings.ServerName, settings.Settings.ServerPort, (uint)_server.ConnectionManager.Count);
        }

        static void LoginManagerPlayerEntityNeeded(object sender, NewPlayerEntityNeededEventArgs e)
        {
            e.PlayerEntity = _gameplay.CreateNewPlayerCharacter(e.Connection.Login, e.EntityId);
        }
    }
}
