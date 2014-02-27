namespace Utopia.Shared.Interfaces
{
    /// <summary>
    /// Allows to store key/value pairs with different data
    /// </summary>
    public interface ICustomStorage
    {
        void SetVariable<T>(string id, T value);
        T GetVariable<T>(string id, T defaultValue = default(T));
    }
}