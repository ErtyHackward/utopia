using System;
using System.Data;
using System.Data.SQLite;
using System.IO;
using Utopia.Shared.Entities;
using Utopia.Shared.Entities.Interfaces;
using Utopia.Shared.Interfaces;
using Utopia.Shared.Structs;

namespace Utopia.Server.Managers
{
    /// <summary>
    /// Allows to store all required data in SQLite database
    /// </summary>
    public class SQLiteStorageManager : IUsersStorage, IChunksStorage, IEntityStorage
    {
        /// <summary>
        /// Creates database file and required tables
        /// </summary>
        /// <param name="path"></param>
        public SQLiteConnection CreateDataBase(string path)
        {
            Path = path;
            try
            {
                if (path != ":memory:")
                {
                    if (!System.IO.Directory.Exists(System.IO.Path.GetDirectoryName(path)))
                        System.IO.Directory.CreateDirectory(System.IO.Path.GetDirectoryName(path));
                    SQLiteConnection.CreateFile(path);
                }
                var conn = new SQLiteConnection("Data Source = " + Path);
                conn.Open();
                CreateDataBase(conn);
                return conn;
            }
            catch (Exception) { }
            return null;
        }

        /// <summary>
        /// Gets or sets database system path
        /// </summary>
        public string Path
        {
            get; private set;
        }

        private void CreateDataBase(SQLiteConnection conn)
        {
            var command = conn.CreateCommand();
            command.CommandText = @"CREATE TABLE [chunks] ([X] integer NOT NULL, [Y] integer NOT NULL,[data] blob NOT NULL, PRIMARY KEY(X,Y)); ";
            command.CommandText += @"CREATE TABLE [users] ([id] integer PRIMARY KEY AUTOINCREMENT NOT NULL, [login] varchar(120) NOT NULL, [password] char(32) NOT NULL, [role] integer NOT NULL, [lastlogin] datetime NULL, [state] blob NULL); CREATE INDEX IDX_USERS_LOGIN on users (login);";
            command.CommandText += @"CREATE TABLE [entities] ([id] integer PRIMARY KEY NOT NULL, [data] blob NOT NULL);";
            command.CommandType = CommandType.Text;
            command.ExecuteNonQuery();
        }

        private SQLiteConnection connection = null;
        /// <summary>
        /// Returns active connection to SQLite database ()
        /// </summary>
        /// <returns></returns>
        public SQLiteConnection GetConnection()
        {
            if (connection == null)
            {
                if (Path == ":memory:" || !File.Exists(Path))
                {
                    connection = CreateDataBase(Path);
                    return connection;
                }
                var csb = new SQLiteConnectionStringBuilder();
                csb.DataSource = Path;
                
                connection = new SQLiteConnection(csb.ToString());
                connection.Open();
                
            }
            return connection;
        }

        public void Dispose()
        {
            if (connection != null)
                connection.Dispose();
        }


        /// <summary>
        /// Executes query and returns reader to get result
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        public SQLiteDataReader Query(string query)
        {
            lock (this)
            {
                SQLiteDataReader reader = null;
                try
                {

                    using (var cmd = GetConnection().CreateCommand())
                    {
                        cmd.CommandText = query;
                        reader = cmd.ExecuteReader();
                    }
                }
                catch { return null; }
                return reader;
            }
        }

        /// <summary>
        /// Executes query and returns number of rows affected
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        public int Execute(string query)
        {
            return Execute(query, GetConnection(), false);
        }

        public int Execute(string query, bool delayWrite)
        {
            return Execute(query, GetConnection(), delayWrite);
        }

        public int Execute(string query, SQLiteConnection conn, bool delayWrite)
        {
            lock (this)
            {
                int val = 0;
                try
                {
                    using (var cmd = conn.CreateCommand())
                    {
                        cmd.CommandText = query;
                        val = cmd.ExecuteNonQuery();
                    }
                }
                catch
                {
                    return -1;
                }
                return val;
            }
        }

        /// <summary>
        /// Escapes ' and " for SQLite query
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        private string Escape(string input)
        {
            if (string.IsNullOrEmpty(input)) return "";
            return input.Replace("'", "''").Replace("\"", "\"\"");
        }

        /// <summary>
        /// Puts block data into SQLite storage
        /// </summary>
        /// <param name="pos">Chunk position</param>
        /// <param name="data">Chunk data</param>
        public void SaveChunk(Vector2I pos, byte[] data)
        {
            SaveChunk(pos, data, GetConnection());
        }

        private void SaveChunk(Vector2I pos, byte[] data, SQLiteConnection conn)
        {
            lock (this)
            {
                try
                {
                    int val = 0;
                    using (var cmd = conn.CreateCommand())
                    {
                        cmd.CommandText = string.Format("INSERT OR REPLACE INTO chunks (X,Y,data) VALUES ({0},{1},@data)",pos.X,pos.Y);
                        SQLiteParameter param = cmd.CreateParameter();
                        param.DbType = DbType.Binary;
                        param.ParameterName = "@data";
                        param.Size = data.Length;
                        param.Value = data;
                        cmd.Parameters.Add(param);
                        val = cmd.ExecuteNonQuery();
                    }
                }
                catch (Exception) { }
                return;
            }
        }

        /// <summary>
        /// Fetch block data from the SQLite storage
        /// </summary>
        /// <param name="pos">Position of the block</param>
        /// <returns></returns>
        public byte[] LoadChunkData(Vector2I pos)
        {
            lock (this)
            {
                byte[] data;
                using (var cmd = GetConnection().CreateCommand())
                {
                    cmd.CommandText = string.Format("SELECT data FROM chunks WHERE X={0} AND Y={1}", pos.X, pos.Y);
                    data = (byte[])cmd.ExecuteScalar();
                }
                return data;
            }
        }

        /// <summary>
        /// Saves multiple chunks in one transaction
        /// </summary>
        /// <param name="positions">Array of chunks positions</param>
        /// <param name="blocksData">corresponding array of chunks data</param>
        public void SaveChunksData(Vector2I[] positions, byte[][] blocksData)
        {
            using (var trans = GetConnection().BeginTransaction())
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
        public SQLiteStorageManager(string filePath)
        {
            Path = filePath;
            

            GetConnection();
        }

        /// <summary>
        /// Tries to register user in database
        /// </summary>
        /// <param name="login">User login</param>
        /// <param name="passwordHash">user password</param>
        /// <param name="role">User role id</param>
        /// <returns>Returns true if register successfull otherwise false</returns>
        public bool Register(string login, string passwordHash, int role)
        {
            if (string.IsNullOrEmpty(login)) return false;
            if (string.IsNullOrEmpty(passwordHash)) return false;

            if (IsRegistered(login))
                return false;

            return 1 == Execute(string.Format("INSERT INTO users (login, password, role) VALUES ('{0}', '{1}', {2})", Escape(login), Escape(passwordHash), role));
        }

        /// <summary>
        /// Checks whether the specified login registered
        /// </summary>
        /// <param name="login"></param>
        /// <returns></returns>
        public bool IsRegistered(string login)
        {
            using (var reader = Query(string.Format("SELECT id FROM users WHERE login = '{0}'", Escape(login))))
            {
                return reader.HasRows;
            }
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
                loginInfo.Role = reader.GetInt32(1);
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
            try
            {
                using (var cmd = GetConnection().CreateCommand())
                {
                    cmd.CommandText = string.Format("UPDATE users SET state = @data WHERE login = '{0}'", Escape(login));
                    var param = cmd.CreateParameter();
                    param.DbType = DbType.Binary;
                    param.ParameterName = "@data";
                    param.Size = state.Length;
                    param.Value = state;
                    cmd.Parameters.Add(param);
                    cmd.ExecuteNonQuery();
                }
            }
            catch (Exception) { }
        }

        public void SaveEntity(IEntity entity)
        {
            byte[] bytes;
            using (var ms = new MemoryStream())
            {
                var writer = new BinaryWriter(ms);
                entity.Save(writer);
                bytes = ms.ToArray();
            }

            SaveEntity(entity.EntityId, bytes);
        }

        public void SaveEntity(uint entityId, byte[] bytes)
        {
            using (var cmd = GetConnection().CreateCommand())
            {
                cmd.CommandText = string.Format("INSERT OR REPLACE INTO entities (id,data) VALUES ('{0}', @data)", entityId);
                var param = cmd.CreateParameter();
                param.DbType = DbType.Binary;
                param.ParameterName = "@data";
                param.Size = bytes.Length;
                param.Value = bytes;
                cmd.Parameters.Add(param);
                cmd.ExecuteNonQuery();
            }
        }

        public IEntity LoadEntity(uint entityId)
        {
            byte[] data = LoadEntityBytes(entityId);
            return EntityFactory.Instance.CreateFromBytes(data);
        }

        public byte[] LoadEntityBytes(uint entityId)
        {
            byte[] data;
            using (var cmd = GetConnection().CreateCommand())
            {
                cmd.CommandText = string.Format("SELECT data FROM entities WHERE id={0}", entityId);
                data = (byte[])cmd.ExecuteScalar();
            }
            return data;
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
    }
}
