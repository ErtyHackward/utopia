using System.Collections.Generic;
using ProtoBuf;

namespace Utopia.Updater
{
    /// <summary>
    /// Represents main file containig all necessary update information
    /// </summary>
    [ProtoContract]
    public class UpdateFile
    {
        /// <summary>
        /// Message to display while updating
        /// </summary>
        [ProtoMember(1)]
        public string Message { get; set; }

        /// <summary>
        /// Display this text instead of update
        /// </summary>
        [ProtoMember(2)]
        public string ErrorText { get; set; }

        /// <summary>
        /// Gets or sets the version of the product
        /// </summary>
        [ProtoMember(3)]
        public string Version { get; set; }

        /// <summary>
        /// Gets or sets the version of the product
        /// </summary>
        [ProtoMember(4)]
        public string UpdateToken { get; set; }

        /// <summary>
        /// Gets or sets list of files 
        /// </summary>
        [ProtoMember(5)]
        public List<FileInfo> Files { get; set; }
    }
}
