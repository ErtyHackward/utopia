using System;
using ProtoBuf;
using S33M3Resources.Structs;
using System.Collections.Generic;

namespace Utopia.Shared.Chunks
{
    /// <summary>
    /// Represents a base class for a voxel block storage
    /// </summary>
    [ProtoContract]
    public abstract class ChunkDataProvider
    {
        /// <summary>
        /// Occurs when block data was changed
        /// </summary>
        public event EventHandler<ChunkDataProviderDataChangedEventArgs> BlockDataChanged;

        public void OnBlockDataChanged(ChunkDataProviderDataChangedEventArgs e)
        {
            var handler = BlockDataChanged;
            if (handler != null) handler(this, e);
        }
        
        /// <summary>
        /// Occurs when whole buffer was changed
        /// </summary>
        public event EventHandler<ChunkDataProviderBufferChangedEventArgs> BlockBufferChanged;

        public void OnBlockBufferChanged(ChunkDataProviderBufferChangedEventArgs e)
        {
            var handler = BlockBufferChanged;
            if (handler != null) handler(this, e);
        }

        /// <summary>
        /// Gets or sets chunk size
        /// </summary>
        public abstract Vector3I ChunkSize { get; set; }

        /// <summary>
        /// Will send back chunk columns informations = "Kind of Data heightmap"
        /// </summary>
        /// <returns></returns>
        public abstract ChunkColumnInfo[] ColumnsInfo { get; set; }

        /// <summary>
        /// Gets a single ColumnInf from internal location specified
        /// </summary>
        /// <param name="inChunkPosition"></param>
        /// <returns></returns>
        public ChunkColumnInfo GetColumnInfo(Vector2I inChunkPosition)
        {
            return GetColumnInfo(inChunkPosition.X, inChunkPosition.Y);
        }

        /// <summary>
        /// Gets a single ColumnInf from internal location specified
        /// </summary>
        /// <param name="inChunkPosition"></param>
        /// <returns></returns>
        public ChunkColumnInfo GetColumnInfo(Vector3I inChunkPosition)
        {
            return GetColumnInfo(inChunkPosition.X, inChunkPosition.Z);
        }

        /// <summary>
        /// Gets a single ColumnInf from internal location specified
        /// </summary>
        /// <param name="inChunkPositionX"></param>
        /// <param name="inChunkPositionZ"></param>
        /// <returns></returns>
        public ChunkColumnInfo GetColumnInfo(byte inChunkPositionX, byte inChunkPositionZ)
        {
            return GetColumnInfo((int)inChunkPositionX, inChunkPositionZ);
        }

        /// <summary>
        /// Gets a single ColumnInf from internal location specified
        /// </summary>
        /// <returns></returns>
        public abstract ChunkColumnInfo GetColumnInfo(int inChunkPositionX, int inChunkPositionZ);

        /// <summary>
        /// Will return the Id of the Biome most present on the chunk.
        /// </summary>
        public abstract ChunkMetaData ChunkMetaData { get; set; }

        /// <summary>
        /// Requests a full block buffer for a chunk. This operation should be used only for saving the data. Only raw blocks ids
        /// </summary>
        /// <returns></returns>
        public abstract byte[] GetBlocksBytes();

        /// <summary>
        /// Gets a single block from internal location specified
        /// </summary>
        /// <param name="inChunkPosition"></param>
        /// <returns></returns>
        public abstract byte GetBlock(Vector3I inChunkPosition);

        /// <summary>
        /// Gets a single block with tag (can be null)
        /// </summary>
        /// <param name="inChunkPosition"></param>
        /// <param name="blockValue"></param>
        /// <param name="tag"></param>
        public void GetBlockWithTag(Vector3I inChunkPosition, out byte blockValue, out BlockTag tag)
        {
            blockValue = GetBlock(inChunkPosition);
            tag = GetTag(inChunkPosition);
        }

        /// <summary>
        /// Get all tags
        /// </summary>
        /// <returns></returns>
        public abstract IEnumerable<KeyValuePair<Vector3I,BlockTag>> GetTags();

        /// <summary>
        /// Gets an optional block tag. Note that returned object will be a copy of original, to change the tag use SetTag/SetBlock methods
        /// </summary>
        /// <param name="inChunkPosition"></param>
        /// <returns></returns>
        public abstract BlockTag GetTag(Vector3I inChunkPosition);

        /// <summary>
        /// Sets a single block into location specified
        /// </summary>
        /// <param name="inChunkPosition"></param>
        /// <param name="blockValue"></param>
        /// <param name="tag"></param>
        /// <param name="sourceDynamicId">Id of the entity that is responsible for the change</param>
        public abstract void SetBlock(Vector3I inChunkPosition, byte blockValue, BlockTag tag = null, uint sourceDynamicId = 0);

        /// <summary>
        /// Sets a group of blocks
        /// </summary>
        /// <param name="positions"></param>
        /// <param name="values"></param>
        /// <param name="tags"> </param>
        /// <param name="sourceDynamicId">Id of the entity that is responsible for the change</param>
        public abstract void SetBlocks(Vector3I[] positions, byte[] values, BlockTag[] tags = null, uint sourceDynamicId = 0);

        /// <summary>
        /// Sets a full block buffer for a chunk (only raw block ids) and the tags collection
        /// </summary>
        /// <param name="bytes"></param>
        /// <param name="tags"> </param>
        public abstract void SetBlockBytes(byte[] bytes, IEnumerable<KeyValuePair<Vector3I,BlockTag>> tags = null);

        /// <summary>
        /// Gets or sets a block in the buffer
        /// </summary>
        /// <param name="position">Local position of the block</param>
        /// <returns></returns>
        public byte this[Vector3I position]
        {
            get { return GetBlock(position); }
            set { SetBlock(position, value); }
        }

        /// <summary>
        /// Allows to block write operations for the chunk for threadsafety
        /// </summary>
        public abstract object WriteSyncRoot { get; }

        /// <summary>
        /// Consumes all data from the provider, om nom nom nom
        /// Don't use consumed provider anymore
        /// </summary>
        /// <param name="blockData"></param>
        public void Consume(ChunkDataProvider blockData)
        {
            ChunkSize = blockData.ChunkSize;

            ColumnsInfo = blockData.ColumnsInfo;
            ChunkMetaData = blockData.ChunkMetaData;

            SetBlockBytes(blockData.GetBlocksBytes(), blockData.GetTags());

            blockData.ColumnsInfo = null;
            blockData.ChunkMetaData = null;
        }
    }
}
