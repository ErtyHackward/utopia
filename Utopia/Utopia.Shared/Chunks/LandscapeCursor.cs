using System;
using Utopia.Shared.Entities;
using Utopia.Shared.Entities.Interfaces;
using Utopia.Shared.Interfaces;
using S33M3Resources.Structs;
using Utopia.Shared.Settings;
using Utopia.Shared.World;
using Utopia.Shared.Structs;

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
        private readonly WorldParameters _wp;

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

                //Transform the Cube position into Chunk Position
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

        public byte Read<T>(out T tag) where T : BlockTag
        {
            BlockTag tmptag;
            byte value;

            _currentChunk.BlockData.GetBlockWithTag(_internalPosition, out value, out tmptag);

            tag = tmptag as T;
            return value;
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

        protected LandscapeCursor(ILandscapeManager2D manager, WorldParameters wp)
        {
            _manager = manager;
            _wp = wp;
        }

        /// <summary>
        /// Creates new instance of landscape cursor 
        /// </summary>
        /// <param name="manager"></param>
        /// <param name="position"></param>
        /// <param name="wp"> </param>
        public LandscapeCursor(ILandscapeManager2D manager, Vector3I position, WorldParameters wp)
            : this(manager, wp)
        {
            GlobalPosition = position;
        }

        /// <summary>
        /// Creates a copy of current cursor
        /// </summary>
        /// <returns></returns>
        public ILandscapeCursor Clone()
        {
            var cursor = new LandscapeCursor(_manager, _wp)
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

            if (newChunkPos != _currentChunk.Position)
            {
                var chunk = _manager.GetChunk(newChunkPos);
                return chunk.BlockData[peekPosition];
            }
            return _currentChunk.BlockData[peekPosition];
        }

        /// <summary>
        /// peek Block ID with Tag
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="moveVector"></param>
        /// <param name="tag"></param>
        /// <returns></returns>
        public byte PeekValue<T>(Vector3I moveVector, out T tag) where T : BlockTag
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

            byte value;
            BlockTag tmpTag;
            if (newChunkPos != _currentChunk.Position)
            {
                var chunk = _manager.GetChunk(newChunkPos);
                chunk.BlockData.GetBlockWithTag(peekPosition, out value, out tmpTag);
                tag = tmpTag as T;
                return value;
            }

            _currentChunk.BlockData.GetBlockWithTag(peekPosition, out value, out tmpTag);
            tag = tmpTag as T;
            return value;
        }


        /// <summary>
        /// Return peek cube profile
        /// </summary>
        /// <param name="moveVector"></param>
        /// <returns></returns>
        public BlockProfile PeekProfile(Vector3I moveVector)
        {
            return _wp.Configuration.BlockProfiles[PeekValue(moveVector)];
        }

        public BlockProfile PeekProfile()
        {
            return _wp.Configuration.BlockProfiles[Read()];
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
        
        /// <summary>
        /// Adds static entity to the world
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="sourceDynamicId">Parent entity that issues adding</param>
        public void AddEntity(StaticEntity entity, uint sourceDynamicId = 0)
        {
            Vector3I entityBlockPosition;
            //If the entity is of type IBlockLinkedEntity, then it needs to be store inside the chunk where the LinkedEntity belong.
            if (entity is IBlockLinkedEntity)
            {
                entityBlockPosition = ((IBlockLinkedEntity)entity).LinkedCube;
            }
            else
            {
                entityBlockPosition = (Vector3I)entity.Position;
            }

            var entityChunk = _manager.GetChunk(entityBlockPosition);

            entityChunk.Entities.Add(entity, sourceDynamicId);
        }


        /// <summary>
        /// Remove a static entity from the world
        /// </summary>
        /// <param name="entity">The Entity chunk linke</param>
        /// <param name="sourceDynamicId">The owner the event</param>
        /// <returns>Removed entity</returns>
        public IStaticEntity RemoveEntity(EntityLink entity, uint sourceDynamicId = 0)
        {
            IStaticEntity entityRemoved;
            IChunkLayout2D chunk = _manager.GetChunk(entity.ChunkPosition);
            chunk.Entities.RemoveById(entity.Tail[0], sourceDynamicId, out entityRemoved);

            return entityRemoved;
        }

        /// <summary>
        /// Returns whether this block is solid to entity
        /// </summary>
        /// <returns></returns>
        public bool IsSolid()
        {
            return _wp.Configuration.BlockProfiles[Read()].IsSolidToEntity;
        }

    }
}