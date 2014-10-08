using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Threading;
using S33M3Resources.Structs;
using Utopia.Shared.Configuration;
using Utopia.Shared.Entities;
using Utopia.Shared.Entities.Interfaces;
using Utopia.Shared.Helpers;
using Utopia.Shared.Interfaces;
using Utopia.Shared.Net.Connections;
using Utopia.Shared.Net.Web;
using Utopia.Shared.Server;
using Utopia.Shared.Server.Events;
using Utopia.Shared.Server.Managers;
using Utopia.Shared.Structs;
using Utopia.Shared.World;
using Utopia.Shared.World.Processors;
using Utopia.Shared.World.Processors.Utopia;
using S33M3CoreComponents.Config;
using Utopia.Shared.Chunks;

namespace Realms.Server
{
    class Program
    {
        private static readonly NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        public static ServerCore Server
        {
            get { return _server; }
        }

        private static ServerCore _server;
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
            IEntitySpawningControler entitySpawningControler = null;
            switch (param.Configuration.WorldProcessor)
            {
                case WorldConfiguration.WorldProcessors.Flat:
                    processor = new FlatWorldProcessor();
                    break;
                case WorldConfiguration.WorldProcessors.Utopia:
                    processor = new UtopiaProcessor(param, _serverFactory, new LandscapeBufferManager());
                    entitySpawningControler = new UtopiaEntitySpawningControler((UtopiaWorldConfiguration)param.Configuration);
                    break;
                default:
                    break;
            }

            _worldGenerator = new WorldGenerator(param, processor);
            _worldGenerator.EntitySpawningControler = entitySpawningControler;
        }

        static void Main(string[] args)
        {

            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;

            logger.Info("Utopia Realms game server v{1} Protocol: v{0}", ServerConnection.ProtocolVersion, Assembly.GetExecutingAssembly().GetName().Version);

            DllLoadHelper.LoadUmnanagedLibrary("sqlite3.dll");

            var path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "main.realm");

            if (!File.Exists(path))
            {
                if (args.Length == 0)
                {
                    logger.Fatal("Could not find the realm file. Specify realm configuration file path.");
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

            var configFileName = "server.config";
            var port = 0;
            string desc = null;

            for (int i = 0; i < args.Length; i++)
            {
                var argument = args[i];
                if (argument == "-conf" && args.Length > i + 1)
                {
                    configFileName = args[i + 1];
                }
                if (argument == "-port" && args.Length > i + 1)
                {
                    port = int.Parse(args[i + 1]);
                }
                if (argument == "-desc" && args.Length > i + 1)
                {
                    desc = args[i + 1];
                }
            }

            _settingsManager = new XmlSettingsManager<ServerSettings>(@"Server\" + configFileName);
            _settingsManager.Load();
            

            if (port != 0 && _settingsManager.Settings.ServerPort != port)
            {
                _settingsManager.Settings.ServerPort = port;
                _settingsManager.Save();
            }

            if (string.IsNullOrEmpty(_settingsManager.Settings.Seed))
            {
                Console.WriteLine();
                Console.WriteLine("Please enter the seed:");
                Console.Write("> ");
                _settingsManager.Settings.Seed = Console.ReadLine();
                _settingsManager.Save();
            }

            if (string.IsNullOrEmpty(_settingsManager.Settings.ServerName) || _settingsManager.Settings.ServerName == "unnamed server")
            {
                Console.WriteLine();
                Console.WriteLine("Please enter the name of the server:");
                Console.Write("> ");
                _settingsManager.Settings.ServerName = Console.ReadLine();
                _settingsManager.Save();
            }

            if (string.IsNullOrEmpty(_settingsManager.Settings.ServerDescription))
            {
                if (desc != null)
                {
                    _settingsManager.Settings.ServerDescription = desc;
                }
                else
                {
                    Console.WriteLine();
                    Console.WriteLine("Please enter the server description:");
                    Console.Write("> ");
                    _settingsManager.Settings.ServerDescription = Console.ReadLine();
                    _settingsManager.Save();
                }
            }

            var wp = new WorldParameters
                {
                    WorldName = "Utopia",
                    SeedName = _settingsManager.Settings.Seed,
                    Configuration = conf
                };

            IocBind(wp);

            _serverWebApi = new ServerWebApi();
            _server = new ServerCore(
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

            _server.UsersStorage.DefaultRole = _server.CustomStorage.GetVariable("DefaultRole", UserRole.Guest);

            // send alive message each 5 minutes
            _reportAliveTimer = new Timer(CommitServerInfo, null, 0, 1000 * 60 * 5);

            _server.EntityManager.LoadNpcs();

            while (Console.ReadLine() != "exit")
            {
                Console.WriteLine("Type 'exit' to quit");
            }

            _server.Dispose();
        }

        static void LoginManager_PlayerLogged(object sender, ConnectionEventArgs e)
        {
            if (e.Connection.UserRole == UserRole.Administrator)
            {
                if (!string.IsNullOrEmpty(_newVersionMessage))
                {
                    e.Connection.SendChat("New server version is available: " + _newVersionMessage);
                }
            }
        }

        static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            var exception = (Exception)e.ExceptionObject;
            logger.Fatal("Unhandled exception {0}\n{1}", exception.Message, exception.StackTrace);

            var aggregate = e.ExceptionObject as AggregateException;

            if (aggregate != null)
            {
                foreach (var innerException in aggregate.Flatten().InnerExceptions)
                {
                    logger.Fatal("Unhandled exception {0}\n{1}", innerException.Message, innerException.StackTrace);   
                }
            }
        }

        static void CommitServerInfo(object state)
        {
            var settings = _settingsManager;
            
            _serverWebApi.AliveUpdateAsync(
                settings.Settings.ServerName, 
                settings.Settings.ServerDescription, 
                LocalIPAddress(),
                settings.Settings.ServerPort, 
                (uint)_server.ConnectionManager.Count, 
                ServerUpdateCompleted);

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

        static string LocalIPAddress()
        {
            IPHostEntry host;
            string localIP = "";
            host = Dns.GetHostEntry(Dns.GetHostName());
            foreach (IPAddress ip in host.AddressList)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                {
                    localIP = ip.ToString();
                    break;
                }
            }
            return localIP;
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
