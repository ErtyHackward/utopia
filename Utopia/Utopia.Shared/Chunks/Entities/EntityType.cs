namespace Utopia.Shared.Chunks.Entities
{
    /// <summary>
    /// Describes main entity types
    /// </summary>
    public enum EntityType : byte
    {
        /// <summary>
        /// Things that can be put into the inventory. Will float slightly in air up and down when thrown away from the inventary (tools, resources, Example: pixate, gold coins)
        /// </summary>
        Gear,
        /// <summary>
        /// Special type of the block, can be used to extend over 255 types of blocks (Example: glass cube with only one middle face )
        /// </summary>
        Block,
        /// <summary>
        /// Static things (Example: chairs, chests, tables, beds, doors)
        /// </summary>
        Static,
        /// <summary>
        /// Dynamic Entity (Player, mobs, ...) Everything that is living and can move in world
        /// </summary>
        Dynamic
    }
}
