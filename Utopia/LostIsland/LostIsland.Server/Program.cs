using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using LostIsland.Shared;
using LostIsland.Shared.Web;
using Ninject;
using Utopia.Server;
using Utopia.Server.Managers;
using Utopia.Server.Sample;
using Utopia.Shared.Config;
using Utopia.Shared.Entities;
using Utopia.Shared.Interfaces;
using Utopia.Shared.Structs.Helpers;
using Utopia.Shared.World;
using Utopia.Shared.World.Processors;
using Utopia.Shared.World.WorldConfigs;
using Utopia.Shared.Settings;

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

            var settingsManager = new XmlSettingsManager<ServerSettings>("utopiaServer.config");
            settingsManager.Load();

            if (string.IsNullOrEmpty(settingsManager.Settings.DatabasePath))
                settingsManager.Settings.DatabasePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Utopia\\world.db");

            var sqLiteStorageManager = new SQLiteStorageManager(settingsManager.Settings.DatabasePath, null);

            _iocContainer.Bind<WorldParameters>().ToConstant(param).InSingletonScope();
            _iocContainer.Bind<XmlSettingsManager<ServerSettings>>().ToConstant(settingsManager).InSingletonScope();

            //_iocContainer.Bind<IWorldProcessorConfig>().To<s33m3WorldConfig>().InSingletonScope().Named("s33m3World");
            //_iocContainer.Bind<IWorldProcessor>().To<s33m3WorldProcessor>().Named("s33m3WorldProcessor");
            //_iocContainer.Bind<IWorldProcessor>().To<LandscapeLayersProcessor>().Named("LandscapeLayersProcessor");

            _iocContainer.Bind<IWorldProcessorConfig>().To<ErtyHackwardWorldConfig>().InSingletonScope().Named("ErtyHackwardWorld");
            _iocContainer.Bind<IWorldProcessor>().To<PlanWorldProcessor>().InSingletonScope().Named("ErtyHackwardPlanWorldProcessor");
            
            _iocContainer.Bind<WorldGenerator>().ToSelf().WithConstructorArgument("worldParameters", param).WithConstructorArgument("processorsConfig", _iocContainer.Get<IWorldProcessorConfig>());

            _iocContainer.Bind<IUsersStorage>().ToConstant(sqLiteStorageManager).InSingletonScope();
            _iocContainer.Bind<IChunksStorage>().ToConstant(sqLiteStorageManager).InSingletonScope();
            _iocContainer.Bind<IEntityStorage>().ToConstant(sqLiteStorageManager).InSingletonScope();

            _iocContainer.Bind<ClientWebApi>().ToSelf().InSingletonScope();
        }

        static void Main(string[] args)
        {
            // redirect all trace into the console
            Trace.Listeners.Add(new TextWriterTraceListener(Console.Out));

            _iocContainer = new StandardKernel(new NinjectSettings());

            GameSystemSettings.Current = new XmlSettingsManager<GameSystemSetting>(@"GameSystemSettings.xml", SettingsStorage.CustomPath) { CustomSettingsFolderPath = @"Config\" };
            GameSystemSettings.Current.Load();

            IocBind(new WorldParameters());

            TraceHelper.Write("Lost Island game server v{1} Protocol: v{0}", Utopia.Server.Server.ServerProtocolVersion, Assembly.GetExecutingAssembly().GetName().Version);

            var factory = new LostIslandEntityFactory(null);
            
            _server = new Utopia.Server.Server(
                _iocContainer.Get<XmlSettingsManager<ServerSettings>>(),
                _iocContainer.Get<WorldGenerator>(),
                _iocContainer.Get<IUsersStorage>(),
                _iocContainer.Get<IChunksStorage>(),
                _iocContainer.Get<IEntityStorage>(),
                factory
                );

            factory.LandscapeManager = _server.LandscapeManager;

            try
            {
                if (_iocContainer.Get<IWorldProcessor>() is PlanWorldProcessor)
                {
                    var processor = _iocContainer.Get<IWorldProcessor>() as PlanWorldProcessor;
                    _server.LoginManager.GenerationParameters = processor.WorldPlan.Parameters;
                }
            }
            catch (Exception)
            {
            }


            _gameplay = new ServerGameplayProvider(_server);

            _server.Services.Add(new TestNpcService());
            _server.Services.Add(new BlueprintRecorderService());

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
