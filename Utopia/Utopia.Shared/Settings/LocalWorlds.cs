using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Data.SQLite;
using Utopia.Shared.World;

namespace Utopia.Shared.Settings
{
    public static class LocalWorlds
    {
        public class LocalWorldsParam
        {
            public WorldParameters WorldParameters;
            public DateTime LastAccess;
            public DirectoryInfo ServerRootPath;
            public DirectoryInfo ClientRootPath;

            public override string ToString()
            {
                return "\"" + WorldParameters.WorldName + "\" last acceded on " + LastAccess.ToLongDateString() + " " + LastAccess.ToLongTimeString();
            }
        }

        private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();


        public static List<LocalWorldsParam> GetAllSinglePlayerWorldsParams(string applicationRootPath)
        {
            List<LocalWorldsParam> WorldsParameters = new List<LocalWorldsParam>();
            //Create the server, singleplayer root path
            string sglPlayerRootPath = applicationRootPath + @"\Server\SinglePlayer";

            if (Directory.Exists(sglPlayerRootPath))
            {
                foreach (var directory in Directory.GetDirectories(sglPlayerRootPath))
                {
                    string ServerDataBasePath = directory + @"\ServerWorld.db";
                    if (File.Exists(ServerDataBasePath))
                    {
                        //Load and extract information from database
                        LocalWorldsParam param = new LocalWorldsParam();
                        param.WorldParameters = ExtractInformationData(ServerDataBasePath, out param.LastAccess);
                        param.ServerRootPath = new DirectoryInfo(directory);
                        param.ClientRootPath = new DirectoryInfo(applicationRootPath + @"\Client\SinglePlayer\" + param.ServerRootPath.Name);
                        WorldsParameters.Add(param);
                    }
                }
            }

            return WorldsParameters;
        }

        private static WorldParameters ExtractInformationData(string DBPath, out DateTime fileTimeStamp)
        {
            WorldParameters worldParameters = null;

            FileInfo fi = new FileInfo(DBPath);
            fileTimeStamp = fi.LastWriteTime;

            SQLiteConnectionStringBuilder csb = new SQLiteConnectionStringBuilder
            {
                SyncMode = SynchronizationModes.Off,
                DataSource = DBPath,
                ReadOnly = true
            };

            SQLiteConnection connection = new SQLiteConnection(csb.ConnectionString);

            try
            {
                connection.Open();

                using (var cmd = connection.CreateCommand())
                {
                    cmd.CommandText = "SELECT * FROM WorldParameters";
                    var dataReader = cmd.ExecuteReader();
                    dataReader.Read();

                    worldParameters = new WorldParameters()
                    {
                        WorldName = dataReader.GetString(0),
                        SeedName = dataReader.GetString(1),
                        SeaLevel = dataReader.GetInt32(2)
                    };
                }

                connection.Close();
                connection.Dispose();
            }
            catch (Exception e)
            {
                logger.Error("Error ({1}) loading single player world data information for file {0}", DBPath, e.Message);
            }

            return worldParameters;
        }
    }
}
