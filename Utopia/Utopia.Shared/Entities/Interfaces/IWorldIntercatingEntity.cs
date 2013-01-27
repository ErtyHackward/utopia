using Utopia.Shared.Interfaces;

namespace Utopia.Shared.Entities.Interfaces
{
    public interface IWorldIntercatingEntity
    {
        /// <summary>
        /// Gets entityFactory, this field is injected
        /// </summary>
        EntityFactory EntityFactory { get; set; }

        /// <summary>
        /// Gets landscape manager, this field is injected
        /// </summary>
        ILandscapeManager2D LandscapeManager { get; set; }

        /// <summary>
        /// Gets dynamic entity manager, this field is injected
        /// </summary>
        IDynamicEntityManager DynamicEntityManager { get; set; }
    }
}
