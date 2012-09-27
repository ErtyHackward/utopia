using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Utopia.Editor
{
    public class ModelsRepository
    {
        public List<string> ModelsFiles = new List<string>();

        public void Load()
        {
            var path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "Utopia");

            //Create the Directory if needed
            if (Directory.Exists(path) == false) Directory.CreateDirectory(path);

            foreach (var file in Directory.EnumerateFiles(path, "*.png"))
            {
                ModelsFiles.Add(file);
            }
        }
    }
}
