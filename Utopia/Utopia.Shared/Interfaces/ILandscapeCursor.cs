using System;
using Utopia.Shared.Chunks;
using S33M3Resources.Structs;
using Utopia.Shared.Entities;
using Utopia.Shared.Entities.Interfaces;
using Utopia.Shared.Settings;
using Utopia.Shared.Structs;

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
        /// Gets or sets owner dynamic entity id. This id will be supplied in the events
        /// </summary>
        uint OwnerDynamicId { get; set; }

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
        /// <param name="sourceDynamicId">Id of the entity that is responsible for that change</param>
        void Write(byte value, BlockTag tag = null, uint sourceDynamicId = 0);

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
        /// <returns></returns>
        BlockProfile PeekProfile();

        /// <summary>
        /// Return Cube profile
        /// </summary>
        /// <param name="moveVector"></param>
        /// <returns></returns>
        BlockProfile PeekProfile(Vector3I moveVector);

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

        /// <summary>
        /// Remove static entity from the world
        /// </summary>
        /// <param name="entity">The chunk entity link</param>
        /// <param name="sourceDynamicId">Parent entity that issues adding</param>
        /// <returns></returns>
        IStaticEntity RemoveEntity(EntityLink entity, uint sourceDynamicId = 0);

        /// <summary>
        /// Starts new transaction and returns the object that will finish it when disposed
        /// </summary>
        /// <returns></returns>
        Scope TransactionScope();

        /// <summary>
        /// Starts new transaction
        /// </summary>
        void BeginTransaction();

        /// <summary>
        /// Finish the transaction
        /// </summary>
        void CommitTransaction();
    }

    public class Scope : IDisposable
    {
        Action _action;

        public Scope(Action action)
        {
            _action = action;
        }
        public void Dispose()
        {
            if (_action != null)
                _action();
        }
    }

    public class LandscapeCursorBeforeWriteEventArgs : EventArgs
    {
        public Vector3I GlobalPosition { get; set; }

        public byte Value { get; set; }

        public BlockTag BlockTag { get; set; }

        public uint SourceDynamicId { get; set; }
    }
}