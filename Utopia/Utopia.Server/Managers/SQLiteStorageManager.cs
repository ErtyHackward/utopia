using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using ProtoBuf;
using Utopia.Server.Interfaces;
using Utopia.Shared;
using Utopia.Shared.Entities;
using Utopia.Shared.Entities.Interfaces;
using Utopia.Shared.Entities.Models;
using Utopia.Shared.Interfaces;
using Utopia.Shared.Structs;
using S33M3Resources.Structs;
using Utopia.Shared.World;
using System.Data.SQLite;

namespace Utopia.Server.Managers
{
    /// <summary>
    /// Allows to store all required data in SQLite database
    /// </summary>
    public class SQLiteStorageManager : SQLiteStorage, IUsersStorage, IChunksStorage, IEntityStorage, IVoxelModelStorage
    {
        private readonly EntityFactory _factory;
        private SQLiteCommand _worldParametersInsertCmd;

        /// <summary>
        /// Returns database creation query 
        /// </summary>
        /// <returns></returns>
        protected override string CreateDataBase()
        {
            var dbCreate = new StringBuilder();

            dbCreate.Append(@"CREATE TABLE [chunks] ([X] integer NOT NULL, [Y] integer NOT NULL, [Z] integer NOT NULL, [data] blob NOT NULL, PRIMARY KEY(X,Y,Z)); ");
            dbCreate.Append(@"CREATE TABLE [users] ([id] integer PRIMARY KEY AUTOINCREMENT NOT NULL, [login] varchar(120) NOT NULL, [password] char(32) NOT NULL, [role] integer NOT NULL, [lastlogin] datetime NULL, [state] blob NULL); CREATE UNIQUE INDEX IDX_USERS_LOGIN on users (login);");
            dbCreate.Append(@"CREATE TABLE [entities] ([id] integer PRIMARY KEY NOT NULL, [data] blob NOT NULL);");
            dbCreate.Append(@"CREATE TABLE [models] ([id] varchar(120) PRIMARY KEY NOT NULL, [data] blob NOT NULL);");
            //dbCreate.Append(@"CREATE TABLE [worldparameters] ([name] varchar(120) PRIMARY KEY NOT NULL, [seed] varchar(120) NOT NULL, [configuration] blob NOT NULL);");
            dbCreate.Append(@"CREATE TABLE [worldparameters] ([name] varchar(120) PRIMARY KEY NOT NULL, [seed] varchar(120) NOT NULL, [state] blob NULL);");
            return dbCreate.ToString();
        }

        private void CreateQueryTemplates()
        {
            string SqlStatment;
            //Upsert a specific chunk
            //SqlStatment = "INSERT OR REPLACE INTO WorldParameters ([WorldName], [SeedName], [RealmConfiguration]) VALUES (@WorldName, @SeedName, @realmConfiguration)";
            SqlStatment = "INSERT OR REPLACE INTO worldparameters ([name], [seed]) VALUES (@name, @seed)";
            _worldParametersInsertCmd = new SQLiteCommand(SqlStatment, Connection);
            _worldParametersInsertCmd.Parameters.Add("@name", System.Data.DbType.String);
            _worldParametersInsertCmd.Parameters.Add("@seed", System.Data.DbType.String);
            //_worldParametersInsertCmd.Parameters.Add("@realmConfiguration", System.Data.DbType.Binary);
        }

        private void InsertWorldParametersData(WorldParameters worldParam, string filePath)
        {
            _worldParametersInsertCmd.Parameters[0].Value = worldParam.WorldName;
            _worldParametersInsertCmd.Parameters[1].Value = worldParam.SeedName;
            
            //Binary serialize the Configuration object into an array of byte[]
            //using (var ms = new MemoryStream())
            //{
            //    var writer = new BinaryWriter(ms);
            //    worldParam.Configuration.Save(writer);
            //    byte[] ConfigurationBytes = ms.ToArray();
            //    _worldParametersInsertCmd.Parameters[2].Value = ConfigurationBytes;
            //}

            //Save also the RealmConfiguration as binary file
            worldParam.Configuration.SaveToFile(Path.GetDirectoryName(filePath) + @"\" + Path.GetFileNameWithoutExtension(filePath) + ".realm");

            //Launch the insert
            _worldParametersInsertCmd.ExecuteNonQuery();
        }

        /// <summary>
        /// Puts block data into SQLite storage
        /// </summary>
        /// <param name="pos">Chunk position</param>
        /// <param name="data">Chunk data</param>
        public void SaveChunk(Vector3I pos, byte[] data)
        {
            InsertBlob(string.Format("INSERT OR REPLACE INTO chunks (X,Y,Z,data) VALUES ({0},{1},{2},@blob)", pos.X, pos.Y, pos.Z), data);
        }

        /// <summary>
        /// Fetch block data from the SQLite storage
        /// </summary>
        /// <param name="pos">Position of the block</param>
        /// <returns></returns>
        public byte[] LoadChunkData(Vector3I pos)
        {
            using (var reader = Query(string.Format("SELECT data FROM chunks WHERE X={0} AND Y={1} AND Z={2}", pos.X, pos.Y, pos.Z)))
            {
                reader.Read();
                return reader.IsDBNull(0) ? null : (byte[])reader.GetValue(0);
            }
        }

        //TODO : CRASH here while at menu state => Was not inactif !!!!
        /// <summary>
        /// Saves multiple chunks in one transaction
        /// </summary>
        /// <param name="positions">Array of chunks positions</param>
        /// <param name="blocksData">corresponding array of chunks data</param>
        public void SaveChunksData(Vector3I[] positions, byte[][] blocksData)
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
        public SQLiteStorageManager(string filePath, EntityFactory factory, WorldParameters worldParam)
            : base(filePath)
        {
            CreateQueryTemplates();

            if (_isDataBaseCreated)
            {
                //Create Database file with configuration information
                InsertWorldParametersData(worldParam, filePath);
            }

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

        public int GetUsersCount()
        {
            using (var reader = Query("SELECT count(*) FROM users"))
            {
                if (reader.Read())
                {
                    return reader.GetInt32(0);
                }
            }
            return 0;
        }

        /// <summary>
        /// Changes the role of the user
        /// </summary>
        /// <param name="login"></param>
        /// <param name="role"></param>
        public bool SetRole(string login, UserRole role)
        {
            return Execute(string.Format("UPDATE users SET role = {0} WHERE login = '{1}'", (int)role, login)) == 1;
        }

        public void SaveDynamicEntity(IDynamicEntity entity)
        {
            SaveEntity(entity.DynamicId, entity.ProtoSerialize());
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

        public void SaveState(GlobalState state)
        {
            InsertBlob("UPDATE worldparameters SET state = @blob", state.ProtoSerialize());
        }

        public GlobalState LoadState()
        {
            using (var reader = Query("SELECT state FROM worldparameters"))
            {
                reader.Read();
                return reader.IsDBNull(0) ? null : ((byte[])reader.GetValue(0)).Deserialize<GlobalState>();
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
        /// <param name="name"></param>
        /// <returns></returns>
        bool IVoxelModelStorage.Contains(string name)
        {
            CheckName(name);
            using (var reader = Query(string.Format("SELECT id FROM models WHERE id = '{0}'", name)))
            {
                return reader.HasRows;
            }
        }

        /// <summary>
        /// Loads a model form the storage
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        VoxelModel IVoxelModelStorage.Load(string name)
        {
            CheckName(name);
            using (var reader = Query(string.Format("SELECT data FROM models WHERE id='{0}'", name)))
            {
                reader.Read();
                var bytes = (byte[])reader.GetValue(0);
                return bytes.Deserialize<VoxelModel>();
            }
        }

        private void CheckName(string name)
        {
            if (string.IsNullOrEmpty(name))
                throw new ArgumentNullException("name", "Null or empty names are not allowed");

            if (name.Contains("'"))
                throw new FormatException("Model name could not contan a \"'\" symbol");
        }

        /// <summary>
        /// Saves a new model to the storage
        /// </summary>
        /// <param name="model"></param>
        void IVoxelModelStorage.Save(VoxelModel model)
        {
            CheckName(model.Name);
            using (var ms = new MemoryStream())
            {
                Serializer.Serialize(ms, model);
                var bytes = ms.ToArray();
                InsertBlob(string.Format("INSERT INTO models (id,data) VALUES ('{0}', @blob)", model.Name), bytes);
            }
        }

        /// <summary>
        /// Removes model from the storage
        /// </summary>
        /// <param name="name"></param>
        void IVoxelModelStorage.Delete(string name)
        {
            CheckName(name);
            Execute(string.Format("DELETE FROM models WHERE id = '{0}'", name));
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

        public override void Dispose()
        {
            _worldParametersInsertCmd.Dispose();
            base.Dispose();
        }
 
    }
}
