using System;
using Utopia.Shared.Interfaces;
using Utopia.Shared.Structs;
using Utopia.Shared.Structs.Landscape;

namespace Utopia.Shared.Chunks
{
    public class SingleArrayLandscapeCursor : ILandscapeCursor
    {
        private readonly SingleArrayChunkContainer _landscapeManger;
        private Vector3I _globalPosition;
        private int _bigArrayIndex;

        public SingleArrayLandscapeCursor(SingleArrayChunkContainer landscapeManger, Vector3I blockPosition)
        {
            if (landscapeManger == null) throw new ArgumentNullException("landscapeManger");
            _landscapeManger = landscapeManger;
            GlobalPosition = blockPosition;
            if(!landscapeManger.IndexSafe(blockPosition.X, blockPosition.Y, blockPosition.Z, out _bigArrayIndex))
                throw new IndexOutOfRangeException();
        }

        public Vector3I GlobalPosition
        {
            get { return _globalPosition; }
            set { _globalPosition = value; }
        }

        public byte Read()
        {
            return _landscapeManger.Cubes[_bigArrayIndex].Id;
        }

        public void Write(byte value)
        {
            _landscapeManger.Cubes[_bigArrayIndex].Id = value;
        }

        public ILandscapeCursor Clone()
        {
            return new SingleArrayLandscapeCursor(_landscapeManger, _globalPosition);
        }

        public bool IsSolid()
        {
            return CubeProfile.CubesProfile[Read()].IsSolidToEntity;
        }

        /// <summary>
        /// Returns whether the block at current position is solid to entity
        /// </summary>
        /// <param name="moveVector">relative move</param>
        /// <returns></returns>
        public bool IsSolid(Vector3I moveVector)
        {
            return CubeProfile.CubesProfile[PeekValue(moveVector)].IsSolidToEntity;
        }

        public bool IsSolidUp()
        {
            return CubeProfile.CubesProfile[PeekUp()].IsSolidToEntity;
        }

        public bool IsSolidDown()
        {
            return CubeProfile.CubesProfile[PeekDown()].IsSolidToEntity;
        }

        public byte PeekDown()
        {
            return PeekValue(new Vector3I(0, -1, 0));
        }

        public byte PeekUp()
        {
            return PeekValue(new Vector3I(0, 1, 0));
        }

        public byte PeekValue(Vector3I moveVector)
        {
            var peekIndex = _bigArrayIndex + _landscapeManger.MoveX * moveVector.X + _landscapeManger.MoveY * moveVector.Y + _landscapeManger.MoveZ * moveVector.Z;
            return _landscapeManger.Cubes[peekIndex].Id;
        }

        public ILandscapeCursor MoveDown()
        {
            return Move(new Vector3I(0, -1, 0));
        }

        public ILandscapeCursor MoveUp()
        {
            return Move(new Vector3I(0, 1, 0));
        }

        public ILandscapeCursor Move(Vector3I moveVector)
        {
            _bigArrayIndex += _landscapeManger.MoveX * moveVector.X + _landscapeManger.MoveY * moveVector.Y + _landscapeManger.MoveZ * moveVector.Z;
            return this;
        }
    }
}
