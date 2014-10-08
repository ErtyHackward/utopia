using Realms.Client.Components;
using Utopia.Shared;

namespace Realms.Client
{
    /// <summary>
    /// Contains various runtime game variables for cross-state communications
    /// </summary>
    public class RealmRuntimeVariables : RuntimeVariables
    {
        /// <summary>
        /// Single player server instance wrapper (can be null)
        /// </summary>
        public LocalServer LocalServer { get; set; }

        /// <summary>
        /// Indicates that main menu state should perform dispose of gameplay components and create new game scope
        /// </summary>
        public bool DisposeGameComponents { get; set; }

        /// <summary>
        /// This message will be displayed when the user will exit the gameplay mode to the main menu
        /// Usefull to show error details
        /// </summary>
        public string MessageOnExit { get; set; }
    }
}
