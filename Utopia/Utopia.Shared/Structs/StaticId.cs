namespace Utopia.Shared.Structs
{
    /// <summary>
    /// Represents static entity id
    /// </summary>
    public struct StaticId
    {
        private Vector2I _chunkPosition;
        private uint _id;


        public StaticId(Vector2I vector2I, uint p)
        {
            _chunkPosition = vector2I;
            _id = p;
        }

        /// <summary>
        /// Chunk position
        /// </summary>
        public Vector2I ChunkPosition
        {
            get { return _chunkPosition; }
            set { _chunkPosition = value; }
        }
        
        /// <summary>
        /// Id number of chunk collection
        /// </summary>
        public uint Id
        {
            get { return _id; }
            set { _id = value; }
        }

        public override int GetHashCode()
        {
            return (_chunkPosition.X << 20) + (_chunkPosition.Y << 10) + (int)_id;
        }
    }
}
