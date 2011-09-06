using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.SQLite;
using System.Data;
using System.IO;
using System.Threading;
using System.Collections.Concurrent;
using Utopia.Worlds.Storage.Structs;
using Utopia.Shared.World;

namespace Utopia.Worlds.Storage
{
    public class SQLiteStorageManager : SQLiteManager, IChunkStorageManager, IDisposable
    {
        #region Private variables
        private Thread _storageThread;
        private readonly static int _nbrTicket = 2000;
        private Queue<int> _requestTickets;
        private ConcurrentQueue<CubeRequest> _dataRequestQueue;
        private ConcurrentQueue<ChunkDataStorage> _dataStoreQueue;
        private SQLiteCommand _landscapeGetHash;
        private SQLiteCommand _landscapeGetCmd;
        private SQLiteCommand _landscapeInsertCmd;
        #endregion

        #region Public Properties/Variables
        public bool IsRunning { get; set; }
        public ChunkDataStorage[] Data { get; private set; }
        #endregion
        /// <summary>
        /// Sqlite Manager
        /// </summary>
        /// <param name="path">The SQLite database path</param>
        public SQLiteStorageManager(WorldParameters worldParameters, bool forceNew = false)
            : base(worldParameters, forceNew)
        {
            Init();
        }

        #region Public Methods
        protected override void CreateDataBase(SQLiteConnection conn)
        {
            var command = conn.CreateCommand();
            command.CommandText = @"CREATE TABLE CHUNKS([ChunkId] BIGINT PRIMARY KEY NOT NULL, [X] integer NOT NULL, [Z] integer NOT NULL, [md5hash] blob NOT NULL, [data] blob NOT NULL);";
            command.CommandType = CommandType.Text;
            command.ExecuteNonQuery();
            Guid t;


        }
        #endregion

        #region Private Methods
        private void Init()
        {
            //Initiliaz the IChunkStorageManager
            IChunkStorageManager_Init();

            IsRunning = true;
            _storageThread = new Thread(new ThreadStart(StorageMainLoop)); //Start the main loop
            _storageThread.Start();
        }

        private void StorageMainLoop()
        {
            while (IsRunning)
            {
                ProcessQueues();
                Thread.Sleep(10);
            }
        }
        #endregion

        #region IChunkStorageManager implementation

        private void ProcessQueues()
        {
            processRequestQueue();
            processStoreQueue();
        }

        //GET + REQUEST Chunk DATA ======================================================================
        private void IChunkStorageManager_Init()
        {
            Data = new ChunkDataStorage[_nbrTicket];
            _requestTickets = new Queue<int>(_nbrTicket);
            _dataRequestQueue = new ConcurrentQueue<CubeRequest>();
            _dataStoreQueue = new ConcurrentQueue<ChunkDataStorage>();

            //Create the tickets and the Data Holder
            for (int i = 0; i < _nbrTicket; i++)
            {
                _requestTickets.Enqueue(i);
                Data[i] = null;
            }

            string CommandText;
            //Get a specific chunk
            CommandText = "SELECT [ChunkId], [X], [Z], [md5hash], [data] FROM CHUNKS WHERE (CHUNKID = @CHUNKID)";
            _landscapeGetCmd = new SQLiteCommand(CommandText);
            _landscapeGetCmd.Parameters.Add("@CHUNKID", System.Data.DbType.Int64);

            //Get All modified chunks Hashs
            CommandText = "SELECT [ChunkId], [md5hash] FROM CHUNKS";
            _landscapeGetHash = new SQLiteCommand(CommandText);

            //Upsert a specific chunk
            CommandText = "INSERT OR REPLACE INTO CHUNKS ([ChunkId],[X], [Z], [md5hash], [data]) VALUES (@CHUNKID, @X, @Z, @MD5, @DATA)";
            _landscapeInsertCmd = new SQLiteCommand(CommandText);
            _landscapeInsertCmd.Parameters.Add("@CHUNKID", System.Data.DbType.Int64);
            _landscapeInsertCmd.Parameters.Add("@X", System.Data.DbType.Int32);
            _landscapeInsertCmd.Parameters.Add("@Z", System.Data.DbType.Int32);
            _landscapeInsertCmd.Parameters.Add("@MD5", System.Data.DbType.Binary);
            _landscapeInsertCmd.Parameters.Add("@DATA", System.Data.DbType.Binary);
        }

        public int RequestDataTicket_async(long chunkID)
        {
            int ticket = _requestTickets.Dequeue();
            _dataRequestQueue.Enqueue(new CubeRequest() { ChunkId = chunkID, Ticket = ticket });
            Data[ticket] = null;
            return ticket;
        }

        public void FreeTicket(int ticket)
        {
            _requestTickets.Enqueue(ticket);
        }

        private void processRequestQueue()
        {
            CubeRequest _processingRequest;
            if (_dataRequestQueue.TryDequeue(out _processingRequest))
            {
                ProcessRequest(ref _processingRequest);
            }
        }

        private void ProcessRequest(ref CubeRequest _processingRequest)
        {
            _landscapeGetCmd.Parameters[0].Value = _processingRequest.ChunkId;

            SQLiteDataReader dataReader = _landscapeGetCmd.ExecuteReader();

            Int64? NbrRows = 0;
            ChunkDataStorage cubeDataStorage = null;

            if (NbrRows.HasValue && NbrRows > 0)
            {
                if (dataReader.Read())
                {
                    cubeDataStorage = new ChunkDataStorage()
                    {
                        ChunkId = dataReader.GetInt64(0),
                        ChunkX = dataReader.GetInt32(1),
                        ChunkZ = dataReader.GetInt32(2),
                        Md5Hash = new Shared.Structs.Md5Hash((byte[])dataReader.GetValue(3)),
                        CubeData = (byte[])dataReader.GetValue(4)
                    };
                }
            }

            dataReader.Close();
            Data[_processingRequest.Ticket] = cubeDataStorage;
        }
        //===============================================================================================

        //Set Chunk Data ================================================================================
        public void StoreData_async(ChunkDataStorage data)
        {
            _dataStoreQueue.Enqueue(data);
        }

        private void processStoreQueue()
        {
            ChunkDataStorage data;
            if (_dataStoreQueue.TryDequeue(out data))
            {
                //SaveObject(ref data);
                Console.WriteLine("Chunk save" + data.ChunkId);
            }
        }

        private void SaveObject(ref ChunkDataStorage data)
        {
            _landscapeInsertCmd.Parameters[0].Value = data.ChunkId;
            _landscapeInsertCmd.Parameters[1].Value = data.ChunkX;
            _landscapeInsertCmd.Parameters[2].Value = data.ChunkZ;
            _landscapeInsertCmd.Parameters[3].Value = data.Md5Hash.Bytes;
            _landscapeInsertCmd.Parameters[4].Value = data.CubeData;

            _landscapeInsertCmd.ExecuteNonQuery();
        }
        //===============================================================================================

        #endregion

        public override void Dispose()
        {
            IsRunning = false;
            _landscapeInsertCmd.Dispose();
            _landscapeGetCmd.Dispose();
            _landscapeGetHash.Dispose();
            base.Dispose();
        }
    }
}
