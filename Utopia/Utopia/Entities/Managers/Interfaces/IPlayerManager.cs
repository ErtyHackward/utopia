using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using S33M3CoreComponents.Cameras.Interfaces;
using S33M3DXEngine.Main.Interfaces;
using S33M3Resources.Structs;
using Utopia.Shared.Entities.Interfaces;

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
        /// If camera is inside water
        /// </summary>
        bool IsHeadInsideWater { get; }
    }
}
