using System;
using Utopia.Shared.Interfaces;
using Utopia.Shared.Structs;
using Utopia.Shared.Structs.Landscape;
using Utopia.Shared.Settings;
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
        /// Reads current block type at cursor position
        /// </summary>
        public byte Read()
        {
            return _currentChunk.BlockData[_internalPosition];
        }

        /// <summary>
        /// Writes specidfied value to current cursor position
        /// </summary>
        /// <param name="value"></param>
        public void Write(byte value)
        {
            _currentChunk.BlockData[_internalPosition] = value;
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
        /// Returns whether this block is solid to entity
        /// </summary>
        /// <returns></returns>
        public bool IsSolid()
        {
            return GameSystemSettings.Current.Settings.CubesProfile[Read()].IsSolidToEntity;
        }

        /// <summary>
        /// Returns whether the block at current position is solid to entity
        /// </summary>
        /// <param name="moveVector">relative move</param>
        /// <returns></returns>
        public bool IsSolid(Vector3I moveVector)
        {
            return GameSystemSettings.Current.Settings.CubesProfile[PeekValue(moveVector)].IsSolidToEntity;
        }

        public bool IsSolidUp()
        {
            return IsSolid(new Vector3I(0, 1, 0));
        }

        public bool IsSolidDown()
        {
            return IsSolid(new Vector3I(0, -1, 0));
        }

        /// <summary>
        /// Returns value downside the cursor
        /// </summary>
        /// <returns></returns>
        public byte PeekDown()
        {
            return PeekValue(new Vector3I(0, -1, 0));
        }

        /// <summary>
        /// Returns value upside the cursor
        /// </summary>
        /// <returns></returns>
        public byte PeekUp()
        {
            return PeekValue(new Vector3I(0, 1, 0));
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

        public ILandscapeCursor MoveDown()
        {
            return Move(new Vector3I(0, -1, 0));
        }

        public ILandscapeCursor MoveUp()
        {
            return Move(new Vector3I(0, 1, 0));
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