using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using Utopia.Shared.Entities;

namespace Utopia.Editor
{
    static class Program
    {
        public static ModelsRepository ModelsRepository { get; set; }

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            ModelsRepository = new ModelsRepository();
            ModelsRepository.Load();

            ModelSelector.Models = ModelsRepository.ModelsFiles;

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new FrmMain());
        }
    }
}
