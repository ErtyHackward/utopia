using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.SQLite;

namespace Utopia.Worlds.Storage.SQLite
{
    public class SQLiteHelper
    {
        public SQLiteConnection OpenConnection(string File, bool inMemory = false)
        {
            try
            {
                SQLiteConnection Connection = new SQLiteConnection();

                if (!inMemory)
                {
                    Connection.ConnectionString = "Data Source=" + File + ";Compress=True;Synchronous=Off";
                    Connection.Open();
                }
                else
                {
                    Connection = CreateNewPlanetDataBaseMemory();
                }
                return Connection;
            }
            catch (Exception e)
            {
                throw new Exception("Error opening SQLite world Storage : " + File, e);
            }
        }

        public void CreateNewPlanetDataBase(string File)
        {
            SQLiteConnection Conn = new SQLiteConnection();
            Conn.ConnectionString = "Data Source=" + File + ";New=True;Compress=True;Synchronous=Off";
            Conn.Open();
            SQLiteCommand Cmd = new SQLiteCommand();
            Cmd = Conn.CreateCommand();
            Cmd.CommandText = "CREATE TABLE CHUNKS([ChunkId] BIGINT PRIMARY KEY NOT NULL, [X] integer NOT NULL, [Z] integer NOT NULL, [md5] integer NOT NULL, [data] blob NOT NULL);";
            Cmd.CommandType = System.Data.CommandType.Text;
            Cmd.ExecuteNonQuery();
            Cmd.Dispose();
            Conn.Close();
        }

        public SQLiteConnection CreateNewPlanetDataBaseMemory()
        {
            SQLiteConnection Conn = new SQLiteConnection();
            Conn.ConnectionString = "Data Source=:memory:;Version=3;New=True;Compress=True";
            Conn.Open();
            SQLiteCommand Cmd = new SQLiteCommand();
            Cmd = Conn.CreateCommand();
            Cmd.CommandText = "CREATE TABLE CHUNKS([ChunkId] BIGINT PRIMARY KEY NOT NULL, [X] integer NOT NULL, [Z] integer NOT NULL, [md5] integer NOT NULL, [data] blob NOT NULL);";
            Cmd.ExecuteNonQuery();
            Cmd.Dispose();

            return Conn;
        }

    }
}
