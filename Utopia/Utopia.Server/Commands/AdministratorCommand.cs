using Utopia.Server.Interfaces;
using Utopia.Server.Structs;
using Utopia.Shared.Structs;

namespace Utopia.Server.Commands
{
    /// <summary>
    /// Base administrator command class 
    /// </summary>
    public abstract class AdministratorCommand : IServerCommand, IRoleRestrictedCommand
    {
        public bool HasAccess(UserRole role)
        {
            return role == UserRole.Administrator;
        }

        public abstract string Id { get; }

        public abstract string Description { get; }
    }
}
