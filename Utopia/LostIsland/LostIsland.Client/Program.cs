using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using Utopia;

namespace LostIsland.Client
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            foreach (string arg in args)
            {
                if (arg == "WithDebug") S33M3Engines.D3D.DebugTools.GameConsole.Actif = true;
            }

            using (GameClient main = new GameClient())
            {
                main.Run();
            }
        }
    }
}
