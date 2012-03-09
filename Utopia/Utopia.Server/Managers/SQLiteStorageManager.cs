using System.Collections.Generic;
using System.IO;
using System.Text;
using Utopia.Shared;
using Utopia.Shared.Entities;
using Utopia.Shared.Entities.Interfaces;
using Utopia.Shared.Entities.Models;
using Utopia.Shared.Interfaces;
using Utopia.Shared.Structs;
using S33M3_Resources.Structs;

namespace Utopia.Server.Managers
{
    /// <summary>
    /// Allows to store all required data in SQLite database
    /// </summary>
    public class SQLiteStorageManager : SQLiteStorage, IUsersStorage, IChunksStorage, IEntityStorage, IVoxelModelStorage
    {
        private readonly EntityFactory _factory;

        /// <summary>
        /// Returns database creation query 
        /// </summary>
        /// <returns></returns>
        protected override string CreateDataBase()
        {
            var dbCreate = new StringBuilder();

            dbCreate.Append(@"CREATE TABLE [chunks] ([X] integer NOT NULL, [Y] integer NOT NULL,[data] blob NOT NULL, PRIMARY KEY(X,Y)); ");
            dbCreate.Append(@"CREATE TABLE [users] ([id] integer PRIMARY KEY AUTOINCREMENT NOT NULL, [login] varchar(120) NOT NULL, [password] char(32) NOT NULL, [role] integer NOT NULL, [lastlogin] datetime NULL, [state] blob NULL); CREATE UNIQUE INDEX IDX_USERS_LOGIN on users (login);");
            dbCreate.Append(@"CREATE TABLE [entities] ([id] integer PRIMARY KEY NOT NULL, [data] blob NOT NULL);");
            dbCreate.Append(@"CREATE TABLE [models] ([id] integer PRIMARY KEY NOT NULL, [data] blob NOT NULL);");
            
            return dbCreate.ToString();
        }

        /// <summary>
        /// Puts block data into SQLite storage
        /// </summary>
        /// <param name="pos">Chunk position</param>
        /// <param name="data">Chunk data</param>
        public void SaveChunk(Vector2I pos, byte[] data)
        {
            InsertBlob(string.Format("INSERT OR REPLACE INTO chunks (X,Y,data) VALUES ({0},{1},@blob)",pos.X,pos.Y), data);
        }

        /// <summary>
        /// Fetch block data from the SQLite storage
        /// </summary>
        /// <param name="pos">Position of the block</param>
        /// <returns></returns>
        public byte[] LoadChunkData(Vector2I pos)
        {
            using (var reader = Query(string.Format("SELECT data FROM chunks WHERE X={0} AND Y={1}", pos.X, pos.Y)))
            {
                reader.Read();
                return reader.IsDBNull(0) ? null : (byte[])reader.GetValue(0);
            }
        }

        /// <summary>
        /// Saves multiple chunks in one transaction
        /// </summary>
        /// <param name="positions">Array of chunks positions</param>
        /// <param name="blocksData">corresponding array of chunks data</param>
        public void SaveChunksData(Vector2I[] positions, byte[][] blocksData)
        {
            using (var trans = Connection.BeginTransaction())
            {
                for (int i = 0; i < positions.Length; i++)
                {
                    SaveChunk(positions[i], blocksData[i]);
                }
                trans.Commit();
            }
        }

        /// <summary>
        /// Creates new instance of SQLite storage manager
        /// </summary>
        /// <param name="filePath"></param>
        /// <param name="factory"></param>
        public SQLiteStorageManager(string filePath, EntityFactory factory) : base(filePath)
        {
            _factory = factory;
        }

        /// <summary>
        /// Tries to register user in database
        /// </summary>
        /// <param name="login">User login</param>
        /// <param name="passwordHash">user password</param>
        /// <param name="role">User role id</param>
        /// <returns>Returns true if register successfull otherwise false</returns>
        public bool Register(string login, string passwordHash, UserRole role)
        {
            if (string.IsNullOrEmpty(login)) return false;
            if (string.IsNullOrEmpty(passwordHash)) return false;

            int userId = 0;

            using (var reader = Query(string.Format("SELECT id FROM users WHERE login = '{0}'", Escape(login))))
            {
                if (reader.Read())
                {
                    userId = reader.GetInt32(0);
                }
            }

            if (userId > 0)
            {
                return 1 == Execute(string.Format("UPDATE users SET password = '{0}', role = {1} WHERE login = '{2}'", passwordHash, (int)role, Escape(login)));
            }
            
            return 1 == Execute(string.Format("INSERT INTO users (login, password, role) VALUES ('{0}', '{1}', {2})", Escape(login), Escape(passwordHash), (int)role));
        }

        /// <summary>
        /// Checks whether the specified user registered and password match
        /// </summary>
        /// <param name="login">User login</param>
        /// <param name="passwordHash">User md5 password hash</param>
        /// <param name="data">Filled login data structure if login succeed</param>
        /// <returns>true if login succeed otherwise false</returns>
        public bool Login(string login, string passwordHash, out LoginData data)
        {
            using (var reader = Query(string.Format("SELECT id, role, state FROM users WHERE login = '{0}' AND password = '{1}'", login, passwordHash)))
            {
                data = new LoginData();

                if (!reader.HasRows)
                    return false;

                if (!reader.Read())
                    return false;

                LoginData loginInfo;

                loginInfo.UserId = reader.GetInt32(0);
                loginInfo.Role = (UserRole)reader.GetInt32(1);
                loginInfo.Login = login;
                loginInfo.State = reader.IsDBNull(2) ? null : (byte[])reader.GetValue(2);
                
                data = loginInfo;
                return true;
            }
        }

        /// <summary>
        /// Sets corresponding data to login. This function can be used to store any user specific information.
        /// </summary>
        /// <param name="login">User login</param>
        /// <param name="state">custom byte array</param>
        public void SetData(string login, byte[] state)
        {
            InsertBlob(string.Format("UPDATE users SET state = @blob WHERE login = '{0}'", Escape(login)), state);
        }

        public void SaveDynamicEntity(IDynamicEntity entity)
        {
            byte[] bytes;
            using (var ms = new MemoryStream())
            {
                var writer = new BinaryWriter(ms);
                entity.Save(writer);
                bytes = ms.ToArray();
            }

            SaveEntity(entity.DynamicId, bytes);
        }

        public void SaveEntity(uint entityId, byte[] bytes)
        {
            InsertBlob(string.Format("INSERT OR REPLACE INTO entities (id,data) VALUES ('{0}', @blob)", entityId), bytes);
        }

        public IEntity LoadEntity(uint entityId)
        {
            var data = LoadEntityBytes(entityId);
            return _factory.CreateFromBytes(data);
        }

        public byte[] LoadEntityBytes(uint entityId)
        {
            using (var reader = Query(string.Format("SELECT data FROM entities WHERE id={0}", entityId)))
            {
                reader.Read();
                return reader.IsDBNull(0) ? null : (byte[])reader.GetValue(0);
            }
        }

        public uint GetMaximumId()
        {
            using (var reader = Query("SELECT MAX(id) FROM entities"))
            {
                if (reader.Read())
                {
                    if (reader.IsDBNull(0)) return 0;
                    var maxNumber = reader.GetInt64(0);
                    return (uint) maxNumber;
                } return 0;
            }
        }

        /// <summary>
        /// Indicates if the storage contains a model with hash specified
        /// </summary>
        /// <param name="hash"></param>
        /// <returns></returns>
        bool IVoxelModelStorage.Contains(Md5Hash hash)
        {
            using (var reader = Query(string.Format("SELECT id FROM models WHERE id = {0}", hash.GetHashCode())))
            {
                return reader.HasRows;
            }
        }

        /// <summary>
        /// Loads a model form the storage
        /// </summary>
        /// <param name="hash"></param>
        /// <returns></returns>
        VoxelModel IVoxelModelStorage.Load(Md5Hash hash)
        {
            using (var reader = Query(string.Format("SELECT data FROM models WHERE id={0}", hash.GetHashCode())))
            {
                reader.Read();
                var bytes = (byte[])reader.GetValue(0);
                return bytes.Deserialize<VoxelModel>();
            }
        }

        /// <summary>
        /// Saves a new model to the storage
        /// </summary>
        /// <param name="model"></param>
        void IVoxelModelStorage.Save(VoxelModel model)
        {
            var bytes = model.Serialize();

            InsertBlob(string.Format("INSERT INTO models (id,data) VALUES ({0}, @blob)", model.Hash.GetHashCode()), bytes);
        }

        /// <summary>
        /// Removes model from the storage
        /// </summary>
        /// <param name="hash"></param>
        void IVoxelModelStorage.Delete(Md5Hash hash)
        {
            Execute(string.Format("DELETE FROM models WHERE id = {0}", hash.GetHashCode()));
        }

        /// <summary>
        /// Allows to fetch all models
        /// </summary>
        /// <returns></returns>
        IEnumerable<VoxelModel> IVoxelModelStorage.Enumerate()
        {
            using (var reader = Query("SELECT data FROM models"))
            {
                while (reader.Read())
                {
                    yield return ((byte[])reader.GetValue(0)).Deserialize<VoxelModel>();
                }
            }
        }
    }
}
