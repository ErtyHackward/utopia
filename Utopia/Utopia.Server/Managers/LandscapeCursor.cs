using System;
using Utopia.Server.Structs;
using Utopia.Shared.Chunks;
using Utopia.Shared.Structs;
using Utopia.Shared.Structs.Landscape;

namespace Utopia.Server.Managers
{
    /// <summary>
    /// Provides ability to travel the cubes without need to know about chunks
    /// </summary>
    public class LandscapeCursor
    {
        private readonly LandscapeManager _manager;
        private Location3<int> _internalPosition;
        private ServerChunk _currentChunk;
        private Location3<int> _position;

        /// <summary>
        /// Gets or sets cursor global block position
        /// </summary>
        public Location3<int> GlobalPosition
        {
            get { return _position; }
            set
            {
                _position = value;
                _internalPosition = new Location3<int>(_position.X % AbstractChunk.ChunkSize.X, _position.Y, _position.Z % AbstractChunk.ChunkSize.Z);

                if (_internalPosition.X < 0)
                    _internalPosition.X = AbstractChunk.ChunkSize.X + _internalPosition.X;
                if (_internalPosition.Z < 0)
                    _internalPosition.Z = AbstractChunk.ChunkSize.Z + _internalPosition.Z;

                _currentChunk = _manager.GetChunk(new IntVector2((int)Math.Floor((double)_position.X / AbstractChunk.ChunkSize.X), (int)Math.Floor((double)_position.Z / AbstractChunk.ChunkSize.Z)));
            }
        }

        /// <summary>
        /// Gets current block type at cursor position
        /// </summary>
        public byte Value
        {
            get { return _currentChunk.BlockData[_internalPosition]; }
        }

        protected LandscapeCursor(LandscapeManager manager)
        {
            _manager = manager;
        }
        
        /// <summary>
        /// Creates new instance of landscape cursor 
        /// </summary>
        /// <param name="manager"></param>
        /// <param name="position"></param>
        public LandscapeCursor(LandscapeManager manager, Location3<int> position) : this(manager)
        {
            GlobalPosition = position;
        }

        /// <summary>
        /// Creates a copy of current cursor
        /// </summary>
        /// <returns></returns>
        public LandscapeCursor Clone()
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
            return CubeProfile.CubesProfile[Value].IsSolidToEntity;
        }

        /// <summary>
        /// Returns whether the block at current position is solid to entity
        /// </summary>
        /// <param name="moveVector">relative move</param>
        /// <returns></returns>
        public bool IsSolid(Location3<int> moveVector)
        {
            return CubeProfile.CubesProfile[PeekValue(moveVector)].IsSolidToEntity;
        }

        public bool IsSolidUp()
        {
            return IsSolid(new Location3<int>(0, 1, 0));
        }

        public bool IsSolidDown()
        {
            return IsSolid(new Location3<int>(0, -1, 0));
        }

        /// <summary>
        /// Returns value downside the cursor
        /// </summary>
        /// <returns></returns>
        public byte PeekDown()
        {
            return PeekValue(new Location3<int>(0, -1, 0));
        }

        /// <summary>
        /// Returns value upside the cursor
        /// </summary>
        /// <returns></returns>
        public byte PeekUp()
        {
            return PeekValue(new Location3<int>(0, 1, 0));
        }

        /// <summary>
        /// Returns block value from cursor moved by vector specified
        /// </summary>
        /// <param name="moveVector"></param>
        /// <returns></returns>
        public byte PeekValue(Location3<int> moveVector)
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
                peekPosition.X = -1 * peekPosition.X % AbstractChunk.ChunkSize.X;
            }
            if (peekPosition.Z < 0)
            {
                newChunkPos.Y += (int)Math.Floor((double)peekPosition.Z / AbstractChunk.ChunkSize.Z);
                peekPosition.Z = -1 * peekPosition.Z % AbstractChunk.ChunkSize.Z;
            }

            if (!newChunkPos.IsZero())
            {
                var chunk = _manager.GetChunk(newChunkPos);
                return chunk.BlockData[peekPosition];
            }
            return _currentChunk.BlockData[peekPosition];
        }

        public LandscapeCursor MoveDown()
        {
            return Move(new Location3<int>(0,-1,0));
        }

        public LandscapeCursor MoveUp()
        {
            return Move(new Location3<int>(0, 1, 0));
        }

        /// <summary>
        /// Moves current cursor and returns itself (Fluent interface)
        /// </summary>
        /// <param name="moveVector"></param>
        /// <returns></returns>
        public LandscapeCursor Move(Location3<int> moveVector)
        {
            _internalPosition.X += moveVector.X;
            _internalPosition.Y += moveVector.Y;
            _internalPosition.Z += moveVector.Z;

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
                _internalPosition.X = -1 * _internalPosition.X % AbstractChunk.ChunkSize.X;
            }
            if (_internalPosition.Z < 0)
            {
                newChunkPos.Y += (int)Math.Floor((double)_internalPosition.Z / AbstractChunk.ChunkSize.Z);
                _internalPosition.Z = -1 * _internalPosition.Z % AbstractChunk.ChunkSize.Z;
            }

            if (newChunkPos != _currentChunk.Position)
            {
                _currentChunk = _manager.GetChunk(newChunkPos);
            }

            return this;
        }



    }
}