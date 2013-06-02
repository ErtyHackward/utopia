using Utopia.Shared.Structs;

namespace Utopia.Shared.Services.Interfaces
{
    /// <summary>
    /// Allows to restrict command usage to specified user group
    /// </summary>
    public interface IRoleRestrictedCommand
    {
        bool HasAccess(UserRole role);
    }
}
