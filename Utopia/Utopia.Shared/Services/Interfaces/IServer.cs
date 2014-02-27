using System;
using Utopia.Shared.Entities;
using Utopia.Shared.Entities.Interfaces;
using Utopia.Shared.Interfaces;
using Utopia.Shared.World;

namespace Utopia.Shared.Services.Interfaces
{
    /// <summary>
    /// Represents utopia server 
    /// It is a root class that provides access to server functions
    /// </summary>
    public interface IServer : IDisposable
    {
        /// <summary>
        /// Gets or sets an entity factory
        /// </summary>
        EntityFactory EntityFactory { get; }

        WorldParameters WorldParameters { get; }

        IAreaManager AreaManager { get; }
        
        /// <summary>
        /// Gets object responsible for player chat commands
        /// </summary>
        ICommandsManager CommandsManager { get; }

        IGlobalStateManager GlobalStateManager { get; }

        IServerLandscapeManager LandscapeManager { get; }

        IChatManager ChatManager { get; }

        /// <summary>
        /// Provides storage for various data (key value pairs)
        /// </summary>
        ICustomStorage CustomStorage { get; }
    }
}