using System;
using System.IO;

namespace Utopia.Updater
{
    public class UpdateFileInfo
    {
        /// <summary>
        /// Relative path of the file, including file name
        /// </summary>
        public string SystemPath { get; set; }

        /// <summary>
        /// Time of last file write
        /// </summary>
        public DateTime LastWriteTime { get; set; }

        /// <summary>
        /// Length of the file
        /// </summary>
        public long Length { get; set; }

        /// <summary>
        /// Md5 hash string to check file integrity
        /// </summary>
        public string Md5Hash { get; set; }

        /// <summary>
        /// Indicates if file should be decompressed before use
        /// </summary>
        public bool Compressed { get; set; }

        /// <summary>
        /// Holds network address to download file from
        /// </summary>
        public string DownloadUri { get; set; }

        public void Save(BinaryWriter writer)
        {
            writer.Write(SystemPath ?? string.Empty);
            writer.Write(DownloadUri ?? string.Empty);
            writer.Write(LastWriteTime.ToBinary());
            writer.Write(Length);
            writer.Write(Md5Hash ?? string.Empty);
            writer.Write(Compressed);
        }

        public void Load(BinaryReader reader)
        {
            SystemPath = reader.ReadString();
            DownloadUri = reader.ReadString();
            LastWriteTime = DateTime.FromBinary(reader.ReadInt64());
            Length = reader.ReadInt64();
            Md5Hash = reader.ReadString();
            Compressed = reader.ReadBoolean();
        }

        public override string ToString()
        {
            return string.Format("{0} {1}", SystemPath, Md5Hash);
        }
    }
}