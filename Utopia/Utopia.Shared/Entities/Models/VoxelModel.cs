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
            States = new List<VoxelModelState>();
            Animations = new List<VoxelModelAnimation>();
        }

        /// <summary>
        /// Gets or sets model name
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets a list of parts of the model
        /// </summary>
        public List<VoxelModelPart> Parts { get; private set; }

        /// <summary>
        /// Gets current model md5 hash
        /// </summary>
        public Md5Hash Hash { get; private set; }

        /// <summary>
        /// Geta a list of model states
        /// </summary>
        public List<VoxelModelState> States { get; private set; }

        /// <summary>
        /// Gets a list of model animations
        /// </summary>
        public List<VoxelModelAnimation> Animations { get; private set; }
        
        /// <summary>
        /// Gets or sets global color mapping
        /// </summary>
        public ColorMapping ColorMapping { get; set; }

        /// <summary>
        /// Calculates a md5 hash from a model
        /// </summary>
        public void UpdateHash()
        {
            using (var ms = new MemoryStream())
            {
                var writer = new BinaryWriter(ms);

                ColorMapping.Write(writer, ColorMapping);

                foreach (var voxelModelPart in Parts)
                {
                    foreach (var voxelFrame in voxelModelPart.Frames)
                    {
                        var bytes = voxelFrame.BlockData.GetBlocksBytes();
                        ms.Write(bytes, 0, bytes.Length);
                    }

                    if (voxelModelPart.ColorMapping != null)
                    {
                        foreach (var color in voxelModelPart.ColorMapping.BlockColors)
                        {
                            writer.Write(color);
                        }
                    }
                }

                foreach (var voxelModelState in States)
                {
                    voxelModelState.Save(writer);
                }

                foreach (var animation in Animations)
                {
                    animation.Save(writer);
                }

                ms.Position = 0;
                Hash = Md5Hash.Calculate(ms);
            }
        }
        
        public void Save(BinaryWriter writer)
        {
            UpdateHash();

            writer.Write(Name);

            if (Hash != null)
            {
                writer.Write((byte)16);
                writer.Write(Hash.Bytes);
            }
            else writer.Write((byte)0);

            ColorMapping.Write(writer, ColorMapping);

            writer.Write((byte)Parts.Count);
            foreach (var voxelModelPart in Parts)
            {
                voxelModelPart.Save(writer);
            }
            writer.Write((byte)States.Count);
            foreach (var voxelModelState in States)
            {
                voxelModelState.Save(writer);
            }
            writer.Write((byte)Animations.Count);
            foreach (var voxelModelAnimation in Animations)
            {
                voxelModelAnimation.Save(writer);
            }
        }

        public void Load(BinaryReader reader)
        {
            Name = reader.ReadString();

            var count = reader.ReadByte();

            if (count > 0)
            {
                var hash = reader.ReadBytes(count);
                if (hash.Length != 16)
                    throw new EndOfStreamException();
                Hash = new Md5Hash(hash);
            }
            else Hash = null;

            ColorMapping = ColorMapping.Read(reader);

            count = reader.ReadByte();

            Parts.Clear();

            for (int i = 0; i < count; i++)
            {
                var modelPart = new VoxelModelPart();
                modelPart.Load(reader);
                Parts.Add(modelPart);
            }

            count = reader.ReadByte();

            States.Clear();

            for (int i = 0; i < count; i++)
            {
                var modelState = new VoxelModelState(this);
                modelState.Load(reader);
                States.Add(modelState);
            }

            count = reader.ReadByte();

            Animations.Clear();

            for (int i = 0; i < count; i++)
            {
                var modelState = new VoxelModelAnimation();
                modelState.Load(reader);
                Animations.Add(modelState);
            }


        }
    }
}
