using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Windows.Forms;

namespace Utopia.Launcher
{
    public class UpdateManager
    {
        private Dictionary<string, UpdateFile> _localFiles = new Dictionary<string, UpdateFile>();
        private int _hashed = 0;
        private string _localPath;

        /// <summary>
        /// Collects all files and takes a md5 hash
        /// </summary>
        public void CollectFiles()
        {
            _localFiles.Clear();
            _hashed = 0;
            _localPath = Application.StartupPath;
            CollectDirectory(_localPath);
            HashFiles();
        }

        private void CollectDirectory(string path)
        {
            foreach (var dirPath in Directory.EnumerateDirectories(path))
            {
                CollectDirectory(dirPath);
            }

            foreach (var filePath in Directory.EnumerateFiles(path))
            {
                var relativePath = filePath.Remove(0, _localPath.Length+1);

                var item = new UpdateFile { FilePath = relativePath };

                _localFiles.Add(relativePath, item);
            }
        }

        private void HashFiles()
        {
            var md5Hasher = MD5.Create();
            foreach (var pair in _localFiles)
            {
                byte[] hash;
                using (var fs = File.OpenRead(pair.Key))
                {
                    hash = md5Hasher.ComputeHash(fs);
                }

                var sb = new StringBuilder();

                foreach (var b in hash)
                    sb.Append(b.ToString("x2").ToLower());

                pair.Value.Md5Hash = sb.ToString();
            }
        }



    }
}
