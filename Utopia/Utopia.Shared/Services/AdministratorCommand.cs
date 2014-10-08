using Utopia.Shared.Services.Interfaces;
using Utopia.Shared.Structs;

namespace Utopia.Shared.Services
{
    /// <summary>
    /// Base administrator command class 
    /// </summary>
    public abstract class AdministratorCommand : IChatCommand, IRoleRestrictedCommand
    {
        public bool HasAccess(UserRole role)
        {
            return role == UserRole.Administrator;
        }

        public abstract string Id { get; }

        public abstract string Description { get; }
    }
}
