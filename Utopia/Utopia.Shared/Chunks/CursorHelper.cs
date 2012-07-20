using System;
using S33M3Resources.Structs;
using Utopia.Shared.Interfaces;
using Utopia.Shared.Settings;

namespace Utopia.Shared.Chunks
{
    public static class CursorHelper
    {
        /// <summary>
        /// Returns whether this block is solid to entity
        /// </summary>
        /// <returns></returns>
        public static bool IsSolid(this ILandscapeCursor cursor)
        {
            return GameSystemSettings.Current.Settings.CubesProfile[cursor.Read()].IsSolidToEntity;
        }

        /// <summary>
        /// Returns whether this block is solid to entity
        /// </summary>
        /// <returns></returns>
        public static bool IsSolid(this Func<Tuple<ILandscapeCursor, Vector3I>> func)
        {
            var operation = func();

            var value = operation.Item2.IsZero() ? operation.Item1.Read() : operation.Item1.PeekValue(operation.Item2);

            return GameSystemSettings.Current.Settings.CubesProfile[value].IsSolidToEntity;
        }

        /// <summary>
        /// Returns whether this block is empty to entity
        /// </summary>
        /// <returns></returns>
        public static bool IsEmpty(this Func<Tuple<ILandscapeCursor, Vector3I>> func)
        {
            var operation = func();

            var value = operation.Item2.IsZero() ? operation.Item1.Read() : operation.Item1.PeekValue(operation.Item2);

            return !GameSystemSettings.Current.Settings.CubesProfile[value].IsSolidToEntity;
        }

        /// <summary>
        /// Reads a block value
        /// </summary>
        /// <param name="func"></param>
        /// <returns></returns>
        public static byte Read(this Func<Tuple<ILandscapeCursor, Vector3I>> func)
        {
            var operation = func();
            return operation.Item2.IsZero() ? operation.Item1.Read() : operation.Item1.PeekValue(operation.Item2);
        }

        /// <summary>
        /// Moves the cursor
        /// </summary>
        /// <param name="func"></param>
        /// <returns></returns>
        public static ILandscapeCursor Move(this Func<Tuple<ILandscapeCursor, Vector3I>> func)
        {
            var operation = func();
            if (!operation.Item2.IsZero()) operation.Item1.Move(operation.Item2);
            return operation.Item1;
        }
        
        /// <summary>
        /// Set offset for next operation
        /// </summary>
        /// <param name="cursor"></param>
        /// <param name="offset"></param>
        /// <returns></returns>
        public static Func<Tuple<ILandscapeCursor, Vector3I>> Offset(this ILandscapeCursor cursor, Vector3I offset)
        {
            return () => Tuple.Create(cursor, offset);
        }

        public static Func<Tuple<ILandscapeCursor, Vector3I>> Up(this ILandscapeCursor cursor)
        {
            return () =>Tuple.Create(cursor, new Vector3I(0, 1, 0));
        }

        public static Func<Tuple<ILandscapeCursor, Vector3I>> Down(this ILandscapeCursor cursor)
        {
            return () => Tuple.Create(cursor, new Vector3I(0, -1, 0));
        }

        public static Func<Tuple<ILandscapeCursor, Vector3I>> Left(this ILandscapeCursor cursor)
        {
            return () => Tuple.Create(cursor, new Vector3I(-1, 0, 0));
        }

        public static Func<Tuple<ILandscapeCursor, Vector3I>> Right(this ILandscapeCursor cursor)
        {
            return () => Tuple.Create(cursor, new Vector3I(1, 0, 0));
        }

        public static Func<Tuple<ILandscapeCursor, Vector3I>> Front(this ILandscapeCursor cursor)
        {
            return () => Tuple.Create(cursor, new Vector3I(0, 0, -1));
        }

        public static Func<Tuple<ILandscapeCursor, Vector3I>> Back(this ILandscapeCursor cursor)
        {
            return () => Tuple.Create(cursor, new Vector3I(0, 0, 1));
        }
    }
}