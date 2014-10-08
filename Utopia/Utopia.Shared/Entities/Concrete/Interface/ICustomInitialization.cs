namespace Utopia.Shared.Entities.Concrete.Interface
{
    /// <summary>
    /// Allows entity to perform gameplay specific actions when created
    /// </summary>
    public interface ICustomInitialization
    {
        void Initialize(EntityFactory factory);
    }
}
