using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Utopia.Updater
{
    /// <summary>
    /// Represents main file containig all necessary update information
    /// </summary>
    public class UpdateFile
    {
        /// <summary>
        /// Message to display while updating
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        /// Display this text instead of update
        /// </summary>
        public string ErrorText { get; set; }

        /// <summary>
        /// Gets or sets the version of the product
        /// </summary>
        public string Version { get; set; }

        /// <summary>
        /// Gets or sets the version of the product
        /// </summary>
        public string UpdateToken { get; set; }

        /// <summary>
        /// Gets or sets list of files 
        /// </summary>
        public List<UpdateFileInfo> Files { get; set; }

        public UpdateFile()
        {
            Files = new List<UpdateFileInfo>();
        }

        public List<UpdateFileInfo> GetChangedFiles(UpdateFile previousVersion)
        {
            var changedFiles = new List<UpdateFileInfo>();
            foreach (var updateFileInfo in Files)
            {
                var file = previousVersion.Files.Find(uf => uf.SystemPath == updateFileInfo.SystemPath);
                
                if (file != null && file.Md5Hash == updateFileInfo.Md5Hash)
                    continue;
                
                changedFiles.Add(updateFileInfo);
            }

            return changedFiles;
        }

        public List<string> GetRemovedFiles(UpdateFile previousVersion)
        {
            return
                previousVersion.Files.Where(f => !Files.Exists(x => x.SystemPath.Equals(f.SystemPath, StringComparison.CurrentCultureIgnoreCase )))
                    .Select(f => f.SystemPath)
                    .ToList();
        }

        public void Save(BinaryWriter writer)
        {
            writer.Write(Message ?? string.Empty);
            writer.Write(ErrorText ?? string.Empty);
            writer.Write(Version ?? string.Empty);
            writer.Write(UpdateToken ?? string.Empty);
            writer.Write(Files.Count);

            foreach (var updateFileInfo in Files)
            {
                updateFileInfo.Save(writer);
            }
        }

        public void Load(BinaryReader reader)
        {
            Message = reader.ReadString();
            ErrorText = reader.ReadString();
            Version = reader.ReadString();
            UpdateToken = reader.ReadString();

            var count = reader.ReadInt32();

            for (int i = 0; i < count; i++)
            {
                var file = new UpdateFileInfo();
                file.Load(reader);
                Files.Add(file);
            }
        }
    }
}
