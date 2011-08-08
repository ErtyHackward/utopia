using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Utopia.Planets.Terran.Cube;
using S33M3Engines.Struct;
using System.Collections.Concurrent;
using System.Data.SQLite;
using Utopia.USM.SQLite;

namespace Utopia.USM
{
    public class USMSet
    {
        #region private variables
        private CubeData _cubeData;
        private SQLiteCommand _landscapeInsertCmd;
        private ConcurrentQueue<CubeData> _dataQueue = new ConcurrentQueue<CubeData>();
        #endregion

        #region public variables
        #endregion

        public USMSet()
        {
            string _landscapeInsert = "INSERT OR REPLACE INTO LANDSCAPE (CHUNKID,BLOC_X,BLOC_Y,BLOC_Z, CUBEID, DATEMODIF, METADATA1, METADATA2, METADATA3) VALUES ";
            _landscapeInsert += "(@CHUNKID, @BLOC_X, @BLOC_Y, @BLOC_Z, @CUBEID, @DATEMODIF, @METADATA1, @METADATA2, @METADATA3)";

            _landscapeInsertCmd = new SQLiteCommand(_landscapeInsert);
            _landscapeInsertCmd.Parameters.Add("@CHUNKID", System.Data.DbType.Int64);
            _landscapeInsertCmd.Parameters.Add("@BLOC_X", System.Data.DbType.Int32);
            _landscapeInsertCmd.Parameters.Add("@BLOC_Y", System.Data.DbType.Int32);
            _landscapeInsertCmd.Parameters.Add("@BLOC_Z", System.Data.DbType.Int32);
            _landscapeInsertCmd.Parameters.Add("@CUBEID", System.Data.DbType.Int16);
            _landscapeInsertCmd.Parameters.Add("@DATEMODIF", System.Data.DbType.DateTime);
            _landscapeInsertCmd.Parameters.Add("@METADATA1", System.Data.DbType.SByte);
            _landscapeInsertCmd.Parameters.Add("@METADATA2", System.Data.DbType.SByte);
            _landscapeInsertCmd.Parameters.Add("@METADATA3", System.Data.DbType.SByte);
            _landscapeInsertCmd.Parameters.Add("@METADATA3", System.Data.DbType.SByte);
            _landscapeInsertCmd.Parameters.Add("@METADATA3", System.Data.DbType.SByte);
            _landscapeInsertCmd.Parameters.Add("@METADATA3", System.Data.DbType.SByte);
            DBHelper.Connection_Refreshed += new DBHelper.NewConnection(DBHelper_Connection_Refreshed);
        }

        #region public properties

        public void Enqueue(ref CubeData saveCube)
        {
            _dataQueue.Enqueue(saveCube);
            UtopiaSaveManager.GetData.AddChunkToList(saveCube.ChunkID);
        }

        public void ProcessQueue()
        {
            if (_dataQueue.TryDequeue(out _cubeData))
            {
                SaveObject();
            }
        }
        #endregion

        #region public Methods

        #endregion

        #region private Methods
        void DBHelper_Connection_Refreshed(SQLiteConnection Connection)
        {
            _landscapeInsertCmd.Connection = DBHelper.Connection;
        }

        private void SaveObject()
        {
            _landscapeInsertCmd.Parameters[0].Value = _cubeData.ChunkID;
            _landscapeInsertCmd.Parameters[1].Value = _cubeData.CubeChunkLocation.X;
            _landscapeInsertCmd.Parameters[2].Value = _cubeData.CubeChunkLocation.Y;
            _landscapeInsertCmd.Parameters[3].Value = _cubeData.CubeChunkLocation.Z;
            _landscapeInsertCmd.Parameters[4].Value = _cubeData.Cube.Id;
            _landscapeInsertCmd.Parameters[5].Value = DateTime.Now;
            _landscapeInsertCmd.Parameters[6].Value = _cubeData.Cube.MetaData1;
            _landscapeInsertCmd.Parameters[7].Value = _cubeData.Cube.MetaData2;
            _landscapeInsertCmd.Parameters[8].Value = _cubeData.Cube.MetaData3;
            
            _landscapeInsertCmd.ExecuteNonQuery();
        }
        #endregion

    }
}

