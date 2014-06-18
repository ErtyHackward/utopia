using System;
using S33M3Resources.Structs;
using Utopia.Shared.Interfaces;
using Utopia.Shared.Structs;

namespace Utopia.Shared.ClassExt
{
    public static class CursorHelper
    {
        /// <summary>
        /// Moves the cursor while the current block is equals to specified one (default 0)
        /// Maximum move length is 128 blocks
        /// Returns itself
        /// </summary>
        /// <param name="cursor"></param>
        /// <param name="moveVector"></param>
        /// <param name="blockId"></param>
        /// <returns></returns>
        public static ILandscapeCursor MoveWhile(this ILandscapeCursor cursor, Vector3I moveVector, byte blockId = 0)
        {
            if (moveVector.IsZero())
                return cursor;

            while (cursor.Read() == blockId)
            {
                cursor.Move(moveVector);
            }

            return cursor;
        }

        public static ILandscapeCursor MoveWhile(this ILandscapeCursor cursor, Vector3I moveVector, Predicate<byte> condition)
        {
            if (moveVector.IsZero())
                return cursor;

            while (condition(cursor.Read()))
            {
                cursor.Move(moveVector);
            }

            return cursor;
        }

        /// <summary>
        /// Moves the cursor until the current block is equals to specified one (default 0)
        /// Maximum move length is 128 blocks
        /// Returns itself
        /// </summary>
        /// <param name="cursor"></param>
        /// <param name="moveVector"></param>
        /// <param name="blockId"></param>
        /// <returns></returns>
        public static ILandscapeCursor MoveUntil(this ILandscapeCursor cursor, Vector3I moveVector, byte blockId = 0)
        {
            if (moveVector.IsZero())
                return cursor;

            while (cursor.Read() != blockId)
            {
                cursor.Move(moveVector);
            }

            return cursor;
        }

        public static ILandscapeCursor MoveUntil(this ILandscapeCursor cursor, Vector3I moveVector, Predicate<byte> condition)
        {
            if (moveVector.IsZero())
                return cursor;

            while (!condition(cursor.Read()))
            {
                cursor.Move(moveVector);
            }

            return cursor;
        }

        /// <summary>
        /// Starts new transaction and returns the object that will finish it when disposed
        /// Affects only blocks events
        /// </summary>
        /// <returns></returns>
        public static Scope TransactionScope(this ILandscapeCursor cursor)
        {
            cursor.BeginTransaction();
            return new Scope(cursor.CommitTransaction);
        }
    }
}
