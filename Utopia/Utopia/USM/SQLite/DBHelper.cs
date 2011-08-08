using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.SQLite;

namespace Utopia.USM.SQLite
{
    public static class DBHelper
    {
        public delegate void NewConnection(SQLiteConnection Connection);
        public static event NewConnection Connection_Refreshed;
        public static SQLiteConnection Connection = new SQLiteConnection();

        private static string connectionStrInMemory = "Data Source=:memory:;Version=3;New=True;Compress=True";
        public static void OpenConnection(string File, bool inMemory = false)
        {
            if (Connection.State != System.Data.ConnectionState.Closed) Connection.Close();
            if (!inMemory)
            {
                Connection.ConnectionString = "Data Source=" + File + ";Compress=True;Synchronous=Off";
                Connection.Open();
            }
            else
            {
                Connection = CreateNewPlanetDataBaseMemory();
            }

            if (Connection_Refreshed != null) Connection_Refreshed(Connection);
        }

        public static void CloseConnection()
        {
            Connection.Close();
        }

        public static void CreateNewPlanetDataBase(string File)
        {
            SQLiteConnection Conn = new SQLiteConnection();
            Conn.ConnectionString = "Data Source=" + File + ";New=True;Compress=True;Synchronous=Off";
            Conn.Open();
            SQLiteCommand Cmd = new SQLiteCommand();
            Cmd = Conn.CreateCommand();
            Cmd.CommandText = "CREATE TABLE PLANET(PLANETSEED integer primary key, UNIVERSE_X integer, UNIVERSE_Y integer, UNIVERSE_Z integer, LANDSCAPEBUILDERVER varchar (10) )";
            Cmd.ExecuteNonQuery();
            Cmd.CommandText = "CREATE TABLE LANDSCAPE(CHUNKID BIGINT, BLOC_X integer, BLOC_Y integer, BLOC_Z integer, DATEMODIF datetime, CUBEID integer, METADATA1 tinyint, METADATA2 tinyint, METADATA3 tinyint, PRIMARY KEY(CHUNKID, BLOC_X, BLOC_Y, BLOC_Z))";
            Cmd.ExecuteNonQuery();
            Cmd.Dispose();
            Conn.Close();
        }

        public static SQLiteConnection CreateNewPlanetDataBaseMemory()
        {
            SQLiteConnection Conn = new SQLiteConnection();
            Conn.ConnectionString = connectionStrInMemory;
            Conn.Open();
            SQLiteCommand Cmd = new SQLiteCommand();
            Cmd = Conn.CreateCommand();
            Cmd.CommandText = "CREATE TABLE PLANET(PLANETSEED integer primary key, UNIVERSE_X integer, UNIVERSE_Y integer, UNIVERSE_Z integer, LANDSCAPEBUILDERVER varchar (10) )";
            Cmd.ExecuteNonQuery();
            Cmd.CommandText = "CREATE TABLE LANDSCAPE(CHUNKID BIGINT, BLOC_X integer, BLOC_Y integer, BLOC_Z integer, DATEMODIF datetime, CUBEID integer, METADATA1 tinyint, METADATA2 tinyint, METADATA3 tinyint, PRIMARY KEY(CHUNKID, BLOC_X, BLOC_Y, BLOC_Z))";
            Cmd.ExecuteNonQuery();
            Cmd.Dispose();

            return Conn;
        }
    }
}
