using System.IO;

namespace Utopia.Shared.Tools.BinarySerializer
{
    /// <summary>
    /// Provides possibility so store object in binary form.
    /// </summary>
    public interface IBinaryStorable
    {
        /// <summary>
        /// Saves current object state to binary form
        /// </summary>
        /// <param name="writer"></param>
        void Save(BinaryWriter writer);

        /// <summary>
        /// Loads current object from binary form
        /// </summary>
        /// <param name="reader"></param>
        void Load(BinaryReader reader);
    }
}
