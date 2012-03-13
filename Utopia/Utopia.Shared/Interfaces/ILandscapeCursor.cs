using Utopia.Shared.Chunks;
using Utopia.Shared.Structs;
using S33M3Resources.Structs;

namespace Utopia.Shared.Interfaces
{
    public interface ILandscapeCursor
    {
        /// <summary>
        /// Gets or sets cursor global block position
        /// </summary>
        Vector3I GlobalPosition { get; set; }

        /// <summary>
        /// Reads current block type at cursor position
        /// </summary>
        byte Read();

        /// <summary>
        /// Writes specidfied value to current cursor position
        /// </summary>
        /// <param name="value"></param>
        void Write(byte value);

        /// <summary>
        /// Creates a copy of current cursor
        /// </summary>
        /// <returns></returns>
        ILandscapeCursor Clone();

        /// <summary>
        /// Returns whether this block is solid to entity
        /// </summary>
        /// <returns></returns>
        bool IsSolid();

        /// <summary>
        /// Returns whether the block at current position is solid to entity
        /// </summary>
        /// <param name="moveVector">relative move</param>
        /// <returns></returns>
        bool IsSolid(Vector3I moveVector);

        bool IsSolidUp();
        bool IsSolidDown();

        /// <summary>
        /// Returns value downside the cursor
        /// </summary>
        /// <returns></returns>
        byte PeekDown();

        /// <summary>
        /// Returns value upside the cursor
        /// </summary>
        /// <returns></returns>
        byte PeekUp();

        /// <summary>
        /// Returns block value from cursor moved by vector specified
        /// </summary>
        /// <param name="moveVector"></param>
        /// <returns></returns>
        byte PeekValue(Vector3I moveVector);

        ILandscapeCursor MoveDown();
        ILandscapeCursor MoveUp();

        /// <summary>
        /// Moves current cursor and returns itself (Fluent interface)
        /// </summary>
        /// <param name="moveVector"></param>
        /// <returns></returns>
        ILandscapeCursor Move(Vector3I moveVector);
    }
}