namespace Utopia.Shared.Entities.Interfaces
{
    public interface ITool : IItem
    {
        /// <summary>
        /// Performs tool business logic
        /// </summary>
        /// <param name="owner"></param>
        /// <returns></returns>
        IToolImpact Use(IDynamicEntity owner);
    }
}