using System.Collections.Generic;
using System.IO;
using Utopia.Shared.Entities.Models;
using Utopia.Shared.Interfaces;

namespace Utopia.Entities
{
    /// <summary>
    /// Provides file system models storage
    /// </summary>
    public class FileModelStorage : IVoxelModelStorage
    {
        private readonly string _folder;

        /// <summary>
        /// Creates new instance of the FileModelStorage
        /// </summary>
        /// <param name="directory">Directory where uvm files stored</param>
        public FileModelStorage(string directory)
        {
            _folder = directory;

            if (!Directory.Exists(_folder))
               Directory.CreateDirectory(_folder);
        }
        
        public bool Contains(string name)
        {
            return File.Exists(Path.Combine(_folder, name, ".uvm"));
        }

        public VoxelModel Load(string name)
        {
            if (Contains(name))
            {
                var model = VoxelModel.LoadFromFile(Path.Combine(_folder, name, ".uvm"));
                return model;
            }
            return null;
        }

        public void Save(VoxelModel model)
        {
            model.SaveToFile(model.Name + ".uvm");
        }

        public void Delete(string name)
        {
            File.Delete(Path.Combine(_folder, name, ".uvm"));
        }

        public IEnumerable<VoxelModel> Enumerate()
        {
            foreach (var file in Directory.EnumerateFiles(_folder, "*.uvm"))
            {
                var model = VoxelModel.LoadFromFile(file);
                yield return model;
            }
        }
    }
}
