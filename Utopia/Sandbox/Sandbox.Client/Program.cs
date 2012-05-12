using System.Threading;
using System;

namespace Sandbox.Client
{
    static class Program
    {
        /// <summary>
        /// Indicates whether the debug should be shown
        /// </summary>
        public static bool ShowDebug;

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            foreach (var arg in args)
            {
                if (arg.ToLower() == "-showdebug")
                    ShowDebug = true;
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
    }
}
