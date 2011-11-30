using System.Collections.Generic;
using S33M3Engines.D3D;

namespace Utopia
{
    /// <summary>
    /// Base class for game state. Each state contains a list of components that should be active
    /// </summary>
    public abstract class GameState
    {
        /// <summary>
        /// Name of the state
        /// </summary>
        public abstract string Name { get; }
        /// <summary>
        /// List of components that should be enabled
        /// </summary>
        public List<GameComponent> EnabledComponents { get; protected set; }

        /// <summary>
        /// List of components that should be visible
        /// </summary>
        public List<DrawableGameComponent> VisibleComponents { get; protected set; }

        /// <summary>
        /// Gets states manager
        /// </summary>
        public StatesManager StatesManager { get; internal set; }

        protected GameState()
        {
            EnabledComponents = new List<GameComponent>();
            VisibleComponents = new List<DrawableGameComponent>();
        }

        /// <summary>
        /// Called when componend get initialized. This is a place where you can perform custom actions
        /// </summary>
        /// <param name="previousState">Previous game state or null</param>
        public virtual void OnEnabled(GameState previousState)
        {

        }

        /// <summary>
        /// Called when componend get disabled. This is a place where you can perform custom actions
        /// </summary>
        /// <param name="nextState">Next game state or null</param>
        public virtual void OnDisabled(GameState nextState)
        {

        }
    }
}