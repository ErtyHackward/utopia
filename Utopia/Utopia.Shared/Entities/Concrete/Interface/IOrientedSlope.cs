namespace Utopia.Shared.Entities.Concrete.Interface
{
    public interface IOrientedSlope : IOrientedItem
    {
        /// <summary>
        /// Gets or sets value indicating if entity can climb on this entity by the angle of 45 degree
        /// </summary>
        bool IsOrientedSlope { get; set; }
    }
}
