using System.Diagnostics;
using System.Threading;
using System;
using S33M3CoreComponents.Config;
using System.IO;
using Utopia.Shared.Helpers;
using Utopia.Shared.Net.Web;

namespace Realms.Client
{
    
    static class Program
    {
        private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        /// <summary>
        /// Indicates whether the debug should be shown
        /// </summary>
        public static bool ShowDebug;
        public static System.Drawing.Size StartUpResolution = new System.Drawing.Size(1280, 720);

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            foreach (var arg in args)
            {
                if (arg.ToLower() == "-nolandscapebuffer") Utopia.Shared.World.LandscapeBufferManager.WithoutLandscapeBuffer = true;
                if (arg.ToLower() == "-showdebug") ShowDebug = true;
                if (arg.ToLower() == "-resetsingleplayerworld") DeleteAllSavedGame();
                if (arg.ToLower() == "-640p") StartUpResolution = new System.Drawing.Size(1024, 576);
                if (arg.ToLower() == "-720p") StartUpResolution = new System.Drawing.Size(1280, 720);
                if (arg.ToLower().StartsWith("-lcdefferedmodelvl"))
                {
                    Utopia.UtopiaRender.LCDefferedModeLvl = int.Parse(arg.ToLower().Replace("-lcdefferedmodelvl", ""));
                }
            }
#if DEBUG
            ShowDebug = true;     
#endif

            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;

            DllLoadHelper.LoadUmnanagedLibrary("sqlite3.dll");

            using (var main = new GameClient())
            {
                Thread.CurrentThread.Priority = ThreadPriority.Highest;
                main.Run();
            }
        }

        static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            var exception = (Exception)e.ExceptionObject;

            logger.Fatal("Unhandled excpetion: {0}\n{1}", exception.Message, exception.StackTrace);

            if (exception.InnerException != null)
            {
                logger.Fatal("Inner exception: {0}\n{1}", exception.InnerException.Message, exception.InnerException.StackTrace);

                if (exception.InnerException.InnerException != null)
                {
                    logger.Fatal("Inner exception 2: {0}\n{1}", exception.InnerException.InnerException.Message, exception.InnerException.InnerException.StackTrace);
                }
            }

            ClientWebApi.SendBugReport(exception);

            var logPath = Path.Combine(Path.GetTempPath(), string.Format("utopia-client-{0}.log", DateTime.Now.ToShortDateString()));

            if (File.Exists(logPath))
            {
                Process.Start(logPath);
            }
        }

        private static void DeleteAllSavedGame()
        {
            string clientDirectory = XmlSettingsManager.GetFilePath(@"Client\Singleplayer", SettingsStorage.ApplicationData, false);
            string serverDirectory = XmlSettingsManager.GetFilePath(@"Server\Singleplayer", SettingsStorage.ApplicationData, false);
            string modelDirectory = XmlSettingsManager.GetFilePath(@"Common", SettingsStorage.ApplicationData, false);

            if (Directory.Exists(clientDirectory))
            {
                foreach (var d in Directory.GetDirectories(clientDirectory))
                {
                    DirectoryInfo di = new DirectoryInfo(d);
                    di.Delete(true);
                }
            }

            if (Directory.Exists(serverDirectory))
            {
                foreach (var d in Directory.GetDirectories(serverDirectory))
                {
                    DirectoryInfo di = new DirectoryInfo(d);
                    di.Delete(true);
                }
            }

            if (Directory.Exists(modelDirectory))
            {
                foreach (var d in Directory.GetDirectories(modelDirectory))
                {
                    DirectoryInfo di = new DirectoryInfo(d);
                    di.Delete(true);
                }
            }

            logger.Info("SinglePlayer saved games have been deleted");
        }
    }
}
