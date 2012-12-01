using System.IO;
using Utopia.Shared.Chunks;
using Utopia.Shared.Interfaces;
using Utopia.Shared.Structs;
using S33M3Resources.Structs;
using Utopia.Shared.Tools.BinarySerializer;

namespace Utopia.Shared.Entities.Models
{
    /// <summary>
    /// Represents a voxel frame, static array of cubes
    /// </summary>
    public class VoxelFrame : IBinaryStorable
    {
        private readonly InsideDataProvider _blockData;

        /// <summary>
        /// Gets or sets global color mapping
        /// </summary>
        public ColorMapping ColorMapping { get; set; }

        /// <summary>
        /// Gets a frame block data provider
        /// </summary>
        public InsideDataProvider BlockData
        {
            get { return _blockData; }
        }
        
        public VoxelFrame()
        {
            _blockData = new InsideDataProvider();
        }

        public VoxelFrame(Vector3I size)
        {
            _blockData = new InsideDataProvider();
            _blockData.UpdateChunkSize(size);
        }

        public void Save(BinaryWriter writer)
        {
            writer.Write(_blockData.ChunkSize);
            writer.Write(_blockData.GetBlocksBytes());
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
        }

        public override string ToString()
        {
            return _blockData.ChunkSize.ToString();
        }
    }
}