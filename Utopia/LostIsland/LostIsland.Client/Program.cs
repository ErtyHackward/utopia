using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using LostIsland.Shared;
using LostIsland.Client;
using Utopia;
using Utopia.Shared.Chunks;
using Utopia.Shared.Chunks.Entities;

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
            }
            
            using (GameClient main = new GameClient())
            {
                main.Run();
            }
        }
    }
}
