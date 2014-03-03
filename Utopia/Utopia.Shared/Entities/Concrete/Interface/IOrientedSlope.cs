namespace Utopia.Shared.Entities.Concrete.Interface
{
    public interface IOrientedSlope : IOrientedItem
    {
        /// <summary>
        /// Gets or sets value indicating if entity can climb on this entity by the angle of 45 degree
        /// </summary>
        bool IsOrientedSlope { get; set; }

        /// <summary>
        /// Get or Set value indicating of the slope is sliding
        /// </summary>
        bool IsSlidingSlope { get; set; }
    }
}
