using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Collections.Concurrent;
using System.IO;
using System.Windows.Forms;
using Utopia.USM.SQLite;
using S33M3Engines.Struct;
using Utopia.Planets.Terran.Cube;
using Utopia.Shared.Structs;
using Utopia.Shared.Structs.Landscape;

namespace Utopia.USM
{
    public struct CubeData
    {
        public Int64 ChunkID;
        public Location3<int> CubeChunkLocation;
        public TerraCube Cube;
    }

    public struct CubeRequest
    {
        public Int64 ChunkID;
        public int ticket;
    }

    public static class UtopiaSaveManager
    {
        #region private variables
        private static Thread _USMThread;
        private static bool _isRunning = false;
        private static Planets.Planet.PlanetInfo _currentPlanetInfo;
        private static string _USMPath;
        private static string _universePath;
        #endregion

        #region public variables
        public static USMSet Setdata;
        public static USMGet GetData;
        #endregion

        #region Public properties
        public static bool IsRunning
        {
            get { return _isRunning; }
            set { _isRunning = value; }
        }

        public static Planets.Planet.PlanetInfo CurrentPlanetInfo
        {
            get { return UtopiaSaveManager._currentPlanetInfo; }
            set { UtopiaSaveManager._currentPlanetInfo = value; }
        }
        #endregion

        #region Private Methods
        private static void USMPump()
        {
            Setdata = new USMSet();
            GetData = new USMGet();

            while (_isRunning)
            {
                Setdata.ProcessQueue();
                GetData.ProcessQueue();

                Thread.Sleep(10);
            }
        }

        private static void ChangeDBFile()
        {
            string PlanetDBFile = _universePath + @"\planet" + _currentPlanetInfo.UniverseLocation.X + "_" + _currentPlanetInfo.UniverseLocation.Y + "_" + _currentPlanetInfo.UniverseLocation.Z + ".dat";
            
            //Does the file Exist ?
            if (!File.Exists(PlanetDBFile))
            {
                //If not create the database !
                DBHelper.CreateNewPlanetDataBase(PlanetDBFile);
            }

            DBHelper.OpenConnection(PlanetDBFile, false);
        }
        #endregion

        #region Public Methods
        public static void Start(string UniverseName)
        {
            //Check if the save directory does exist
            _USMPath = Path.GetDirectoryName(Application.ExecutablePath) + @"\Saves";

            //delete save file===============================================
            if (!Directory.Exists(_USMPath))
            {
                Directory.CreateDirectory(_USMPath);
            }
            else
            {
                Directory.Delete(_USMPath, true);
                Directory.CreateDirectory(_USMPath);
            }
            //================================================================

            //Check if the Universe Directory exist
            _universePath = _USMPath + @"\" + UniverseName;
            if (!Directory.Exists(_universePath))
            {
                Directory.CreateDirectory(_universePath);
            }

            _isRunning = true;
            _USMThread = new Thread(new ThreadStart(UtopiaSaveManager.USMPump));
            _USMThread.Start();
        }

        public static void ChangePlanet(Planets.Planet.PlanetInfo planetInfo)
        {
            _currentPlanetInfo = planetInfo;
            ChangeDBFile();
            GetData.FetchChunkListFromPlanet();
        }

        public static void StopUSM()
        {
            _isRunning = false;
            DBHelper.CloseConnection();
        }
        #endregion
    }
}
