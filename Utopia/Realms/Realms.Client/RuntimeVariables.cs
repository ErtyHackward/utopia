using Realms.Client.Components;
using Utopia.Shared;

namespace Realms.Client
{
    /// <summary>
    /// Contains various runtime game variables
    /// </summary>
    public class RealmRuntimeVariables : RuntimeVariables
    {
        /// <summary>
        /// Single player server instance wrapper (can be null)
        /// </summary>
        public LocalServer LocalServer { get; set; }
    }
}
