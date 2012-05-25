using System;
using System.IO;
using S33M3Resources.Structs;
using Utopia.Shared.Interfaces;

namespace Utopia.Shared.Chunks
{
    /// <summary>
    /// Represents a base class for a voxel block storage
    /// </summary>
    public abstract class ChunkDataProvider : IBinaryStorable
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
        /// Will send back chunk columns informations = "Kind of Data heightmap"
        /// </summary>
        /// <returns></returns>
        public abstract ChunkColumnInfo[] GetColumnsInfo { get; set; }

        /// <summary>
        /// Gets a single ColumnInf from internal location specified
        /// </summary>
        /// <param name="inChunkPosition"></param>
        /// <returns></returns>
        public abstract ChunkColumnInfo GetColumnInfo(Vector2I inChunkPosition);

        /// <summary>
        /// Gets a single ColumnInf from internal location specified
        /// </summary>
        /// <param name="inChunkPosition"></param>
        /// <returns></returns>
        public abstract ChunkColumnInfo GetColumnInfo(Vector3I inChunkPosition);


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
        /// Gets an optional block tag
        /// </summary>
        /// <param name="inChunkPosition"></param>
        /// <returns></returns>
        public abstract BlockTag GetTag(Vector3I inChunkPosition);

        /// <summary>
        /// Gets an optional block tag
        /// </summary>
        /// <param name="inChunkPosition"></param>
        /// <returns></returns>
        public abstract void SetTag(BlockTag tag, Vector3I inChunkPosition);

        /// <summary>
        /// Sets a single block into location specified
        /// </summary>
        /// <param name="inChunkPosition"></param>
        /// <param name="blockValue"></param>
        /// <param name="tag"></param>
        public abstract void SetBlock(Vector3I inChunkPosition, byte blockValue, BlockTag tag = null);

        /// <summary>
        /// Seta a group of blocks
        /// </summary>
        /// <param name="positions"></param>
        /// <param name="values"></param>
        /// <param name="tags"> </param>
        public abstract void SetBlocks(Vector3I[] positions, byte[] values, BlockTag[] tags = null);

        /// <summary>
        /// Sets a full block buffer for a chunk (only raw block ids)
        /// </summary>
        /// <param name="bytes"></param>
        public abstract void SetBlockBytes(byte[] bytes);

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
        /// Saves current object state to binary form
        /// </summary>
        /// <param name="writer"></param>
        public abstract void Save(BinaryWriter writer);

        /// <summary>
        /// Loads current object from binary form
        /// </summary>
        /// <param name="reader"></param>
        public abstract void Load(BinaryReader reader);

        /// <summary>
        /// Allows to block write operations for the chunk for threadsafety
        /// </summary>
        public abstract object WriteSyncRoot { get; }
    }
}
