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
        /// User that have no registration yet
        /// </summary>
        Guest,
        /// <summary>
        /// Normal ordinary registered user
        /// </summary>
        Registered,
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
