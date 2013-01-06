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

        /// <summary>
        /// Performs actions to rollback preliminary made actions on the client side
        /// </summary>
        /// <param name="impact"></param>
        void Rollback(IToolImpact impact);
    }
}