using System;
using System.IO;
using System.Reflection;
using LostIsland.Shared;
using Ninject;
using Utopia.Server;
using Utopia.Server.Managers;
using Utopia.Server.Services;
using Utopia.Shared.Chunks.Entities;
using Utopia.Shared.Config;
using Utopia.Shared.Interfaces;
using Utopia.Shared.Structs.Landscape;
using Utopia.Shared.World;
using Utopia.Shared.World.Processors;
using Utopia.Shared.World.WorldConfigs;

namespace LostIsland.Server
{
    class Program
    {
        public static Utopia.Server.Server Server
        {
            get { return _server; }
        }

        private static Utopia.Server.Server _server;
        private static IKernel _iocContainer;
        private static ServerGameplayProvider _gameplay;

        static void IocBind(WorldParameters param)
        {

            var settingsManager = new XmlSettingsManager<ServerSettings>("utopiaServer.config", SettingsStorage.ApplicationData);
            settingsManager.Load();

            if (string.IsNullOrEmpty(settingsManager.Settings.DatabasePath))
                settingsManager.Settings.DatabasePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Utopia\\world.db");

            var sqLiteStorageManager = new SQLiteStorageManager(settingsManager.Settings.DatabasePath);

            _iocContainer.Bind<WorldParameters>().ToConstant(param).InSingletonScope();
            _iocContainer.Bind<XmlSettingsManager<ServerSettings>>().ToConstant(settingsManager).InSingletonScope();

            _iocContainer.Bind<IWorldProcessorConfig>().To<s33m3WorldConfig>().InSingletonScope().Named("s33m3World");
            _iocContainer.Bind<IWorldProcessor>().To<s33m3WorldProcessor>().Named("s33m3WorldProcessor");
            _iocContainer.Bind<IWorldProcessor>().To<LandscapeLayersProcessor>().Named("LandscapeLayersProcessor");

            //_iocContainer.Bind<IWorldProcessorConfig>().To<ErtyHackwardWorldConfig>().InSingletonScope().Named("ErtyHackwardWorld");
            //_iocContainer.Bind<IWorldProcessor>().To<ErtyHackwardPlanWorldProcessor>().Named("ErtyHackwardPlanWorldProcessor");
            
            _iocContainer.Bind<WorldGenerator>().ToSelf().WithConstructorArgument("worldParameters", param).WithConstructorArgument("processorsConfig", _iocContainer.Get<IWorldProcessorConfig>());
            _iocContainer.Bind<IUsersStorage>().ToConstant(sqLiteStorageManager).InSingletonScope();
            _iocContainer.Bind<IChunksStorage>().ToConstant(sqLiteStorageManager).InSingletonScope();
            _iocContainer.Bind<IEntityStorage>().ToConstant(sqLiteStorageManager).InSingletonScope();

        }

        [STAThread]
        static void Main(string[] args)
        {
            _iocContainer = new StandardKernel(new NinjectSettings());

            CubeProfile.InitCubeProfiles(@"Config\CubesProfile.xml");

            

            IocBind(new WorldParameters());

            Console.WriteLine("Welcome to Lost Island game server v{1} Protocol: v{0}", Utopia.Server.Server.ServerProtocolVersion, Assembly.GetExecutingAssembly().GetName().Version);

            _server = new Utopia.Server.Server(
                _iocContainer.Get<XmlSettingsManager<ServerSettings>>(),
                _iocContainer.Get<WorldGenerator>(),
                _iocContainer.Get<IUsersStorage>(),
                _iocContainer.Get<IChunksStorage>(),
                _iocContainer.Get<IEntityStorage>()
                );

            EntityFactory.Instance = new LostIslandEntityFactory(_server.LandscapeManager);

            _gameplay = new ServerGameplayProvider(_server);

            _server.Services.Add(new TestNpcService());

            _server.ConnectionManager.Listen();

            _server.LoginManager.PlayerEntityNeeded += LoginManagerPlayerEntityNeeded;

            while (Console.ReadLine() != "exit")
            {

            }

        }

        static void LoginManagerPlayerEntityNeeded(object sender, NewPlayerEntityNeededEventArgs e)
        {
            e.PlayerEntity = _gameplay.CreateNewPlayerCharacter(e.Connection.Login, e.EntityId);
        }
    }
}
