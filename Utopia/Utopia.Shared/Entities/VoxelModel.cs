using System;
using System.Collections.Generic;
using System.IO;
using S33M3Engines.Shared.Math;
using SharpDX;
using Utopia.Shared.Interfaces;
using Utopia.Shared.Structs;

namespace Utopia.Shared.Entities
{
    /// <summary>
    /// Represents a voxel model for voxel entities
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

        public void Save(BinaryWriter writer)
        {
            writer.Write((byte)Parts.Count);
            foreach (var voxelModelPart in Parts)
            {
                voxelModelPart.Save(writer);
            }
        }

        public void Load(BinaryReader reader)
        {
            var count = reader.ReadByte();

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
        /// Gets or sets active voxel frame 
        /// </summary>
        public byte VoxelFrameIndex { get; set; }

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
        private Vector3I _size;
        private byte[] _blockData;
        
        /// <summary>
        /// Gets or sets size of a 3d frame
        /// </summary>
        public Vector3I Size
        {
            get { return _size; }
            set { _size = value; }
        }

        /// <summary>
        /// Gets or sets a frame block data
        /// </summary>
        public byte[] BlockData
        {
            get { return _blockData; }
            set { _blockData = value; }
        }

        /// <summary>
        /// Gets or sets a color mapping, if null the parent voxel model part color mapping will be used
        /// </summary>
        public ColorMapping ColorMapping { get; set; }

        public void Save(BinaryWriter writer)
        {
            writer.Write(Size);
            writer.Write(_blockData);

            ColorMapping.Write(writer, ColorMapping);
        }

        public void Load(BinaryReader reader)
        {
            Size = reader.ReadVector3I();

            var length = Size.X * Size.Y * Size.Z;

            _blockData = reader.ReadBytes(length);

            if (_blockData != null && _blockData.Length != length)
            {
                throw new EndOfStreamException();
            }

            ColorMapping = ColorMapping.Read(reader);
        }

        public byte GetBlock(Vector3I pos)
        {
            return _blockData[pos.X * _size.Y + pos.Y + pos.Z * _size.Y * _size.X];
        }

        public void SetBlock(Vector3I pos, byte value)
        {
            _blockData[pos.X * _size.Y + pos.Y + pos.Z * _size.Y * _size.X] = value;
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
