using Utopia.Shared.Structs;

namespace Utopia.Shared.Interfaces
{
    /// <summary>
    /// Describes users storage. For server usage
    /// </summary>
    public interface IUsersStorage
    {
        /// <summary>
        /// Tries to register user, or update his passwordHash or role 
        /// </summary>
        /// <param name="login">User login</param>
        /// <param name="passwordHash">user password</param>
        /// <param name="role">User role id</param>
        /// <returns>Returns true if register successfull otherwise false</returns>
        bool Register(string login, string passwordHash, UserRole role);

        /// <summary>
        /// Checks whether the specified user registered and password match
        /// </summary>
        /// <param name="login">User login</param>
        /// <param name="passwordHash">User SHA1 password hash</param>
        /// <param name="data">Filled login data structure if login succeed</param>
        /// <returns>true if login succeed otherwise false</returns>
        bool Login(string login, string passwordHash, out LoginData data);

        /// <summary>
        /// Sets corresponding data to a login. This function can be used to store any user specific information.
        /// </summary>
        /// <param name="login">User login</param>
        /// <param name="state">custom byte array</param>
        void SetData(string login, byte[] state);
    }
}