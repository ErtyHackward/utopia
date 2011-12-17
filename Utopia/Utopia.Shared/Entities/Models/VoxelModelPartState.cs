using System.IO;
using SharpDX;
using Utopia.Shared.Interfaces;

namespace Utopia.Shared.Entities.Models
{
    /// <summary>
    /// Contains a layput of single part of the model
    /// </summary>
    public class VoxelModelPartState : IBinaryStorable
    {
        /// <summary>
        /// Current active frame
        /// </summary>
        public byte ActiveFrame;

        /// <summary>
        /// Frame transformation
        /// </summary>
        public Matrix Transform;

        public void Save(BinaryWriter writer)
        {
            writer.Write(ActiveFrame);
            writer.Write(Transform);
        }

        public void Load(BinaryReader reader)
        {
            ActiveFrame = reader.ReadByte();
            Transform = reader.ReadMatrix();
        }
    }
}