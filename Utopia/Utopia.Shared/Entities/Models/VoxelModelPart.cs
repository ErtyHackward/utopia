using System.Collections.Generic;
using System.IO;
using S33M3Engines.Shared.Math;
using SharpDX;
using Utopia.Shared.Interfaces;

namespace Utopia.Shared.Entities.Models
{
    /// <summary>
    /// Represents a part of a voxel model. Model parts consists of frames
    /// </summary>
    public class VoxelModelPart : IBinaryStorable
    {
        /// <summary>
        /// Gets or sets voxel model part name, example "Head"
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets a list of frames
        /// </summary>
        public List<VoxelFrame> Frames { get; set; }

        /// <summary>
        /// Gets or sets a default part color mapping, can be null
        /// </summary>
        public ColorMapping ColorMapping { get; set; }

        /// <summary>
        /// Gets or sets current model part rotation
        /// </summary>
        public Quaternion Rotation { get; set; }

        /// <summary>
        /// Gets or sets current model part relative position to the model position
        /// </summary>
        public Vector3D RelativePosition { get; set; }

        public VoxelModelPart()
        {
            Frames = new List<VoxelFrame>();
        }

        public void Save(BinaryWriter writer)
        {
            writer.Write((byte)Frames.Count);
            foreach (var voxelFrame in Frames)
            {
                voxelFrame.Save(writer);
            }

            ColorMapping.Write(writer, ColorMapping);
        }

        public void Load(BinaryReader reader)
        {
            Frames.Clear();
            var framesCount = reader.ReadByte();
            for (int i = 0; i < framesCount; i++)
            {
                var frame = new VoxelFrame();
                frame.Load(reader);
                Frames.Add(frame);
            }

            ColorMapping = ColorMapping.Read(reader);
        }
    }
}