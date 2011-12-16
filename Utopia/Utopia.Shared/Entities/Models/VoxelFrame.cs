using System.IO;
using Utopia.Shared.Chunks;
using Utopia.Shared.Interfaces;

namespace Utopia.Shared.Entities.Models
{
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
}