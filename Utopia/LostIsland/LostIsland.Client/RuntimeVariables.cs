namespace LostIsland.Client
{
    /// <summary>
    /// Contains various runtime game variables
    /// </summary>
    public class RuntimeVariables
    {
        /// <summary>
        /// Indicates if game should be started in single player mode
        /// </summary>
        public bool SinglePlayer { get; set; }

        /// <summary>
        /// Gets selected server address
        /// </summary>
        public string CurrentServerAddress { get; set; }
    }
}
