namespace Utopia.Shared
{
    /// <summary>
    /// Contains various runtime game variables for cross-state communications
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

        /// <summary>
        /// Gets selected server local address (for the case if connecting from the intranet)
        /// </summary>
        public string CurrentServerLocalAddress { get; set; }

        /// <summary>
        /// Logged user email
        /// </summary>
        public string Login { get; set; }

        /// <summary>
        /// Logged user password SHA1 hash
        /// </summary>
        public string PasswordHash { get; set; }

        /// <summary>
        /// Gets or sets user display name
        /// </summary>
        public string DisplayName { get; set; }

        /// <summary>
        /// Gets or sets user world cache database
        /// </summary>
        public string LocalDataBasePath { get; set; }

        /// <summary>
        /// Gets application common folder for data
        /// </summary>
        public string ApplicationDataPath { get; set; }
    }
}
