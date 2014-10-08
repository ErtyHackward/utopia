using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using Utopia.Editor.Forms;
using Utopia.Editor.Properties;
using Utopia.Shared.Entities;
using Utopia.Shared.Helpers;
using Utopia.Shared.Tools;

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

            DllLoadHelper.LoadUmnanagedLibrary("sqlite3.dll");
            IconManager.Initialize(Settings.Default.UtopiaFolder);

            // collect all sound files
            var baseSoundPath = Path.Combine(Settings.Default.UtopiaFolder);
            List<string> files = new List<string>();
            files.Add(null);
            files.AddRange(GetFiles(Path.Combine(Settings.Default.UtopiaFolder), "*.wav").Select(f => f.Remove(0, baseSoundPath.Length + 1)));
            files.AddRange(GetFiles(Path.Combine(Settings.Default.UtopiaFolder), "*.wma").Select(f => f.Remove(0, baseSoundPath.Length + 1)));

            ShortSoundSelector.PossibleSound = files.OrderBy(x => x).ToArray();
            SoundList.PossibleSound = ShortSoundSelector.PossibleSound;
            ModelIcons = new Dictionary<string, Image>();

            Application.Run(new FrmMain());
        }

        private static IEnumerable<string> GetFiles(string folder, string mask)
        {
            foreach (var file in Directory.GetFiles(folder, mask))
            {
                yield return file;
            }

            foreach (var dir in Directory.GetDirectories(folder))
            {
                foreach (var file in GetFiles(dir, mask))
                {
                    yield return file;    
                }
            }
        }
    }
}
