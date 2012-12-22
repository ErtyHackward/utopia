using System;
using ProtoBuf;

namespace Utopia.Updater
{
    [ProtoContract]
    public class UpdateFileInfo
    {
        /// <summary>
        /// Relative path of the file, including file name
        /// </summary>
        [ProtoMember(1)]
        public string SystemPath { get; set; }

        /// <summary>
        /// Time of last file write
        /// </summary>
        [ProtoMember(2)]
        public DateTime LastWriteTime { get; set; }

        /// <summary>
        /// Length of the file
        /// </summary>
        [ProtoMember(3)]
        public long Length { get; set; }

        /// <summary>
        /// Md5 hash string to check file integrity
        /// </summary>
        [ProtoMember(4)]
        public string Md5Hash { get; set; }

        /// <summary>
        /// Indicates if file should be decompressed before use
        /// </summary>
        [ProtoMember(5)]
        public bool Compressed { get; set; }

        /// <summary>
        /// Holds network address to download file from
        /// </summary>
        [ProtoMember(6)]
        public string DownloadUri { get; set; }
    }
}