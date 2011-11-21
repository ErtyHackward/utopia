namespace Utopia.Shared.Structs
{
    /// <summary>
    /// Structure for user-related infomation to store in database
    /// </summary>
    public struct LoginData
    {
        /// <summary>
        /// User identification number
        /// </summary>
        public int UserId;
        /// <summary>
        /// User login
        /// </summary>
        public string Login;
        /// <summary>
        /// User role id
        /// </summary>
        public UserRole Role;        
        /// <summary>
        /// Binary-serialized user state
        /// </summary>
        public byte[] State;
    }
}