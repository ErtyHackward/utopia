using System;
using Utopia.Shared.Structs;

namespace Utopia.Shared.Interfaces
{
    /// <summary>
    /// Describes users storage. For server usage
    /// </summary>
    public interface IUsersStorage
    {
        /// <summary>
        /// Gets or sets new user role (except first one)
        /// </summary>
        UserRole DefaultRole { get; set; }

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

        /// <summary>
        /// Returns current cached user accounts
        /// </summary>
        /// <returns></returns>
        int GetUsersCount();

        /// <summary>
        /// Changes the role of the user
        /// </summary>
        /// <param name="login"></param>
        /// <param name="role"></param>
        bool SetRole(string login, UserRole role);

        /// <summary>
        /// Returns specified user role
        /// </summary>
        /// <param name="login"></param>
        /// <returns></returns>
        UserRole GetRole(string login);

        void AddBan(string login, TimeSpan time);

        bool IsBanned(string login, out TimeSpan timeLeft);

    }
}