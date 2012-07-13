using System.IO;
using SharpDX;
using Utopia.Shared.Interfaces;

namespace Utopia.Shared.Entities.Models
{
    /// <summary>
    /// Contains a layout of a single part of the model and a current active frame
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
        /// Optional palm tranformation. Specifies the location of the tool equipped. Only for arm
        /// </summary>
        public Matrix? PalmTransform;

        /// <summary>
        /// Current part bounding box
        /// </summary>
        public BoundingBox BoundingBox;

        public VoxelModelPartState()
        {
            
        }

        public VoxelModelPartState(VoxelModelPartState copyFrom)
        {
            ActiveFrame = copyFrom.ActiveFrame;
            Transform = copyFrom.Transform;
            BoundingBox = copyFrom.BoundingBox;
        }

        public void Save(BinaryWriter writer)
        {
            writer.Write(ActiveFrame);
            writer.Write(Transform);
            writer.Write(PalmTransform.HasValue);
            if (PalmTransform.HasValue)
            {
                writer.Write(PalmTransform.Value);
            }
        }

        public void Load(BinaryReader reader)
        {
            ActiveFrame = reader.ReadByte();
            Transform = reader.ReadMatrix();
            bool havePalmTransform = reader.ReadBoolean();

            if (havePalmTransform)
            {
                PalmTransform = reader.ReadMatrix();
            }

        }
    }
}