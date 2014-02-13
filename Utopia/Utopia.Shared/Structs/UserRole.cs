namespace Utopia.Shared.Structs
{
    /// <summary>
    /// Enumerates all possible roles for the user
    /// </summary>
    public enum UserRole
    {
        /// <summary>
        /// Indicates that role is invalid
        /// </summary>
        Undefinded = 0,
        /// <summary>
        /// Default user rights
        /// </summary>
        Normal,
        /// <summary>
        /// User with higher credentials
        /// </summary>
        Moderator,
        /// <summary>
        /// Server administrator, all commands allowed
        /// </summary>
        Administrator
    }
}
