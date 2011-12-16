using System.Collections.Generic;
using System.IO;
using S33M3Engines.Shared.Math;
using SharpDX;
using Utopia.Shared.Chunks;
using Utopia.Shared.Interfaces;
using Utopia.Shared.Structs;

namespace Utopia.Shared.Entities
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
        /// Gets a list of part frames
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

    /// <summary>
    /// Represents a voxel frame, static array of cubes
    /// </summary>
    public class VoxelFrame : IBinaryStorable
    {
        private readonly InsideDataProvider _blockData;

        /// <summary>
        /// Gets a frame block data provider
        /// </summary>
        public InsideDataProvider BlockData
        {
            get { return _blockData; }
        }

        /// <summary>
        /// Gets or sets a color mapping, if null the parent voxel model part color mapping will be used
        /// </summary>
        public ColorMapping ColorMapping { get; set; }

        public VoxelFrame()
        {
            _blockData = new InsideDataProvider();
        }

        public void Save(BinaryWriter writer)
        {
            writer.Write(_blockData.ChunkSize);
            writer.Write(_blockData.GetBlocksBytes());

            ColorMapping.Write(writer, ColorMapping);
        }

        public void Load(BinaryReader reader)
        {
            var size = reader.ReadVector3I();

            var length = size.X * size.Y * size.Z;

            var bytes = reader.ReadBytes(length);

            if (bytes != null && bytes.Length != length)
            {
                throw new EndOfStreamException();
            }

            _blockData.UpdateChunkSize(size);
            _blockData.SetBlockBytes(bytes);

            ColorMapping = ColorMapping.Read(reader);
        }
    }

    /// <summary>
    /// Defines a color mapping information of a model part
    /// </summary>
    public class ColorMapping
    {
        /// <summary>
        /// Gets colors scheme, maximum 256 items
        /// </summary>
        public Color4[] BlockColors { get; set; }

        public static ColorMapping Read(BinaryReader reader)
        {
            var colorMappingLength = reader.ReadByte();

            ColorMapping colorMapping = null;

            if (colorMappingLength > 0)
            {
                colorMapping = new ColorMapping();

                for (var i = 0; i < colorMappingLength; i++)
                {
                    colorMapping.BlockColors[i] = reader.ReadColor4();
                }
            }

            return colorMapping;
        }

        public static void Write(BinaryWriter writer, ColorMapping mapping)
        {
            if (mapping == null || mapping.BlockColors.Length == 0)
            {
                writer.Write((byte)0);
            }
            else
            {
                writer.Write((byte)mapping.BlockColors.Length);

                foreach (var t in mapping.BlockColors)
                {
                    writer.Write(t);
                }
            }
        }
    }
}
