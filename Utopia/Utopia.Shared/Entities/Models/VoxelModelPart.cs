using System.Collections.Generic;
using System.IO;
using Utopia.Shared.Interfaces;
using Utopia.Shared.Tools.BinarySerializer;

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
        /// Indicates if this part is the head, if true then played head rotation will be applied to the part
        /// </summary>
        public bool IsHead { get; set; }

        /// <summary>
        /// Indicates if this part is the arm. Equipped tool will be displayed at the arm palm point
        /// </summary>
        public bool IsArm { get; set; }

        /// <summary>
        /// Gets a list of frames
        /// </summary>
        public List<VoxelFrame> Frames { get; set; }

        /// <summary>
        /// Gets or sets a default part color mapping, can be null
        /// </summary>
        public ColorMapping ColorMapping { get; set; }

        public VoxelModelPart()
        {
            Frames = new List<VoxelFrame>();
        }

        public void Save(BinaryWriter writer)
        {
            if (string.IsNullOrEmpty(Name))
                Name = "unnamed";

            writer.Write(Name);
            writer.Write(IsHead);
            writer.Write(IsArm);

            writer.Write((byte)Frames.Count);
            foreach (var voxelFrame in Frames)
            {
                voxelFrame.Save(writer);
            }

            ColorMapping.Write(writer, ColorMapping);
        }

        public void Load(BinaryReader reader)
        {
            Name = reader.ReadString();
            IsHead = reader.ReadBoolean();
            IsArm = reader.ReadBoolean();

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

        public override string ToString()
        {
            return Name;
        }
    }
}