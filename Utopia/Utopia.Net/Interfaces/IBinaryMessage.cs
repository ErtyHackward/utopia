using System.IO;

namespace Utopia.Net.Interfaces
{
    /// <summary>
    /// Indicates that this instance can be sent in binary mode
    /// </summary>
    public interface IBinaryMessage
    {
        /// <summary>
        /// Writes all necessary instance members
        /// </summary>
        /// <param name="writer"></param>
        void Write(BinaryWriter writer);
    }
}
