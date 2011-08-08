using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.Concurrent;
using System.Data.SQLite;
using Utopia.USM.SQLite;
using Utopia.Shared.Structs;
using Utopia.Shared.Structs.Landscape;

namespace Utopia.USM
{
    public class USMGet
    {
        #region private variables
        private static int NbrTickers = 2000;
        private ConcurrentQueue<CubeRequest> _dataRequest = new ConcurrentQueue<CubeRequest>();
        private Queue<int> _requestTickets = new Queue<int>(NbrTickers);
        private CubeRequest _cubeRequest;
        private SQLiteCommand _landscapeGetCmd;
        private SQLiteDataReader _dataReader;
        public HashSet<Int64> _modifiedChunksList; // Will store the various
        #endregion

        #region public variables
        public List<CubeData[]> Data = new List<CubeData[]>(NbrTickers);
        #endregion

        public USMGet()
        {
            //Create the generic command to get Chunks block
            string CommandText = "SELECT count(*) as nbr FROM LANDSCAPE WHERE (CHUNKID = @CHUNKID); SELECT CHUNKID, BLOC_X, BLOC_Y, BLOC_Z, DATEMODIF, CUBEID, METADATA1, METADATA2, METADATA3  FROM LANDSCAPE WHERE (CHUNKID = @CHUNKID)";
            _landscapeGetCmd = new SQLiteCommand(CommandText);
            _landscapeGetCmd.Parameters.Add("@CHUNKID", System.Data.DbType.Int64);

            DBHelper.Connection_Refreshed += new DBHelper.NewConnection(DBHelper_Connection_Refreshed);

            //Create the tickets and the Data Holder
            for (int i = 0; i < NbrTickers; i++)
            {
                _requestTickets.Enqueue(i);
                Data.Add(null);
            }


        }

        #region public properties

        #endregion

        #region public Methods
        //Retrieve the list of chunks that have been modified
        public void FetchChunkListFromPlanet()
        {
            SQLiteCommand selectDistinctChunks = new SQLiteCommand();
            selectDistinctChunks.Connection = _landscapeGetCmd.Connection;
            selectDistinctChunks.CommandText = "SELECT distinct(CHUNKID) as chunkID FROM LANDSCAPE";
            var _dataReader = selectDistinctChunks.ExecuteReader();
            _modifiedChunksList = new HashSet<long>();
            while (_dataReader.Read())
            {
                _modifiedChunksList.Add((Int64)(_dataReader[0] as Int64?));
                GetObject();
            }
        }

        public void AddChunkToList(Int64 chunkID)
        {
            _modifiedChunksList.Add(chunkID);
        }

        public void ProcessQueue()
        {
            if (_dataRequest.TryDequeue(out _cubeRequest))
            {
                GetObject();
            }
        }

        public int RequestChunkData(Int64 chunkId)
        {
            //Check if the chunk has been subject to modifications
            if (!_modifiedChunksList.Contains(chunkId)) return -1;

            int ticket = _requestTickets.Dequeue();
            _dataRequest.Enqueue(new CubeRequest() { ChunkID = chunkId, ticket = ticket });
            Data[ticket] = null;
            return ticket;
        }

        public void FreeTicket(int ticket)
        {
            _requestTickets.Enqueue(ticket);
        }
        #endregion

        #region private Methods

        void DBHelper_Connection_Refreshed(SQLiteConnection Connection)
        {
            _landscapeGetCmd.Connection = DBHelper.Connection;
        }

        private void GetObject()
        {
            _landscapeGetCmd.Parameters[0].Value = _cubeRequest.ChunkID;

            _dataReader = _landscapeGetCmd.ExecuteReader();

            Int64? NbrRows = 0;
            CubeData[] cubeDatas;
            CubeData cubedata;
            while (_dataReader.Read())
            {
                NbrRows = _dataReader[0] as Int64?;
            }

            if (NbrRows.HasValue && NbrRows > 0)
            {
                int rowId = 0;
                cubeDatas = new CubeData[(int)NbrRows];
                _dataReader.NextResult();
                while (_dataReader.Read())
                {
                    cubedata = new CubeData()
                    {
                        ChunkID = (int)(_dataReader[0] as Int64?),
                        Cube = new TerraCube()
                        {
                            Id = (byte)(_dataReader[5] as Int64?),
                            MetaData1 = (byte)(_dataReader[6] as byte?),
                            MetaData2 = (byte)(_dataReader[7] as byte?),
                            MetaData3 = (byte)(_dataReader[8] as byte?)
                        },
                        CubeChunkLocation = new Location3<int>((int)(_dataReader[1] as Int64?), (int)(_dataReader[2] as Int64?), (int)(_dataReader[3] as Int64?))
                    };

                    NbrRows = _dataReader[0] as int?;
                    cubeDatas[rowId] = cubedata;
                    rowId++;
                }
            }
            else
            {
                cubeDatas = new CubeData[0];
            }
            _dataReader.Close();
            Data[_cubeRequest.ticket] = cubeDatas;

        }
        #endregion
    }
}
