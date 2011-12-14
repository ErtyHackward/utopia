﻿using System;
using LostIsland.Shared.Web;
using Utopia.Server.Managers;
using Utopia.Shared.Interfaces;
using Utopia.Shared.Structs.Helpers;

namespace LostIsland.Server
{
    /// <summary>
    /// Provides common login system
    /// </summary>
    public class ServerUsersStorage : IUsersStorage
    {
        private readonly SQLiteStorageManager _storage;
        private readonly ServerWebApi _webApi;

        public ServerUsersStorage(SQLiteStorageManager storage, ServerWebApi webApi)
        {
            if (storage == null) throw new ArgumentNullException("storage");
            if (webApi == null) throw new ArgumentNullException("webApi");
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
        public bool Login(string login, string passwordHash, out Utopia.Shared.Structs.LoginData data)
        {
            // check if we have this user in our local database
            if (_storage.Login(login, passwordHash, out data))
                return true;

            try
            {
                // we need to authenticate user from gloabal server
                var responce = _webApi.UserAuthenticate(login, passwordHash);

                if (responce != null && responce.Valid)
                {
                    // create or update local registrantion 
                    return _storage.Register(login, passwordHash, Utopia.Shared.Structs.UserRole.Guest);
                }

                return false;
            }
            catch (Exception x)
            {
                TraceHelper.Write("Authentication exception of {0}: {1}", login, x.Message);
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
    }
}
