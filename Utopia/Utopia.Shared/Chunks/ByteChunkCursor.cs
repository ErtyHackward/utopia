using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Utopia.Shared.Interfaces;
using S33M3Resources.Structs;
using Utopia.Shared.Structs;

namespace Utopia.Shared.Chunks
{
    public class ByteChunkCursor
    {
        #region Private Variables
        private byte[] _chunkData;
        private Vector3I _internalPosition;
        private int _arrayIndex;
        private ChunkColumnInfo[] _chunkColumn;
        #endregion

        #region Public Properties
        public byte[] ChunkData
        {
            get { return _chunkData; }
            set { _chunkData = value; }
        }
        public Vector3I InternalPosition
        {
            get { return _internalPosition; }
            set { _internalPosition = value; }
        }
        public int ArrayIndex
        {
            get { return _arrayIndex; }
            set { _arrayIndex = value; }
        }
        public ChunkColumnInfo[] ChunkColumn
        {
            get { return _chunkColumn; }
            set { _chunkColumn = value; }
        }
        #endregion

        public ByteChunkCursor(byte[] chunkData, Vector3I internalChunkPosition, ChunkColumnInfo[] chunkColumn)
        {
            _chunkData = chunkData;
            _chunkColumn = chunkColumn;
            SetInternalPosition(internalChunkPosition);
        }

        public ByteChunkCursor(byte[] chunkData, ChunkColumnInfo[] chunkColumn)
        {
            _chunkData = chunkData;
            _chunkColumn = chunkColumn;
        }

        public ByteChunkCursor(ByteChunkCursor cursor)
        {
            ChunkData = cursor.ChunkData;
            ArrayIndex = cursor.ArrayIndex;
            InternalPosition = cursor.InternalPosition;
            _chunkColumn = cursor.ChunkColumn;
        }

        public ByteChunkCursor Clone()
        {
            return new ByteChunkCursor(this);
        }
        #region Public Methods

        public void SetInternalPosition(Vector3I internalChunkPosition)
        {
            _internalPosition = internalChunkPosition;
            _arrayIndex = _internalPosition.Z * AbstractChunk.ChunkSize.X * AbstractChunk.ChunkSize.Y + _internalPosition.X * AbstractChunk.ChunkSize.Y + _internalPosition.Y;
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

        public bool IsSurrendedBy(byte CubeId, bool withoutTopCheck = false)
        {
            return IsSurrendedBy(new byte[] { CubeId }, withoutTopCheck);
        }

        public bool IsSurrendedBy(byte[] CubeIds, bool withoutTopCheck = false)
        {
            int topValue = withoutTopCheck == true ? topValue = 0 : topValue = 1;
            //Save Position
            Vector3I originalInternalPosition = _internalPosition;
            int originalarrayIndex = _arrayIndex;

            for (int y = originalInternalPosition.Y - 1; y <= originalInternalPosition.Y + topValue; y++)
            {
                for (int x = originalInternalPosition.X - 1; x <= originalInternalPosition.X + 1; x++)
                {
                    for (int z = originalInternalPosition.Z - 1; z <= originalInternalPosition.Z + 1; z++)
                    {
                        if (x < 0 || x >= AbstractChunk.ChunkSize.X || z < 0 || z >= AbstractChunk.ChunkSize.Z || y < 0 || y >= AbstractChunk.ChunkSize.Y) continue;
                        SetInternalPosition(x, y, z);
                        byte value = Read();
                        if (CubeIds.Contains(value))
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

            //Check for new max height
            int index2D = _internalPosition.X * AbstractChunk.ChunkSize.Z + _internalPosition.Z;
            if (_internalPosition.Y > _chunkColumn[index2D].MaxHeight) _chunkColumn[index2D].MaxHeight = (byte)_internalPosition.Y;
        }

        public bool Move(CursorRelativeMovement relativeMove)
        {
            return Move((int)relativeMove);
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
                    _arrayIndex = _arrayIndex + (AbstractChunk.ChunkSize.Y);
                    break;
                case 2://X - 1
                    if (_internalPosition.X - 1 < 0) return false;
                    _internalPosition.X--;
                    _arrayIndex = _arrayIndex - (AbstractChunk.ChunkSize.Y);
                    break;
                case 5://Y + 1
                    if (_internalPosition.Y + 1 >= AbstractChunk.ChunkSize.Y) return false;
                    _internalPosition.Y++;
                    _arrayIndex = _arrayIndex + 1;
                    break;
                case 6://Y - 1
                    if (_internalPosition.Y - 1 < 0) return false;
                    _internalPosition.Y--;
                    _arrayIndex = _arrayIndex - 1;
                    break;
                case 3://Z + 1
                    if (_internalPosition.Z + 1 >= AbstractChunk.ChunkSize.Z) return false;
                    _internalPosition.Z++;
                    _arrayIndex = _arrayIndex + (AbstractChunk.ChunkSize.X * AbstractChunk.ChunkSize.Y);
                    break;
                case 4://Z - 1
                    if (_internalPosition.Z - 1 < 0) return false;
                    _internalPosition.Z--;
                    _arrayIndex = _arrayIndex - (AbstractChunk.ChunkSize.X * AbstractChunk.ChunkSize.Y);
                    break;
            }
            return true;
        }

        public bool PeekWithCheck(CursorRelativeMovement relativeMove, out byte value)
        {
            return PeekWithCheck((int)relativeMove, out value);
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
        public bool PeekWithCheck(int relativeMove, out byte value)
        {
            relativeMove = Math.Abs(relativeMove);
            value = 0;
            switch (relativeMove)
            {
                case 1://X + 1
                    if (_internalPosition.X + 1 >= AbstractChunk.ChunkSize.X) return false;
                    value = _chunkData[_arrayIndex + (AbstractChunk.ChunkSize.Y)];
                    break;
                case 2://X - 1
                    if (_internalPosition.X - 1 < 0) return false;
                    value = _chunkData[_arrayIndex - (AbstractChunk.ChunkSize.Y)];
                    break;
                case 5://Y + 1
                    if (_internalPosition.Y + 1 >= AbstractChunk.ChunkSize.Y) return false;
                    value = _chunkData[_arrayIndex + 1];
                    break;
                case 6://Y - 1
                    if (_internalPosition.Y - 1 < 0) return false;
                    value = _chunkData[_arrayIndex - 1];
                    break;
                case 3://Z + 1
                    if (_internalPosition.Z + 1 >= AbstractChunk.ChunkSize.Z) return false;
                    value = _chunkData[_arrayIndex + (AbstractChunk.ChunkSize.X * AbstractChunk.ChunkSize.Y)];
                    break;
                case 4://Z - 1
                    if (_internalPosition.Z - 1 < 0) return false;
                    value = _chunkData[_arrayIndex - (AbstractChunk.ChunkSize.X * AbstractChunk.ChunkSize.Y)];
                    break;
            }
            return true;
        }

        public byte Peek(CursorRelativeMovement relativeMove)
        {
            return Peek((int)relativeMove);
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
                    return _chunkData[_arrayIndex + ( AbstractChunk.ChunkSize.Y)];
                case 2://X - 1
                    return _chunkData[_arrayIndex - (AbstractChunk.ChunkSize.Y)];
                case 5://Y + 1
                    return _chunkData[_arrayIndex + 1];
                case 6://Y - 1
                    return _chunkData[_arrayIndex - 1];
                case 3://Z + 1
                    return _chunkData[_arrayIndex + (AbstractChunk.ChunkSize.X * AbstractChunk.ChunkSize.Y)];
                case 4://Z - 1
                    return _chunkData[_arrayIndex - (AbstractChunk.ChunkSize.X * AbstractChunk.ChunkSize.Y)];
                default:
                    throw new Exception("Peek error, relativeMove unknown : " + relativeMove);
            }
        }

        #endregion

        #region Private Methods
        #endregion
    }
}
