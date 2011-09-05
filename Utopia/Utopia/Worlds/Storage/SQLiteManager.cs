using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.SQLite;
using System.Data;
using System.IO;
using Utopia.Shared.World;

namespace Utopia.Worlds.Storage
{
    public abstract class SQLiteManager : IDisposable
    {

        #region Private Variables
        protected SQLiteConnection _connection = null;
        protected string _path;
        #endregion

        #region Public Variable
        #endregion

        public SQLiteManager(WorldParameters worldParameters, bool forceNew)
        {
            Init(worldParameters);
            if (forceNew && File.Exists(_path)) File.Delete(_path);
            GetConnection(); //Get a connection to The DB
        }

        #region Public Methods
        /// <summary>
        /// Returns active connection to SQLite database ()
        /// </summary>
        /// <returns></returns>
        public SQLiteConnection GetConnection()
        {
            if (_connection == null)
            {
                if (_path == ":memory:" || !File.Exists(_path))
                {
                    _connection = CreateDataBase(_path);
                    return _connection;
                }
                var csb = new SQLiteConnectionStringBuilder();
                csb.DataSource = _path;

                _connection = new SQLiteConnection(csb.ToString());
                _connection.Open();

            }
            return _connection;
        }

        public virtual void Dispose()
        {
            if (_connection != null)
                _connection.Dispose();
        }
        #endregion

        #region Private Methods
        private void Init(WorldParameters worldParameters)
        {
            //Check if the save directory does exist
            _path = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + @"\Utopia\Storage\Worlds\";
            if (!Directory.Exists(_path))
            {
                Directory.CreateDirectory(_path);
            }
            _path = Path.Combine(_path, @"Chunk_" + worldParameters.Seed + ".dat");
        }

        /// <summary>
        /// Creates database file and required tables
        /// </summary>
        /// <param name="path"></param>
        private SQLiteConnection CreateDataBase(string path)
        {
            _path = path;
            try
            {
                if (path != ":memory:")
                {
                    if (!System.IO.Directory.Exists(System.IO.Path.GetDirectoryName(path)))
                        System.IO.Directory.CreateDirectory(System.IO.Path.GetDirectoryName(path));
                    SQLiteConnection.CreateFile(path);
                }
                var conn = new SQLiteConnection("Data Source = " + _path);
                conn.Open();
                CreateDataBase(conn);
                return conn;
            }
            catch (Exception) { }
            return null;
        }

        protected abstract void CreateDataBase(SQLiteConnection conn);

        /// <summary>
        /// Escapes ' and " for SQLite query
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        protected string Escape(string input)
        {
            if (string.IsNullOrEmpty(input)) return "";
            return input.Replace("'", "''").Replace("\"", "\"\"");
        }
        #endregion

    }
}
