using System.IO;
using Utopia.Shared.Interfaces;

namespace Utopia.Shared.Chunks
{
    /// <summary>
    /// Base class for block tags
    /// </summary>
    public abstract class BlockTag : IBinaryStorable
    {
        public abstract byte Id { get; }

        /// <summary>
        /// Saves current object state to binary form
        /// </summary>
        /// <param name="writer"></param>
        public abstract void Save(BinaryWriter writer);

        /// <summary>
        /// Loads current object from binary form
        /// </summary>
        /// <param name="reader"></param>
        public abstract void Load(BinaryReader reader);
    }
}