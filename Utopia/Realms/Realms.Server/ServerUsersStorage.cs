using System;
using Utopia.Shared.Interfaces;
using Utopia.Shared.Net.Web;
using Utopia.Shared.Server.Managers;
using Utopia.Shared.Structs;

namespace Realms.Server
{
    /// <summary>
    /// Provides common login system
    /// </summary>
    public class ServerUsersStorage : IUsersStorage
    {
        private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        private readonly SqliteStorageManager _storage;
        private readonly ServerWebApi _webApi;

        public UserRole DefaultRole { get; set; }

        public ServerUsersStorage(SqliteStorageManager storage, ServerWebApi webApi)
        {
            if (storage == null) 
                throw new ArgumentNullException("storage");
            if (webApi == null) 
                throw new ArgumentNullException("webApi");

            DefaultRole = UserRole.Guest;

            _storage = storage;
            _webApi = webApi;
        }

        /// <summary>
        /// Direct registrantion on the server is not supported, always throws an NotSupportedException()
        /// </summary>
        /// <param name="login"></param>
        /// <param name="passwordHash"></param>
        /// <param name="role"></param>
        /// <returns></returns>
        public bool Register(string login, string passwordHash, Utopia.Shared.Structs.UserRole role)
        {
            throw new NotSupportedException();
        }
        
        /// <summary>
        /// Checks if user registered on local database, request a login from central server if not.
        /// </summary>
        /// <param name="login">User login</param>
        /// <param name="passwordHash">User SHA1 password hash</param>
        /// <param name="data">Filled login data structure if login succeed</param>
        /// <returns>true if login succeed otherwise false</returns>
        public bool Login(string login, string passwordHash, out LoginData data)
        {
            // check if we have this user in our local database
            if (_storage.Login(login, passwordHash, out data))
                return true;

            try
            {
                // we need to authenticate user from global server
                var responce = _webApi.UserAuthenticate(login, passwordHash);
                
                if (responce != null && responce.Valid)
                {
                    var role = DefaultRole;

                    if (_storage.GetUsersCount() == 0)
                        role = UserRole.Administrator;
                    
                    logger.Info("Creating new user record {0} {1}", login, role);

                    // create or update local registration 
                    _storage.Register(login, passwordHash, role);
                    return _storage.Login(login, passwordHash, out data);
                }

                return false;
            }
            catch (Exception x)
            {
                logger.Error("Authentication exception of {0}: {1}", login, x.Message);
                return false;
            }
        }
        
        /// <summary>
        /// Sets corresponding data to login. This function can be used to store any user specific information.
        /// </summary>
        /// <param name="login">User login</param>
        /// <param name="state">custom byte array</param>
        public void SetData(string login, byte[] state)
        {
            _storage.SetData(login, state);
        }

        public int GetUsersCount()
        {
            return _storage.GetUsersCount();
        }

        public bool SetRole(string login, UserRole role)
        {
            return _storage.SetRole(login, role);
        }

        public UserRole GetRole(string login)
        {
            return _storage.GetRole(login);
        }

        public void AddBan(string login, TimeSpan time)
        {
            _storage.AddBan(login, time);
        }

        public bool IsBanned(string login, out TimeSpan timeLeft)
        {
            return _storage.IsBanned(login, out timeLeft);
        }
    }
}
