namespace Utopia.Shared.Entities.Inventory
{
    /// <summary>
    /// Defines tool pick logic possible types
    /// </summary>
    public enum PickType
    {
        /// <summary>
        /// Means that this block or entity should be picked
        /// </summary>
        Pick,
        /// <summary>
        /// Indicates that pick is failed and nothing should be picked
        /// Will stop picking process
        /// </summary>
        Stop,
        /// <summary>
        /// Allows to skip block or entity and pick following item
        /// </summary>
        Transparent
    }
}