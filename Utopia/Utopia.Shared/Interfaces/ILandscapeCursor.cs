using System;
using Utopia.Shared.Chunks;
using S33M3Resources.Structs;
using Utopia.Shared.Entities.Interfaces;
using Utopia.Shared.Settings;

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
        byte Read<T>(out T tag) where T: BlockTag;

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
        /// Returns a block value from the cursor moved by a vector specified
        /// </summary>
        /// <param name="moveVector"></param>
        /// <returns></returns>
        byte PeekValue(Vector3I moveVector);

        /// <summary>
        /// Returns block and tag from the cursor moved by a vector specified
        /// </summary>
        /// <param name="moveVector"></param>
        /// <param name="tag"> </param>
        /// <returns></returns>
        byte PeekValue<T>(Vector3I moveVector, out T tag) where T: BlockTag;


        /// <summary>
        /// Return Cube profile
        /// </summary>
        /// <param name="moveVector"></param>
        /// <returns></returns>
        CubeProfile PeekProfile();

        /// <summary>
        /// Return Cube profile
        /// </summary>
        /// <param name="moveVector"></param>
        /// <returns></returns>
        CubeProfile PeekProfile(Vector3I moveVector);

        /// <summary>
        /// Moves current cursor and returns itself (Fluent interface)
        /// </summary>
        /// <param name="moveVector"></param>
        /// <returns></returns>
        ILandscapeCursor Move(Vector3I moveVector);

        /// <summary>
        /// Adds static entity to the world
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="sourceDynamicId">Parent entity that issues adding</param>
        void AddEntity(IStaticEntity entity, uint sourceDynamicId = 0);
    }

    public class LandscapeCursorBeforeWriteEventArgs : EventArgs
    {
        public Vector3I GlobalPosition { get; set; }

        public byte Value { get; set; }

        public BlockTag BlockTag { get; set; }
    }
}