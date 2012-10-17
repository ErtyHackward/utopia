using System.Threading;
using System;
using S33M3CoreComponents.Config;
using System.IO;

namespace Realms.Client
{
    static class Program
    {
        private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        /// <summary>
        /// Indicates whether the debug should be shown
        /// </summary>S
        public static bool ShowDebug;

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            foreach (string arg in args)
            {
            }
            
            foreach (var arg in args)
            {
                if (arg.ToLower() == "-showdebug") ShowDebug = true;
                if (arg.ToLower() == "-resetsingleplayerworld") DeleteAllSavedGame();
            }

#if DEBUG
            ShowDebug = true;     
#endif

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
