﻿using System;
using System.Data;
using System.Data.SQLite;
using System.IO;

namespace Utopia.Shared
{
    /// <summary>
    /// Base class for storage using SQLite database
    /// </summary>
    public abstract class SQLiteStorage
    {
        private readonly SQLiteConnection _connection;
        private readonly string _path;
        private readonly object _syncRoot = new object();

        /// <summary>
        /// Gets database path
        /// </summary>
        public string DatabasePath
        {
            get { return _path; }
        }

        /// <summary>
        /// Gets an object for lock syncronization
        /// </summary>
        public object SyncRoot
        {
            get { return _syncRoot; }
        }

        /// <summary>
        /// Gets current sqlite connection
        /// </summary>
        public SQLiteConnection Connection
        {
            get { return _connection; }
        }

        /// <summary>
        /// Create a new db connection, creates file if not exists, use ":memory:" fileName to create a memory database
        /// </summary>
        /// <param name="fileName">Full path for database file or ":memory:" value</param>
        /// <param name="wipeDatabase"></param>
        protected SQLiteStorage(string fileName, bool wipeDatabase = false)
        {
            _path = fileName;

            var createDb = false;

            if (_path != ":memory:")
            {
                if (wipeDatabase && File.Exists(fileName))
                    File.Delete(fileName);

                if (!File.Exists(fileName))
                {
                    // create db directory
                    var dirName = Path.GetDirectoryName(_path);

                    if (!string.IsNullOrEmpty(dirName) && !Directory.Exists(dirName))
                        Directory.CreateDirectory(dirName);

                    // and db file
                    SQLiteConnection.CreateFile(_path);
                    createDb = true;
                }
            }
            else createDb = true;

            var csb = new SQLiteConnectionStringBuilder {
                SyncMode = SynchronizationModes.Normal, 
                DataSource = _path
            };
            
            _connection = new SQLiteConnection(csb + @";COMPRESS=TRUE");
            _connection.Open();

            Execute("PRAGMA LOCKING_MODE=EXCLUSIVE;PRAGMA JOURNAL_MODE=WAL;");

            if (createDb) CreateDataBaseInternal();
        }

        public virtual void Dispose()
        {
            if (_connection != null)
                _connection.Dispose();
        }

        /// <summary>
        /// Creates database file and required tables
        /// </summary>
        private void CreateDataBaseInternal()
        {
            Execute(CreateDataBase());
        }

        /// <summary>
        /// Returns database creation query 
        /// </summary>
        /// <returns></returns>
        protected abstract string CreateDataBase();

        /// <summary>
        /// Escapes ' and " for SQLite query
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public string Escape(string input)
        {
            if (string.IsNullOrEmpty(input)) return "";
            return input.Replace("'", "''").Replace("\"", "\"\"");
        }

        /// <summary>
        /// Executes query and returns reader to get result, dispose it after using
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        public SQLiteDataReader Query(string query)
        {
            lock (_syncRoot)
            {
                try
                {
                    using (var cmd = _connection.CreateCommand())
                    {
                        cmd.CommandText = query;
                        return cmd.ExecuteReader();
                    }
                }
                catch { 
                    return null; 
                }
            }
        }

        public SQLiteDataReader Query(string format, params object[] pars)
        {
            return Query(string.Format(format, pars));
        }

        /// <summary>
        /// Use this function to insert a blob value to the db, call the parameret @blob, Example: INSERT INTO images (name, data) VALUES ("img.png", @blob)
        /// </summary>
        /// <param name="query"></param>
        /// <param name="blob"></param>
        /// <returns></returns>
        public int InsertBlob(string query, byte[] blob)
        {
            lock (_syncRoot)
            {
                using (var cmd = _connection.CreateCommand())
                {
                    cmd.CommandText = query;
                    var param = cmd.CreateParameter();
                    param.DbType = DbType.Binary;
                    param.ParameterName = "@blob";
                    param.Size = blob.Length;
                    param.Value = blob;
                    cmd.Parameters.Add(param);
                    return cmd.ExecuteNonQuery();
                }
            }
        }

        /// <summary>
        /// Executes query and returns number of rows affected
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        public int Execute(string query)
        {
            lock (_syncRoot)
            {
                using (var cmd = _connection.CreateCommand())
                {
                    cmd.CommandText = query;
                    return cmd.ExecuteNonQuery();
                }
            }
        }

        public int Execute(string format, params object[] pars)
        {
            return Execute(string.Format(format, pars));
        }
    }
}
