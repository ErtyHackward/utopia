namespace Utopia.Shared.Structs
{
    /// <summary>
    /// Enumerates all possible roles for the user
    /// Warning: the higher values means more power
    /// </summary>
    public enum UserRole
    {
        /// <summary>
        /// Indicates that role is invalid
        /// </summary>
        Undefinded = 0,
        /// <summary>
        /// Read only user, can not modify the world
        /// </summary>
        Guest = 1,
        /// <summary>
        /// User is allowed to modify the world
        /// </summary>
        Member = 2,
        /// <summary>
        /// User with higher credentials
        /// </summary>
        Moderator = 3,
        /// <summary>
        /// Server administrator, all commands allowed
        /// </summary>
        Administrator = 4
    }
}
