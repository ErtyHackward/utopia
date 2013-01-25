using Sandbox.Client.Components;
using Utopia.Shared;

namespace Sandbox.Client
{
    /// <summary>
    /// Contains various runtime game variables
    /// </summary>
    public class SandboxRuntimeVariables : RuntimeVariables
    {
        /// <summary>
        /// Single player server instance wrapper (can be null)
        /// </summary>
        public LocalServer LocalServer { get; set; }
    }
}
