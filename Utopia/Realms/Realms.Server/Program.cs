using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Threading;
using Utopia.Server;
using Utopia.Server.Managers;
using Utopia.Shared.Configuration;
using Utopia.Shared.Entities;
using Utopia.Shared.Interfaces;
using Utopia.Shared.Net.Connections;
using Utopia.Shared.Net.Web;
using Utopia.Shared.Structs;
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
        private static string _newVersionMessage;

        static void IocBind(WorldParameters param)
        {
            _worldParameters = param;

            if (string.IsNullOrEmpty(_settingsManager.Settings.DatabasePath))
                _settingsManager.Settings.DatabasePath = Path.Combine(XmlSettingsManager.GetFilePath("", SettingsStorage.ApplicationData), "Server", "MultiPlayer", param.Seed.ToString(), "ServerWorld.db");

            Console.WriteLine("Database path is " + _settingsManager.Settings.DatabasePath);

            _sqLiteStorageManager = new SqliteStorageManager(_settingsManager.Settings.DatabasePath, null, param);

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

            _worldGenerator = new WorldGenerator(param, processor);
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
            
            System.Net.ServicePointManager.Expect100Continue = false;
            
            _settingsManager = new XmlSettingsManager<ServerSettings>(@"Server\server.config");
            _settingsManager.Load();

            if (string.IsNullOrEmpty(_settingsManager.Settings.Seed))
            {
                Console.WriteLine();
                Console.WriteLine("Please enter the seed:");
                Console.Write("> ");
                _settingsManager.Settings.Seed = Console.ReadLine();
                _settingsManager.Save();
            }

            if (string.IsNullOrEmpty(_settingsManager.Settings.ServerName))
            {
                Console.WriteLine();
                Console.WriteLine("Please enter the name of the server:");
                Console.Write("> ");
                _settingsManager.Settings.ServerName = Console.ReadLine();
                _settingsManager.Save();
            }


            var wp = new WorldParameters
                {
                    WorldName = "Utopia",
                    SeedName = _settingsManager.Settings.Seed,
                    Configuration = conf
                };

            IocBind(wp);

            _serverWebApi = new ServerWebApi();
            _server = new Utopia.Server.Server(
                _settingsManager,
                _worldGenerator,
                new ServerUsersStorage(_sqLiteStorageManager, _serverWebApi), 
                _sqLiteStorageManager,
                _sqLiteStorageManager,
                _sqLiteStorageManager,
                _serverFactory,
                wp
                );
            
            _serverFactory.LandscapeManager = _server.LandscapeManager;
            _serverFactory.DynamicEntityManager = _server.AreaManager;
            _serverFactory.GlobalStateManager = _server.GlobalStateManager;
            _serverFactory.ScheduleManager = _server.Scheduler;
            _serverFactory.ServerSide = true;

            _gameplay = new ServerGameplayProvider(_server, conf);
            
            _server.ConnectionManager.Listen();
            _server.LoginManager.PlayerEntityNeeded += LoginManagerPlayerEntityNeeded;
            _server.LoginManager.PlayerLogged += LoginManager_PlayerLogged;

            // send alive message each 5 minutes
            _reportAliveTimer = new Timer(CommitServerInfo, null, 0, 1000 * 60 * 5);

            while (Console.ReadLine() != "exit")
            {
                Console.WriteLine("Type 'exit' to quit");
            }

            _server.ConnectionManager.Dispose();
        }

        static void LoginManager_PlayerLogged(object sender, PlayerLoggedEventArgs e)
        {
            if (e.ClientConnection.UserRole == UserRole.Administrator)
            {
                if (!string.IsNullOrEmpty(_newVersionMessage))
                {
                    e.ClientConnection.SendChat("New server version is available: " + _newVersionMessage);
                }
            }
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

            try
            {
                var wc = new WebClient();
                var s = wc.DownloadString("http://update.utopiarealms.com/token_server");
                var reader = new StringReader(s);
                Version version;
                if (Version.TryParse(reader.ReadLine(), out version))
                {
                    if (Assembly.GetExecutingAssembly().GetName().Version < version)
                    {
                        _newVersionMessage = s;

                        foreach (var connection in _server.ConnectionManager.Connections().Where(c => c.UserRole == UserRole.Administrator))
                        {
                            connection.SendChat("New server version is available: " + _newVersionMessage);
                        }
                    }
                }
            }
            catch (Exception x)
            {
                logger.Error("Exception during update: " + x.Message);
            }
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
