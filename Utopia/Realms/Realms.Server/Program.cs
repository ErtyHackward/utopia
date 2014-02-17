using System;
using System.IO;
using System.Reflection;
using System.Threading;
using S33M3CoreComponents.Sound;
using Utopia.Server;
using Utopia.Server.Interfaces;
using Utopia.Server.Managers;
using Utopia.Shared.Configuration;
using Utopia.Shared.Entities;
using Utopia.Shared.Interfaces;
using Utopia.Shared.Net.Connections;
using Utopia.Shared.Net.Web;
using Utopia.Shared.Services;
using Utopia.Shared.World;
using Utopia.Shared.World.Processors;
using Utopia.Shared.World.Processors.Utopia;
using S33M3CoreComponents.Config;

namespace Realms.Server
{
    class Program
    {
        private static readonly NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        public static Utopia.Server.Server Server
        {
            get { return _server; }
        }

        private static Utopia.Server.Server _server;
        private static ServerGameplayProvider _gameplay;
        private static Timer _reportAliveTimer;
        private static WorldParameters _worldParameters;
        private static XmlSettingsManager<ServerSettings> _settingsManager;
        private static WorldGenerator _worldGenerator;
        private static SqliteStorageManager _sqLiteStorageManager;
        private static EntityFactory _serverFactory;
        private static ServerWebApi _serverWebApi;


        static void IocBind(WorldParameters param)
        {
            _worldParameters = param;

            _settingsManager = new XmlSettingsManager<ServerSettings>(@"Server\server.config");
            _settingsManager.Load();

            if (string.IsNullOrEmpty(_settingsManager.Settings.DatabasePath))
                _settingsManager.Settings.DatabasePath = Path.Combine(XmlSettingsManager.GetFilePath("", SettingsStorage.ApplicationData), "Server", "MultiPlayer", param.Seed.ToString(), "ServerWorld.db");

            //_iocContainer.Bind<ISoundEngine>().ToConstant<ISoundEngine>(null);
            //_iocContainer.Bind<ILandscapeManager>().ToConstant<ILandscapeManager>(null);

            Console.WriteLine("Database path is " + _settingsManager.Settings.DatabasePath);

            _sqLiteStorageManager = new SqliteStorageManager(_settingsManager.Settings.DatabasePath, null, param);

            //_iocContainer.Bind<WorldParameters>().ToConstant(param).InSingletonScope();
            //_iocContainer.Bind<XmlSettingsManager<ServerSettings>>().ToConstant(_settingsManager).InSingletonScope();
            //_iocContainer.Bind<LandscapeBufferManager>().ToSelf();
            
            IWorldProcessor processor = null;
            switch (param.Configuration.WorldProcessor)
            {
                case WorldConfiguration.WorldProcessors.Flat:
                    processor = new FlatWorldProcessor();
                    break;
                case WorldConfiguration.WorldProcessors.Utopia:
                    processor = new UtopiaProcessor(param, _serverFactory, new LandscapeBufferManager());
                    break;
                default:
                    break;
            }

            //_iocContainer.Rebind<WorldConfiguration>().ToConstant(param.Configuration);

            _worldGenerator = new WorldGenerator(param, processor);
            //_iocContainer.Rebind<WorldGenerator>().ToConstant(worldGenerator).InSingletonScope();

            //_iocContainer.Bind<IWorldProcessorConfig>().To<s33m3WorldConfig>().InSingletonScope().Named("s33m3World");
            //_iocContainer.Bind<IWorldProcessor>().To<s33m3WorldProcessor>().Named("s33m3WorldProcessor");
            //_iocContainer.Bind<IWorldProcessor>().To<LandscapeLayersProcessor>().Named("LandscapeLayersProcessor");

            //_iocContainer.Bind<IWorldProcessorConfig>().To<ErtyHackwardWorldConfig>().InSingletonScope().Named("ErtyHackwardWorld");
            //_iocContainer.Bind<IWorldProcessor>().To<PlanWorldProcessor>().InSingletonScope().Named("ErtyHackwardPlanWorldProcessor");
            
            //_iocContainer.Bind<WorldGenerator>().ToSelf().WithConstructorArgument("worldParameters", param).WithConstructorArgument("processorsConfig", _iocContainer.Get<IWorldProcessorConfig>());

            //_iocContainer.Bind<SqliteStorageManager>().ToConstant(_sqLiteStorageManager).InSingletonScope();
            //_iocContainer.Bind<IUsersStorage>().To<ServerUsersStorage>().InSingletonScope();
            //_iocContainer.Bind<IChunksStorage>().ToConstant(_sqLiteStorageManager).InSingletonScope();
            //_iocContainer.Bind<IEntityStorage>().ToConstant(_sqLiteStorageManager).InSingletonScope();

            //_iocContainer.Bind<ServerWebApi>().ToSelf().InSingletonScope();
        }

        static void Main(string[] args)
        {

            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;

            logger.Info("Utopia Realms game server v{1} Protocol: v{0}", ServerConnection.ProtocolVersion, Assembly.GetExecutingAssembly().GetName().Version);

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

            _serverFactory = new EntityFactory();

            EntityFactory.InitializeProtobufInheritanceHierarchy();

            WorldConfiguration conf;
            try
            {
                conf = WorldConfiguration.LoadFromFile(path);
                _serverFactory.Config = conf;
                logger.Info("Realm file {0} loaded", path);
            }
            catch (Exception ex)
            {
                logger.Fatal("Exception when trying to load configuration:\n" + ex.Message);
                return;
            }

            //_iocContainer = new StandardKernel(new NinjectSettings { AllowNullInjection = true });
            System.Net.ServicePointManager.Expect100Continue = false;
            //_iocContainer.Bind<EntityFactory>().ToConstant(_serverFactory).InSingletonScope();

            var wp = new WorldParameters
                {
                    WorldName = "Utopia",
                    SeedName = "",
                    Configuration = conf
                };

            IocBind(wp);


            Console.WriteLine();
            Console.WriteLine("Please enter the name of the server:");
            Console.Write("[{0}]>", _settingsManager.Settings.ServerName);
            var name = Console.ReadLine();

            if (!string.IsNullOrEmpty(name))
            {
                _settingsManager.Settings.ServerName = name;
                _settingsManager.Save();
            }


            _serverWebApi = new ServerWebApi();
            _server = new Utopia.Server.Server(
                _settingsManager,
                _worldGenerator,
                new ServerUsersStorage(_sqLiteStorageManager, _serverWebApi), 
                _sqLiteStorageManager,
                _sqLiteStorageManager,
                _serverFactory,
                wp
                );

            //_iocContainer.Rebind<ILandscapeManager>().ToConstant(_server.LandscapeManager);

            _serverFactory.LandscapeManager = _server.LandscapeManager;
            _serverFactory.DynamicEntityManager = _server.AreaManager;
            _serverFactory.GlobalStateManager = _server.GlobalStateManager;
            _serverFactory.ScheduleManager = _server.Scheduler;
            _serverFactory.ServerSide = true;

            _gameplay = new ServerGameplayProvider(_server, conf);
            
            _server.ConnectionManager.Listen();
            _server.LoginManager.PlayerEntityNeeded += LoginManagerPlayerEntityNeeded;

            // send alive message each 5 minutes
            _reportAliveTimer = new Timer(CommitServerInfo, null, 0, 1000 * 60 * 5);

            while (Console.ReadLine() != "exit")
            {
                Console.WriteLine("Type 'exit' to quit");
            }

            _server.ConnectionManager.Dispose();
        }

        static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            var exception = (Exception)e.ExceptionObject;
            logger.Fatal("Unhandled exception {0}\n{1}", exception.Message, exception.StackTrace);
        }

        static void CommitServerInfo(object state)
        {
            var settings = _settingsManager;
            _serverWebApi.AliveUpdateAsync(settings.Settings.ServerName, settings.Settings.ServerPort, (uint)_server.ConnectionManager.Count, ServerUpdateCompleted);
        }

        private static void ServerUpdateCompleted(WebEventArgs e)
        {
            if (e.Error != 0)
            {
                throw new ApplicationException("Api error: " + e.ErrorText);
            }
            if (e.Exception != null)
            {
                throw e.Exception;
            }
        }

        static void LoginManagerPlayerEntityNeeded(object sender, NewPlayerEntityNeededEventArgs e)
        {
            e.PlayerEntity = _gameplay.CreateNewPlayerCharacter(e.Connection.DisplayName, e.EntityId);
        }
    }
}
