namespace Utopia.Shared.Entities.Interfaces
{
    /// <summary>
    /// Describes container of static entities
    /// </summary>
    public interface IStaticContainer
    {
        /// <summary>
        /// Adds new static entity to the container. Updates static entity id and Container properties
        /// </summary>
        /// <param name="entity"></param>
        void Add(IStaticEntity entity);

        /// <summary>
        /// Removes static entity from the container
        /// </summary>
        /// <param name="entity"></param>
        void Remove(IStaticEntity entity);

        /// <summary>
        /// Gets entity by its static id
        /// </summary>
        /// <param name="staticId"></param>
        /// <returns></returns>
        IStaticEntity GetStaticEntity(uint staticId);

        /// <summary>
        /// Deletes all items from the container
        /// </summary>
        void Clear();

        /// <summary>
        /// Detects if spicified entity in this collection
        /// </summary>
        /// <param name="staticEntityId"></param>
        /// <param name="entity"></param>
        /// <returns></returns>
        bool ContainsId(uint staticEntityId, out IStaticEntity entity);

        /// <summary>
        /// Detects if spicified entity in this collection
        /// </summary>
        /// <param name="staticEntityId"></param>
        /// <returns></returns>
        bool ContainsId(uint staticEntityId);
    }
}
