using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Utopia.Shared.Interfaces;
using S33M3Resources.Structs;
using S33M3_Resources.Structs;
using Utopia.Shared.Structs;

namespace Utopia.Shared.Chunks
{
    public class ByteChunkCursor
    {
        #region Private Variables
        private byte[] _chunkData;
        private Vector3I _internalPosition;
        private int _arrayIndex;
        #endregion

        #region Public Properties
        #endregion
        public ByteChunkCursor(byte[] chunkData, Vector3I internalChunkPosition)
        {
            _chunkData = chunkData;
            SetInternalPosition(internalChunkPosition);
        }

        public ByteChunkCursor(byte[] chunkData)
        {
            _chunkData = chunkData;
        }

        #region Public Methods

        public void SetInternalPosition(Vector3I internalChunkPosition)
        {
            _internalPosition = internalChunkPosition;
            _arrayIndex = _internalPosition.X * AbstractChunk.ChunkSize.Z * AbstractChunk.ChunkSize.Y + _internalPosition.Z * AbstractChunk.ChunkSize.Y + _internalPosition.Y;
        }

        public void SetInternalPosition(int x, int y, int z)
        {
            SetInternalPosition(new Vector3I(x, y, z));   
        }

        public bool IsCubePresent(byte CubeId, Vector3I relativeRange)
        {
            //Save Position
            Vector3I originalInternalPosition = _internalPosition;
            int originalarrayIndex = _arrayIndex;

            for (int y = originalInternalPosition.Y - relativeRange.Y; y <= originalInternalPosition.Y + relativeRange.Y; y++)
            {
                for (int x = originalInternalPosition.X - relativeRange.X; x <= originalInternalPosition.X + relativeRange.X; x++)
                {
                    for (int z = originalInternalPosition.Z - relativeRange.Z; z <= originalInternalPosition.Z + relativeRange.Z; z++)
                    {
                        SetInternalPosition(x, y, z);
                        if (Read() == CubeId)
                        {
                            //restore position
                            _internalPosition = originalInternalPosition;
                            _arrayIndex = originalarrayIndex;
                            return true;
                        }
                    }
                }
            }

            //restore position
            _internalPosition = originalInternalPosition;
            _arrayIndex = originalarrayIndex;
            return false;
        }

        public byte Read()
        {
            return _chunkData[_arrayIndex];
        }

        public void Write(byte cubeId)
        {
            _chunkData[_arrayIndex] = cubeId;
        }

        /// <summary>
        /// Move the cursor in the chunk
        /// </summary>
        /// <param name="relativeMove">
        /// 1 = X_Plus1;
        /// 2 = X_Minus1;
        /// 3 = Y_Plus1;
        /// 4 = Y_Minus1;
        /// 5 = Z_Plus1;
        /// 6 = Z_Minus1;
        /// </param>
        /// <returns></returns>
        public bool Move(int relativeMove)
        {
            relativeMove = Math.Abs(relativeMove);
            switch (relativeMove)
            {
                case 1://X + 1
                    if (_internalPosition.X + 1 >= AbstractChunk.ChunkSize.X) return false;
                    _internalPosition.X++;
                    _arrayIndex = _arrayIndex + (AbstractChunk.ChunkSize.Z * AbstractChunk.ChunkSize.Y);
                    break;
                case 2://X - 1
                    if (_internalPosition.X - 1 < 0) return false;
                    _internalPosition.X--;
                    _arrayIndex = _arrayIndex - (AbstractChunk.ChunkSize.Z * AbstractChunk.ChunkSize.Y);
                    break;
                case 3://Y + 1
                    if (_internalPosition.Y + 1 >= AbstractChunk.ChunkSize.Y) return false;
                    _internalPosition.Y++;
                    _arrayIndex = _arrayIndex + 1;
                    break;
                case 4://Y - 1
                    if (_internalPosition.Y - 1 < 0) return false;
                    _internalPosition.Y--;
                    _arrayIndex = _arrayIndex - 1;
                    break;
                case 5://Z + 1
                    if (_internalPosition.Z + 1 >= AbstractChunk.ChunkSize.Z) return false;
                    _internalPosition.Z++;
                    _arrayIndex = _arrayIndex + (AbstractChunk.ChunkSize.Y);
                    break;
                case 6://Z - 1
                    if (_internalPosition.Z - 1 < 0) return false;
                    _internalPosition.Z--;
                    _arrayIndex = _arrayIndex - (AbstractChunk.ChunkSize.Y);
                    break;
            }
            return true;
        }


        /// <summary>
        /// Move the cursor in the chunk
        /// </summary>
        /// <param name="relativeMove">
        /// 1 = X_Plus1;
        /// 2 = X_Minus1;
        /// 3 = Y_Plus1;
        /// 4 = Y_Minus1;
        /// 5 = Z_Plus1;
        /// 6 = Z_Minus1;
        /// </param>
        /// <returns></returns>
        public bool PeekWithCheck(int relativeMove, byte value)
        {
            relativeMove = Math.Abs(relativeMove);
            value = 0;
            switch (relativeMove)
            {
                case 1://X + 1
                    if (_internalPosition.X + 1 >= AbstractChunk.ChunkSize.X) return false;
                    value = _chunkData[_arrayIndex + (AbstractChunk.ChunkSize.Z * AbstractChunk.ChunkSize.Y)];
                    break;
                case 2://X - 1
                    if (_internalPosition.X - 1 < 0) return false;
                    value = _chunkData[_arrayIndex - (AbstractChunk.ChunkSize.Z * AbstractChunk.ChunkSize.Y)];
                    break;
                case 3://Y + 1
                    if (_internalPosition.Y + 1 >= AbstractChunk.ChunkSize.Y) return false;
                    value = _chunkData[_arrayIndex + 1];
                    break;
                case 4://Y - 1
                    if (_internalPosition.Y - 1 < 0) return false;
                    value = _chunkData[_arrayIndex - 1];
                    break;
                case 5://Z + 1
                    if (_internalPosition.Z + 1 >= AbstractChunk.ChunkSize.Z) return false;
                    value = _chunkData[_arrayIndex + (AbstractChunk.ChunkSize.Y)];
                    break;
                case 6://Z - 1
                    if (_internalPosition.Z - 1 < 0) return false;
                    value = _chunkData[_arrayIndex - (AbstractChunk.ChunkSize.Y)];
                    break;
            }
            return true;
        }

        /// <summary>
        /// Move the cursor in the chunk
        /// </summary>
        /// <param name="relativeMove">
        /// 0 = X_Minus1
        /// 1 = X_plus1
        /// 2 = Y_Minus1
        /// 3 = Y_plus1
        /// 4 = Z_Minus1
        /// 5 = Z_plus1
        /// </param>
        /// <returns></returns>
        public byte Peek(int relativeMove)
        {
            relativeMove = Math.Abs(relativeMove);
            switch (relativeMove)
            {
                case 1://X + 1
                    return _chunkData[_arrayIndex + (AbstractChunk.ChunkSize.Z * AbstractChunk.ChunkSize.Y)];
                case 2://X - 1
                    return _chunkData[_arrayIndex - (AbstractChunk.ChunkSize.Z * AbstractChunk.ChunkSize.Y)];
                case 3://Y + 1
                    return _chunkData[_arrayIndex + 1];
                case 4://Y - 1
                    return _chunkData[_arrayIndex - 1];
                case 5://Z + 1
                    return _chunkData[_arrayIndex + (AbstractChunk.ChunkSize.Y)];
                case 6://Z - 1
                    return _chunkData[_arrayIndex - (AbstractChunk.ChunkSize.Y)];
                default:
                    throw new Exception("Peek error, relativeMove unknown : " + relativeMove);
            }
        }

        #endregion

        #region Private Methods
        #endregion
    }
}
