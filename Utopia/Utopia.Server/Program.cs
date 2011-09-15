using System;
using System.IO;
using System.Reflection;
using Ninject;
using Ninject.Parameters;
using Utopia.Server.Managers;
using Utopia.Server.Services;
using Utopia.Shared.Config;
using Utopia.Shared.Interfaces;
using Utopia.Shared.World;
using Utopia.Shared.World.Processors;
using Utopia.Shared.World.WorldConfigs;

namespace Utopia.Server
{
    class Program
    {
        private static Server _server;
        private static IKernel _iocContainer;

        static void IocBind(WorldParameters param)
        {

            var settingsManager = new XmlSettingsManager<ServerSettings>("utopiaServer.config", SettingsStorage.ApplicationData );
            settingsManager.Load();

            if (string.IsNullOrEmpty(settingsManager.Settings.DatabasePath))
                settingsManager.Settings.DatabasePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Utopia\\world.db");

            var sqLiteStorageManager = new SQLiteStorageManager(settingsManager.Settings.DatabasePath);

            _iocContainer.Bind<WorldParameters>().ToConstant(param).InSingletonScope();
            _iocContainer.Bind<XmlSettingsManager<ServerSettings>>().ToConstant(settingsManager).InSingletonScope();

            _iocContainer.Bind<IWorldProcessorConfig>().To<s33m3WorldConfig>().InSingletonScope().Named("s33m3World");
            _iocContainer.Bind<IWorldProcessor>().To<s33m3WorldProcessor>().Named("s33m3WorldProcessor");
            _iocContainer.Bind<IWorldProcessor>().To<LandscapeLayersProcessor>().Named("LandscapeLayersProcessor");

            
            _iocContainer.Bind<WorldGenerator>().ToSelf().WithConstructorArgument("worldParameters", param).WithConstructorArgument("processorsConfig", _iocContainer.Get<IWorldProcessorConfig>());
            _iocContainer.Bind<IUsersStorage>().ToConstant(sqLiteStorageManager).InSingletonScope();
            _iocContainer.Bind<IChunksStorage>().ToConstant(sqLiteStorageManager).InSingletonScope();
            _iocContainer.Bind<IEntityStorage>().ToConstant(sqLiteStorageManager).InSingletonScope();
            
        }

        static void Main(string[] args)
        {
            _iocContainer = new StandardKernel(new NinjectSettings());
            
            IocBind(new WorldParameters());

            Console.WriteLine("Welcome to Utopia game server v{1} Protocol: v{0}", Server.ServerProtocolVersion, Assembly.GetExecutingAssembly().GetName().Version);

            _server = new Server( 
                _iocContainer.Get<XmlSettingsManager<ServerSettings>>(),  
                _iocContainer.Get<WorldGenerator>(),
                _iocContainer.Get<IUsersStorage>(),
                _iocContainer.Get<IChunksStorage>(),
                _iocContainer.Get<IEntityStorage>()
                );

            _server.Services.Add(new ZombieService());

            _server.Listen();
            
            while (true)
            {
                var command = Console.ReadLine().ToLower();

                switch (command)
                {
                    case "exit":
                        _server.Dispose();
                        return;
                    case "status":
                        Console.WriteLine("Currently {0} entities", _server.AreaManager.EntitiesCount);
                        break;
                }
            }
        }
    }
}
