using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Data.SQLite;
using Utopia.Shared.World;
using System.Globalization;
using Utopia.Shared.Configuration;

namespace Utopia.Shared.Settings
{
    public static class LocalWorlds
    {
        public static List<LocalWorlds.LocalWorldsParam> LocalWorldsParams;

        public class LocalWorldsParam
        {
            public WorldParameters WorldParameters;
            public DateTime LastAccess;
            public DirectoryInfo WorldServerRootPath;
            public DirectoryInfo WorldClientRootPath;

            public override string ToString()
            {
                return "\"" + WorldParameters.WorldName + "\" last acceded on " + LastAccess.ToString("yyyy/MM/dd HH:mm:ss", new CultureInfo("en"));
            }
        }

        private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        public static string GetSinglePlayerServerRootPath(string applicationRootPath)
        {
            return applicationRootPath + @"\Server\SinglePlayer";
        }

        public static string GetSinglePlayerClientRootPath(string applicationRootPath)
        {
            return applicationRootPath + @"\Client\SinglePlayer";
        }

        public static List<LocalWorldsParam> GetAllSinglePlayerWorldsParams(string applicationRootPath)
        {
            List<LocalWorldsParam> worldsParameters = new List<LocalWorldsParam>();
            //Create the server, singleplayer root path
            string sglPlayerRootPath = GetSinglePlayerServerRootPath(applicationRootPath);

            if (Directory.Exists(sglPlayerRootPath))
            {
                foreach (var directory in Directory.GetDirectories(sglPlayerRootPath))
                {
                    var serverDataBasePath = directory + @"\ServerWorld.db";
                    if (File.Exists(serverDataBasePath))
                    {
                        //Load and extract information from database
                        LocalWorldsParam param = new LocalWorldsParam();
                        param.WorldParameters = ExtractInformationData(serverDataBasePath, out param.LastAccess);
                        param.WorldServerRootPath = new DirectoryInfo(directory);
                        param.WorldClientRootPath = new DirectoryInfo(GetSinglePlayerClientRootPath(applicationRootPath)+ @"\" + param.WorldServerRootPath.Name);
                        worldsParameters.Add(param);
                    }
                }
            }

            return worldsParameters;
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
                    using (var dataReader = cmd.ExecuteReader())
                    {
                        dataReader.Read();

                        RealmConfiguration configuration = new RealmConfiguration();
                        using (var ms = new MemoryStream((byte[])dataReader.GetValue(2)))
                        {
                            configuration.Load(new BinaryReader(ms));
                        }

                        worldParameters = new WorldParameters()
                                              {
                                                  WorldName = dataReader.GetString(0),
                                                  SeedName = dataReader.GetString(1),
                                                  Configuration = configuration
                                              };
                    }
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
