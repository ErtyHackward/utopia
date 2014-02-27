using Utopia.Shared.Services.Interfaces;
using Utopia.Shared.Structs;

namespace Utopia.Shared.Services
{
    public abstract class ModeratorCommand : IChatCommand, IRoleRestrictedCommand
    {
        public bool HasAccess(UserRole role)
        {
            return role == UserRole.Moderator;
        }

        public abstract string Id { get; }

        public abstract string Description { get; }
    }
}
