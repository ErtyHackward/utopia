using System;
using Utopia.Shared.Chunks;
using Utopia.Shared.Interfaces;
using Utopia.Shared.Settings;
using S33M3Resources.Structs;

namespace Utopia.Worlds.Chunks.ChunkEntityImpacts
{
    public class SingleArrayLandscapeCursor : ILandscapeCursor
    {
        private readonly IChunkEntityImpactManager _landscapeManager;
        private Vector3I _globalPosition;
        private int _bigArrayIndex;

        public SingleArrayLandscapeCursor(IChunkEntityImpactManager landscapeManager, Vector3I blockPosition)
        {
            if (landscapeManager == null) throw new ArgumentNullException("landscapeManager");
            _landscapeManager = landscapeManager;
            GlobalPosition = blockPosition;
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
        public void ReadBlockWithTag(out byte blockValue, out BlockTag tag)
        {
            throw new NotImplementedException();
        }

        public void Write(byte value, BlockTag tag = null)
        {
            _landscapeManager.ReplaceBlock(_bigArrayIndex, ref _globalPosition, value, tag);
        }

        public ILandscapeCursor Clone()
        {
            return new SingleArrayLandscapeCursor(_landscapeManager, _globalPosition);
        }

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
            return GameSystemSettings.Current.Settings.CubesProfile[PeekUp()].IsSolidToEntity;
        }

        public bool IsSolidDown()
        {
            return GameSystemSettings.Current.Settings.CubesProfile[PeekDown()].IsSolidToEntity;
        }

        public byte PeekDown()
        {
            var peekIndex = _landscapeManager.CubesHolder.FastIndex(_bigArrayIndex, _globalPosition.Y, Shared.Chunks.SingleArrayChunkContainer.IdxRelativeMove.Y_Minus1, false);
            return _landscapeManager.CubesHolder.Cubes[peekIndex].Id;
        }

        public byte PeekUp()
        {
            var peekIndex = _landscapeManager.CubesHolder.FastIndex(_bigArrayIndex, _globalPosition.Y, Shared.Chunks.SingleArrayChunkContainer.IdxRelativeMove.Y_Plus1, false);
            return _landscapeManager.CubesHolder.Cubes[peekIndex].Id;
        }

        public byte PeekValue(Vector3I moveVector)
        {
            Vector3I peekPosition = _globalPosition + moveVector;
            var peekIndex = _landscapeManager.CubesHolder.Index(ref peekPosition);
            return _landscapeManager.CubesHolder.Cubes[peekIndex].Id;
        }

        public ILandscapeCursor MoveDown()
        {
            _bigArrayIndex = _landscapeManager.CubesHolder.FastIndex(_bigArrayIndex, _globalPosition.Y, Shared.Chunks.SingleArrayChunkContainer.IdxRelativeMove.Y_Minus1, false);
            return this;
        }

        public ILandscapeCursor MoveUp()
        {
            _bigArrayIndex = _landscapeManager.CubesHolder.FastIndex(_bigArrayIndex, _globalPosition.Y, Shared.Chunks.SingleArrayChunkContainer.IdxRelativeMove.Y_Plus1, false);
            return this;
        }

        public ILandscapeCursor Move(Vector3I moveVector)
        {
            _globalPosition += moveVector;
            _bigArrayIndex = _landscapeManager.CubesHolder.Index(ref _globalPosition);
            return this;
        }
    }
}
