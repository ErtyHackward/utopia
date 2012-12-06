using System;
using Utopia.Shared.Chunks;
using Utopia.Shared.Entities;
using Utopia.Shared.Entities.Interfaces;
using Utopia.Shared.Interfaces;
using S33M3Resources.Structs;
using Utopia.Shared.Settings;
using Utopia.Shared.Configuration;

namespace Utopia.Worlds.Chunks.ChunkEntityImpacts
{
    public class SingleArrayLandscapeCursor : ILandscapeCursor
    {
        private readonly IChunkEntityImpactManager _landscapeManager;
        private Vector3I _globalPosition;
        private WorldConfiguration _config;
        private int _bigArrayIndex;

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
            if (!landscapeManager.CubesHolder.IndexSafe(blockPosition.X, blockPosition.Y, blockPosition.Z, out _bigArrayIndex))
                throw new IndexOutOfRangeException();
        }

        public Vector3I GlobalPosition
        {
            get { return _globalPosition; }
            set { 
                _globalPosition = value;
                _bigArrayIndex = _landscapeManager.CubesHolder.Index(ref _globalPosition);
            }
        }

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
            throw new NotImplementedException();
        }

        /// <summary>
        /// Reads current block and tag at the cursor position
        /// </summary>
        /// <returns></returns>
        public byte Read<T>(out T tag) where T : BlockTag
        {
            throw new NotImplementedException();
        }

        public void Write(byte value, BlockTag tag = null)
        {
            var handler = BeforeWrite;
            if (handler != null) 
                handler(this, new LandscapeCursorBeforeWriteEventArgs { GlobalPosition = GlobalPosition, Value = value, BlockTag = tag });

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

        public CubeProfile PeekProfile(Vector3I moveVector)
        {
            return _config.CubeProfiles[PeekValue(moveVector)];
        }

        public CubeProfile PeekProfile()
        {
            return _config.CubeProfiles[Read()];
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

            var chunk = _landscapeManager.GetChunk(entityBlockPosition);
            chunk.Entities.Add(entity, sourceDynamicId);
        }

    }
}
