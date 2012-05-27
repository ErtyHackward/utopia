using System;
using Utopia.Shared.Chunks;
using S33M3Resources.Structs;

namespace Utopia.Shared.Interfaces
{
    public interface ILandscapeCursor
    {
        /// <summary>
        /// Occurs when someone tries to write using this cursor
        /// </summary>
        event EventHandler<LandscapeCursorBeforeWriteEventArgs> BeforeWrite;

        /// <summary>
        /// Gets or sets cursor global block position
        /// </summary>
        Vector3I GlobalPosition { get; set; }

        /// <summary>
        /// Reads current block type at the cursor position
        /// </summary>
        byte Read();

        /// <summary>
        /// Reads current block tag at the cursor position
        /// </summary>
        /// <returns></returns>
        BlockTag ReadTag();

        /// <summary>
        /// Reads current block and tag at the cursor position
        /// </summary>
        /// <returns></returns>
        void ReadBlockWithTag(out byte blockValue, out BlockTag tag);

        /// <summary>
        /// Writes specidfied value to current cursor position
        /// </summary>
        /// <param name="value"></param>
        /// <param name="tag"> </param>
        void Write(byte value, BlockTag tag = null);

        /// <summary>
        /// Creates a copy of current cursor
        /// </summary>
        /// <returns></returns>
        ILandscapeCursor Clone();

        /// <summary>
        /// Returns block value from cursor moved by vector specified
        /// </summary>
        /// <param name="moveVector"></param>
        /// <returns></returns>
        byte PeekValue(Vector3I moveVector);

        /// <summary>
        /// Moves current cursor and returns itself (Fluent interface)
        /// </summary>
        /// <param name="moveVector"></param>
        /// <returns></returns>
        ILandscapeCursor Move(Vector3I moveVector);
    }

    public class LandscapeCursorBeforeWriteEventArgs : EventArgs
    {
        public Vector3I GlobalPosition { get; set; }

        public byte Value { get; set; }

        public BlockTag BlockTag { get; set; }
    }
}