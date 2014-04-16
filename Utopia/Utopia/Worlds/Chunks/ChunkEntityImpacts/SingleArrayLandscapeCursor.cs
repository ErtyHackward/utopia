using System;
using Utopia.Shared.Chunks;
using Utopia.Shared.Entities;
using Utopia.Shared.Entities.Interfaces;
using Utopia.Shared.Interfaces;
using S33M3Resources.Structs;
using Utopia.Shared.Settings;
using Utopia.Shared.Configuration;
using Utopia.Shared.Structs;
using Utopia.Shared.Structs.Helpers;

namespace Utopia.Worlds.Chunks.ChunkEntityImpacts
{
    public class SingleArrayLandscapeCursor : ILandscapeCursor
    {
        private readonly IChunkEntityImpactManager _landscapeManager;
        private Vector3I _globalPosition;
        private WorldConfiguration _config;
        private int _bigArrayIndex;

        public bool isError { get { return _bigArrayIndex == int.MaxValue; } }

        /// <summary>
        /// Occurs when someone tries to write using this cursor
        /// </summary>
        public event EventHandler<LandscapeCursorBeforeWriteEventArgs> BeforeWrite;

        public SingleArrayLandscapeCursor(IChunkEntityImpactManager landscapeManager, Vector3I blockPosition, WorldConfiguration config)
        {
            if (landscapeManager == null) throw new ArgumentNullException("landscapeManager");
            _landscapeManager = landscapeManager;
            GlobalPosition = blockPosition;
            _config = config;
            landscapeManager.CubesHolder.Index(blockPosition.X, blockPosition.Y, blockPosition.Z, true, out _bigArrayIndex);
        }

        public Vector3I GlobalPosition
        {
            get { return _globalPosition; }
            set { 
                _globalPosition = value;
                _bigArrayIndex = _landscapeManager.CubesHolder.Index(ref _globalPosition);
            }
        }

        public uint OwnerDynamicId { get; set; }

        public byte Read()
        {
            return _landscapeManager.CubesHolder.Cubes[_bigArrayIndex].Id;
        }

        /// <summary>
        /// Reads current block tag at the cursor position
        /// </summary>
        /// <returns></returns>
        public BlockTag ReadTag()
        {
            var chunk = (VisualChunk)_landscapeManager.GetChunkFromBlock(_globalPosition);
            if (chunk == null) return null;
            return chunk.BlockData.GetTag(BlockHelper.GlobalToInternalChunkPosition(_globalPosition));
        }

        /// <summary>
        /// Reads current block and tag at the cursor position
        /// </summary>
        /// <returns></returns>
        public byte Read<T>(out T tag) where T : BlockTag
        {
            var chunk = (VisualChunk)_landscapeManager.GetChunkFromBlock(_globalPosition);
            if (chunk == null)
            {
                tag = null;
                return 255; //Send back "Error" value;
            }
            tag = (T)chunk.BlockData.GetTag(BlockHelper.GlobalToInternalChunkPosition(_globalPosition));
            return Read();
        }

        /// <summary>
        /// Writes specidfied value to current cursor position
        /// </summary>
        /// <param name="value"></param>
        /// <param name="tag"> </param>
        /// <param name="sourceDynamicId">Id of the entity that is responsible for that change</param>
        public void Write(byte value, BlockTag tag = null, uint sourceDynamicId = 0)
        {
            var handler = BeforeWrite;
            if (handler != null) 
                handler(this, new LandscapeCursorBeforeWriteEventArgs { 
                    GlobalPosition = GlobalPosition, 
                    Value = value, 
                    BlockTag = tag,
                    SourceDynamicId = sourceDynamicId == 0 ? OwnerDynamicId : sourceDynamicId
                });

            _landscapeManager.ReplaceBlock(_bigArrayIndex, ref _globalPosition, value, false, tag);
        }

        public ILandscapeCursor Clone()
        {
            return new SingleArrayLandscapeCursor(_landscapeManager, _globalPosition, _config);
        }
        
        /// <summary>
        /// Optimized method
        /// </summary>
        /// <returns></returns>
        public byte PeekDown()
        {
            var peekIndex = _landscapeManager.CubesHolder.FastIndex(_bigArrayIndex, _globalPosition.Y, SingleArrayChunkContainer.IdxRelativeMove.Y_Minus1);
            return _landscapeManager.CubesHolder.Cubes[peekIndex].Id;
        }

        /// <summary>
        /// Optimized method
        /// </summary>
        /// <returns></returns>
        public byte PeekUp()
        {
            var peekIndex = _landscapeManager.CubesHolder.FastIndex(_bigArrayIndex, _globalPosition.Y, SingleArrayChunkContainer.IdxRelativeMove.Y_Plus1);
            return _landscapeManager.CubesHolder.Cubes[peekIndex].Id;
        }

        public byte PeekValue(Vector3I moveVector)
        {
            Vector3I peekPosition = _globalPosition + moveVector;
            var peekIndex = _landscapeManager.CubesHolder.Index(ref peekPosition);
            return _landscapeManager.CubesHolder.Cubes[peekIndex].Id;
        }

        public BlockProfile PeekProfile(Vector3I moveVector)
        {
            return _config.BlockProfiles[PeekValue(moveVector)];
        }

        public BlockProfile PeekProfile()
        {
            return _config.BlockProfiles[Read()];
        }

        public byte PeekValue<T>(Vector3I moveVector, out T tag) where T : BlockTag
        {
            throw new NotSupportedException();
        }

        /// <summary>
        /// Optimized method
        /// </summary>
        /// <returns></returns>
        public ILandscapeCursor MoveDown()
        {
            _bigArrayIndex = _landscapeManager.CubesHolder.FastIndex(_bigArrayIndex, _globalPosition.Y, SingleArrayChunkContainer.IdxRelativeMove.Y_Minus1);
            return this;
        }

        /// <summary>
        /// Optimized method
        /// </summary>
        /// <returns></returns>
        public ILandscapeCursor MoveUp()
        {
            _bigArrayIndex = _landscapeManager.CubesHolder.FastIndex(_bigArrayIndex, _globalPosition.Y, SingleArrayChunkContainer.IdxRelativeMove.Y_Plus1);
            return this;
        }

        public ILandscapeCursor Move(Vector3I moveVector)
        {
            _globalPosition += moveVector;
            _bigArrayIndex = _landscapeManager.CubesHolder.Index(ref _globalPosition);
            return this;
        }

        /// <summary>
        /// Adds static entity to the world
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="sourceDynamicId">Parent entity that issues adding</param>
        public void AddEntity(IStaticEntity entity, uint sourceDynamicId = 0)
        {
            _landscapeManager.AddEntity(entity, sourceDynamicId == 0 ? OwnerDynamicId : sourceDynamicId);
        }

        /// <summary>
        /// Remove a static entity from the world
        /// </summary>
        /// <param name="entity">The Entity chunk linke</param>
        /// <param name="sourceDynamicId">The owner the event</param>
        /// <returns>Removed entity</returns>
        public IStaticEntity RemoveEntity(EntityLink entity, uint sourceDynamicId = 0)
        {
            return _landscapeManager.RemoveEntity(entity, sourceDynamicId == 0 ? OwnerDynamicId : sourceDynamicId);
        }

        /// <summary>
        /// throws NotSupportedException
        /// </summary>
        /// <returns></returns>
        public Scope TransactionScope()
        {
            throw new NotSupportedException();
        }

        /// <summary>
        /// throws NotSupportedException
        /// </summary>
        public void BeginTransaction()
        {
            throw new NotSupportedException();
        }

        /// <summary>
        /// throws NotSupportedException
        /// </summary>
        public void CommitTransaction()
        {
            throw new NotSupportedException();
        }
    }
}
