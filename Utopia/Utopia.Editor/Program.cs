using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using Utopia.Editor.Forms;
using Utopia.Editor.Properties;
using Utopia.Shared.Entities;

namespace Utopia.Editor
{
    static class Program
    {
        public static Dictionary<string, Image> ModelIcons { get; set; }

        public static IconManager IconManager { get; private set; }

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            EntityFactory.InitializeProtobufInheritanceHierarchy();

            IconManager = new IconManager();

            if (string.IsNullOrEmpty(Settings.Default.UtopiaFolder))
            {
                // we need to have a path to utopia game folder
                var openFileDialog = new OpenFileDialog();

                openFileDialog.Filter = "Realms.exe main file|Realms.exe";
                openFileDialog.Title = "Editor needs utopia files...";

                if (openFileDialog.ShowDialog() != DialogResult.OK)
                    return;

                Settings.Default.UtopiaFolder = Path.GetDirectoryName(openFileDialog.FileName);
                Settings.Default.Save();
            }

            IconManager.Initialize(Settings.Default.UtopiaFolder);

            ModelIcons = new Dictionary<string, Image>();

            Application.Run(new FrmMain());
        }
    }
}
