using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Windows.Forms;
using LostIsland.Shared;
using LostIsland.Client;
using Utopia;
using Utopia.Shared.Chunks;
using System.Globalization;

namespace LostIsland.Client
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main(string[] args)
        {
            foreach (string arg in args)
            {
            }
            
            using (GameClient main = new GameClient())
            {
                Thread.CurrentThread.Priority = ThreadPriority.Highest;
                main.Run();
            }
        }
    }
}
