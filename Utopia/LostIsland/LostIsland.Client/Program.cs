﻿using System.Threading;

namespace Sandbox.Client
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
            
            using (var main = new GameClient())
            {
                Thread.CurrentThread.Priority = ThreadPriority.Highest;
                main.Run();
            }
        }
    }
}
