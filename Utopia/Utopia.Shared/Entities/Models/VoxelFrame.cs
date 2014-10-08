using System;
using System.IO;
using ProtoBuf;
using Utopia.Shared.Chunks;
using S33M3Resources.Structs;
using Utopia.Shared.Tools.BinarySerializer;

namespace Utopia.Shared.Entities.Models
{
    /// <summary>
    /// Represents a voxel frame, static array of cubes
    /// </summary>
    [ProtoContract]
    public class VoxelFrame 
    {
        private InsideDataProvider _blockData;

        /// <summary>
        /// Gets or sets frame name
        /// </summary>
        [ProtoMember(1)]
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets global color mapping
        /// </summary>
        [ProtoMember(2)]
        public ColorMapping ColorMapping { get; set; }

        /// <summary>
        /// Gets a frame block data provider
        /// </summary>
        [ProtoMember(3)]
        public InsideDataProvider BlockData
        {
            get { return _blockData; }
            set { _blockData = value; }
        }

        /// <summary>
        /// Allows to control model face generation on edges
        /// </summary>
        [ProtoMember(4)]
        public FrameMirror FrameMirror { get; set; }
        
        public VoxelFrame()
        {
            _blockData = new InsideDataProvider();
            Name = "Noname";
        }

        public VoxelFrame(Vector3I size) : this()
        {
            _blockData.UpdateChunkSize(size);
        }

        public override string ToString()
        {
            return Name + " " + _blockData.ChunkSize;
        }
    }

    [Flags]
    public enum FrameMirror
    {
        None            = 0,
        MirrorTop       = 1 << 0,
        MirrorBottom    = 1 << 1,
        MirrorLeft      = 1 << 2,
        MirrorRight     = 1 << 3,
        MirrorFront     = 1 << 4,
        MirrorBack      = 1 << 5,
        TileTop         = 1 << 6,
        TileBottom      = 1 << 7,
        TileLeft        = 1 << 8,
        TileRight       = 1 << 9,
        TileFront       = 1 << 10,
        TileBack        = 1 << 11,
    }
}