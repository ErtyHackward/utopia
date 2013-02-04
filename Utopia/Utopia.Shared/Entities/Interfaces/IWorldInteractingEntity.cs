using Utopia.Shared.Interfaces;

namespace Utopia.Shared.Entities.Interfaces
{
    public interface IWorldInteractingEntity
    {
        /// <summary>
        /// Gets entityFactory, this field is injected
        /// </summary>
        EntityFactory EntityFactory { get; set; }
    }
}
