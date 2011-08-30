using System;
using Utopia.Shared.Chunks.Entities.Events;
using Utopia.Shared.Chunks.Entities.Management;

namespace Utopia.Shared.Chunks.Entities.Interfaces
{
    public interface IDynamicEntity : IEntity
    {
        /// <summary>
        /// Occurs when entity changes its position
        /// </summary>
        event EventHandler<EntityMoveEventArgs> PositionChanged;

        /// <summary>
        /// Perform actions when getting into the area
        /// </summary>
        /// <param name="area"></param>
        void AreaEnter(MapArea area);

        /// <summary>
        /// Perform actions when leaving the area
        /// </summary>
        /// <param name="area"></param>
        void AreaLeave(MapArea area);

        /// <summary>
        /// Perform dynamic update (AI logic)
        /// </summary>
        void Update(DateTime gameTime);
    }
}