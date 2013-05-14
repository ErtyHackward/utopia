﻿using S33M3CoreComponents.Cameras.Interfaces;
using S33M3DXEngine.Main.Interfaces;
using Utopia.Shared.Entities;
using Utopia.Shared.Entities.Interfaces;
using Utopia.Shared.Structs;

namespace Utopia.Entities.Managers.Interfaces
{
    /// <summary>
    /// Provides player current focus location
    /// This could be a character or just point in the world (god mode)
    /// </summary>
    public interface IPlayerManager : ICameraPlugin, IGameComponent
    {
        /// <summary>
        /// Gets main player entity (character or PlayerFocusEntity)
        /// </summary>
        IDynamicEntity Player { get; }

        /// <summary>
        /// Gets player's faction
        /// </summary>
        Faction Faction { get; }

        /// <summary>
        /// If camera is inside water
        /// </summary>
        bool IsHeadInsideWater { get; }

        /// <summary>
        /// Gets active player tool or null
        /// Affects picking algo
        /// </summary>
        IItem ActiveTool { get; }
    }
}
