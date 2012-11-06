using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using SharpDX.Direct3D11;
using Utopia.Entities;
using Utopia.Entities.Voxel;
using Utopia.Shared.Entities;
using Utopia.Shared.GameDXStates;
using Utopia.Shared.Settings;
using Utopia.Shared.Tools;

namespace Utopia.Editor
{
    static class Program
    {
        public static ModelsRepository ModelsRepository { get; set; }

        public static Dictionary<string, Image> ModelIcons { get; set; }

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            ModelIcons = new Dictionary<string, Image>();

            ModelsRepository = new ModelsRepository();
            ModelsRepository.Load();

            ModelSelector.Models = ModelsRepository.ModelsFiles;

            Application.Run(new FrmMain());
        }
    }
}
