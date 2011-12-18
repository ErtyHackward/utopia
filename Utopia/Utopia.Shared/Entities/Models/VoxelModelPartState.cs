using System.IO;
using SharpDX;
using Utopia.Shared.Interfaces;

namespace Utopia.Shared.Entities.Models
{
    /// <summary>
    /// Contains a layout of single part of the model and current active frame
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

        /// <summary>
        /// Current part bounding box
        /// </summary>
        public BoundingBox BoundingBox;

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