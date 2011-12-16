using System.Collections.Generic;
using System.IO;
using Utopia.Shared.Interfaces;
using Utopia.Shared.Structs;

namespace Utopia.Shared.Entities.Models
{
    /// <summary>
    /// Represents a voxel model for voxel entities. Model consists from one or more parts. 
    /// Each part have its own relative position and rotation and may have a color mapping scheme.
    /// </summary>
    public class VoxelModel : IBinaryStorable
    {
        public VoxelModel()
        {
            Parts = new List<VoxelModelPart>();
        }

        /// <summary>
        /// Gets a list of parts of the model
        /// </summary>
        public List<VoxelModelPart> Parts { get; private set; }

        /// <summary>
        /// Gets current model md5 hash
        /// </summary>
        public Md5Hash Hash { get; private set; }

        /// <summary>
        /// Calculates a md5 hash from a model
        /// </summary>
        public void UpdateHash()
        {
            using (var ms = new MemoryStream())
            {
                var writer = new BinaryWriter(ms);
                foreach (var voxelModelPart in Parts)
                {
                    writer.Write(voxelModelPart.RelativePosition);
                    writer.Write(voxelModelPart.Rotation);

                    foreach (var voxelFrame in voxelModelPart.Frames)
                    {
                        var bytes = voxelFrame.BlockData.GetBlocksBytes();
                        ms.Write(bytes, 0, bytes.Length);

                        if (voxelFrame.ColorMapping != null)
                        {
                            foreach (var color in voxelFrame.ColorMapping.BlockColors)
                            {
                                writer.Write(color);
                            }
                        }
                    }

                    if (voxelModelPart.ColorMapping != null)
                    {
                        foreach (var color in voxelModelPart.ColorMapping.BlockColors)
                        {
                            writer.Write(color);
                        }
                    }
                }
                ms.Position = 0;
                Hash = Md5Hash.Calculate(ms);
            }
        }

        public void Save(BinaryWriter writer)
        {
            UpdateHash();

            if (Hash != null)
            {
                writer.Write((byte)16);
                writer.Write(Hash.Bytes);
            }
            else writer.Write((byte)0);

            writer.Write((byte)Parts.Count);
            foreach (var voxelModelPart in Parts)
            {
                voxelModelPart.Save(writer);
            }
        }

        public void Load(BinaryReader reader)
        {
            var count = reader.ReadByte();

            if (count > 0)
            {
                var hash = reader.ReadBytes(count);
                if (hash.Length != 16)
                    throw new EndOfStreamException();
                Hash = new Md5Hash(hash);
            }
            else Hash = null;

            count = reader.ReadByte();

            Parts.Clear();

            for (int i = 0; i < count; i++)
            {
                var modelPart = new VoxelModelPart();
                modelPart.Load(reader);
                Parts.Add(modelPart);
            }
        }
    }
}
