using System.Threading;
using System;
using S33M3CoreComponents.Config;
using System.IO;
using Utopia.Shared.Tools;

namespace Sandbox.Client
{

    static class Program
    {
        private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        /// <summary>
        /// Indicates whether the debug should be shown
        /// </summary>S
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

            ExceptionHandler.SaveCrashReportsToFile();

            using (var main = new GameClient())
            {
                Thread.CurrentThread.Priority = ThreadPriority.Highest;
                main.Run();
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
