using System;
using Utopia.Shared.Interfaces;
using S33M3Resources.Structs;

namespace Utopia.Shared.Chunks
{
    /// <summary>
    /// Provides ability to travel the cubes without need to know about chunks
    /// </summary>
    public class LandscapeCursor : ILandscapeCursor
    {
        private readonly ILandscapeManager2D _manager;
        private Vector3I _internalPosition;
        private IChunkLayout2D _currentChunk;
        private Vector3I _position;

        /// <summary>
        /// Occurs when someone tries to write using this cursor
        /// </summary>
        public event EventHandler<LandscapeCursorBeforeWriteEventArgs> BeforeWrite;

        private void OnBeforeWrite(LandscapeCursorBeforeWriteEventArgs e)
        {
            var handler = BeforeWrite;
            if (handler != null) handler(this, e);
        }

        /// <summary>
        /// Gets or sets cursor global block position
        /// </summary>
        public Vector3I GlobalPosition
        {
            get { return _position; }
            set
            {
                _position = value;
                _internalPosition = new Vector3I(_position.X % AbstractChunk.ChunkSize.X, _position.Y, _position.Z % AbstractChunk.ChunkSize.Z);

                if (_internalPosition.X < 0)
                    _internalPosition.X = AbstractChunk.ChunkSize.X + _internalPosition.X;
                if (_internalPosition.Z < 0)
                    _internalPosition.Z = AbstractChunk.ChunkSize.Z + _internalPosition.Z;

                _currentChunk = _manager.GetChunk(new Vector2I((int)Math.Floor((double)_position.X / AbstractChunk.ChunkSize.X), (int)Math.Floor((double)_position.Z / AbstractChunk.ChunkSize.Z)));
            }
        }

        /// <summary>
        /// Reads current block type at the cursor position
        /// </summary>
        public byte Read()
        {
            return _currentChunk.BlockData[_internalPosition];
        }

        /// <summary>
        /// Reads current block tag at the cursor position
        /// </summary>
        /// <returns></returns>
        public BlockTag ReadTag()
        {
            return _currentChunk.BlockData.GetTag(_internalPosition);
        }

        public void ReadBlockWithTag(out byte blockValue, out BlockTag tag)
        {
            _currentChunk.BlockData.GetBlockWithTag(_internalPosition, out blockValue, out tag);
        }

        /// <summary>
        /// Writes specidfied value to the current cursor position
        /// </summary>
        /// <param name="value"></param>
        /// <param name="tag"> </param>
        public void Write(byte value, BlockTag tag = null)
        {
            OnBeforeWrite(new LandscapeCursorBeforeWriteEventArgs { GlobalPosition = GlobalPosition, Value = value, BlockTag = tag }); 
            _currentChunk.BlockData.SetBlock(_internalPosition, value, tag);
        }

        protected LandscapeCursor(ILandscapeManager2D manager)
        {
            _manager = manager;
        }
        
        /// <summary>
        /// Creates new instance of landscape cursor 
        /// </summary>
        /// <param name="manager"></param>
        /// <param name="position"></param>
        public LandscapeCursor(ILandscapeManager2D manager, Vector3I position)
            : this(manager)
        {
            GlobalPosition = position;
        }

        /// <summary>
        /// Creates a copy of current cursor
        /// </summary>
        /// <returns></returns>
        public ILandscapeCursor Clone()
        {
            var cursor = new LandscapeCursor(_manager)
                             {
                                 _position = _position,
                                 _internalPosition = _internalPosition,
                                 _currentChunk = _currentChunk
                             };

            return cursor;
        }

        /// <summary>
        /// Returns block value from cursor moved by vector specified
        /// </summary>
        /// <param name="moveVector"></param>
        /// <returns></returns>
        public byte PeekValue(Vector3I moveVector)
        {
            var peekPosition = _internalPosition + moveVector;

            var newChunkPos = _currentChunk.Position;

            if (peekPosition.X >= AbstractChunk.ChunkSize.X)
            {
                newChunkPos.X += peekPosition.X / AbstractChunk.ChunkSize.X;
                peekPosition.X = peekPosition.X % AbstractChunk.ChunkSize.X;
            }
            if (peekPosition.Z >= AbstractChunk.ChunkSize.Z)
            {
                newChunkPos.Y += peekPosition.Z / AbstractChunk.ChunkSize.Z;
                peekPosition.Z = peekPosition.Z % AbstractChunk.ChunkSize.Z;
            }
            if (peekPosition.X < 0)
            {
                newChunkPos.X += (int)Math.Floor((double)peekPosition.X / AbstractChunk.ChunkSize.X);
                peekPosition.X = AbstractChunk.ChunkSize.X + peekPosition.X % AbstractChunk.ChunkSize.X;
            }
            if (peekPosition.Z < 0)
            {
                newChunkPos.Y += (int)Math.Floor((double)peekPosition.Z / AbstractChunk.ChunkSize.Z);
                peekPosition.Z = AbstractChunk.ChunkSize.Z + peekPosition.Z % AbstractChunk.ChunkSize.Z;
            }

            if (!newChunkPos.IsZero())
            {
                var chunk = _manager.GetChunk(newChunkPos);
                return chunk.BlockData[peekPosition];
            }
            return _currentChunk.BlockData[peekPosition];
        }

        /// <summary>
        /// Moves current cursor and returns itself (Fluent interface)
        /// </summary>
        /// <param name="moveVector"></param>
        /// <returns></returns>
        public ILandscapeCursor Move(Vector3I moveVector)
        {
            _internalPosition.X += moveVector.X;
            _internalPosition.Y += moveVector.Y;
            _internalPosition.Z += moveVector.Z;

            _position += moveVector;

            var newChunkPos = _currentChunk.Position;

            if (_internalPosition.X >= AbstractChunk.ChunkSize.X)
            {
                newChunkPos.X += _internalPosition.X / AbstractChunk.ChunkSize.X;
                _internalPosition.X = _internalPosition.X % AbstractChunk.ChunkSize.X;
            }
            if (_internalPosition.Z >= AbstractChunk.ChunkSize.Z)
            {
                newChunkPos.Y += _internalPosition.Z / AbstractChunk.ChunkSize.Z;
                _internalPosition.Z = _internalPosition.Z % AbstractChunk.ChunkSize.Z;
            }
            if (_internalPosition.X < 0)
            {
                newChunkPos.X += (int)Math.Floor((double)_internalPosition.X / AbstractChunk.ChunkSize.X);
                _internalPosition.X = AbstractChunk.ChunkSize.X + _internalPosition.X % AbstractChunk.ChunkSize.X;
            }
            if (_internalPosition.Z < 0)
            {
                newChunkPos.Y += (int)Math.Floor((double)_internalPosition.Z / AbstractChunk.ChunkSize.Z);
                _internalPosition.Z = AbstractChunk.ChunkSize.Z + _internalPosition.Z % AbstractChunk.ChunkSize.Z;
            }

            if (newChunkPos != _currentChunk.Position)
            {
                _currentChunk = _manager.GetChunk(newChunkPos);
            }

            return this;
        }

    }
}