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
        /// Indicates if tool should be used multiple times util button is released
        /// </summary>
        bool RepeatedActionsAllowed { get; set; }
    }
}