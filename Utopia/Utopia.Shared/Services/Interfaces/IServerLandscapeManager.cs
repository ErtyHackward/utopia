using System;
using Utopia.Shared.Interfaces;

namespace Utopia.Shared.Services.Interfaces
{
    public interface IServerLandscapeManager : ILandscapeManager
    {
        event EventHandler<ServerLandscapeManagerBlockChangedEventArgs> BlockChanged;
    }
}