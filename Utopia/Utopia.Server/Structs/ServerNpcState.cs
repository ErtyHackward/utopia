﻿namespace Utopia.Server.Structs
{
    public enum ServerNpcState
    {
        /// <summary>
        /// Means that npc have nothing to do
        /// </summary>
        Idle,
        /// <summary>
        /// trying to take an item or use it
        /// the item will be blocked for other entities
        /// </summary>
        UsingItem,
        /// <summary>
        /// trying to use the block, it will be blocked for other entities
        /// generally this means that npc want to remove that block
        /// </summary>
        UsingBlock,
        /// <summary>
        /// Following other npc
        /// </summary>
        Following
    }
}
