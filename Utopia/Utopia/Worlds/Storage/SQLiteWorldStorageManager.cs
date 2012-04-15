using System.Collections.Generic;
using System.Data.SQLite;
using System.Threading;
using System.Collections.Concurrent;
using Utopia.Shared;
using Utopia.Worlds.Storage.Structs;
using Utopia.Shared.Structs;

namespace Utopia.Worlds.Storage
{
    public class SQLiteWorldStorageManager : SQLiteStorage, IChunkStorageManager
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
        public ConcurrentDictionary<long, Md5Hash> ChunkHashes { get; private set; }
        #endregion

        /// <summary>
        /// Sqlite Manager
        /// </summary>
        /// <param name="fileName">The SQLite database path</param>
        /// <param name="forceNew"></param>
        public SQLiteWorldStorageManager(string fileName, bool forceNew = false)
            : base(fileName, forceNew)
        {
            //Initiliaz the IChunkStorageManager
            ChunkStorageManagerInit();

            //Init the MD5 chunks hashes code
            GetChunksMd5();

            IsRunning = true;
            _storageThread = new Thread(StorageMainLoop); //Start the main loop
            _storageThread.Start();
        }

        public void Reset(string fileName, bool forceNew = false)
        {
            IsRunning = false;
            CloseConnection();
            CreateDBConnection(fileName, forceNew);

            _landscapeInsertCmd.Dispose();
            _landscapeGetCmd.Dispose();
            _landscapeGetHash.Dispose();

            ChunkStorageManagerInit();

            IsRunning = true;
            _storageThread = new Thread(StorageMainLoop); //Start the main loop
            _storageThread.Start();
        }

        protected override string CreateDataBase()
        {
            return @"CREATE TABLE CHUNKS([ChunkId] BIGINT PRIMARY KEY NOT NULL, [X] integer NOT NULL, [Z] integer NOT NULL, [md5hash] blob, [data] blob NOT NULL);";
        }

        #region Private Methods

        private void GetChunksMd5()
        {
            ChunkHashes = new ConcurrentDictionary<long, Md5Hash>();

            using (var dataReader = _landscapeGetHash.ExecuteReader())
            {
                while (dataReader.Read())
                {
                    var chunkId = dataReader.GetInt64(0);
                    var md5Hash = dataReader.IsDBNull(1) ? null : new Md5Hash((byte[]) dataReader.GetValue(1));

                    ChunkHashes.TryAdd(chunkId, md5Hash);
                }
            }
        }

        private void StorageMainLoop()
        {           
            while (IsRunning)
            {
                ProcessQueues();
                Thread.Sleep(1);
            }
        }
        #endregion

        #region IChunkStorageManager implementation

        private void ProcessQueues()
        {
            ProcessRequestQueue();
            ProcessStoreQueue();
        }

        //GET + REQUEST Chunk DATA ======================================================================
        private void ChunkStorageManagerInit()
        {
            Data = new ChunkDataStorage[_nbrTicket + 1];
            _requestTickets = new Queue<int>(_nbrTicket);
            _dataRequestQueue = new ConcurrentQueue<CubeRequest>();
            _dataStoreQueue = new ConcurrentQueue<ChunkDataStorage>();

            //Create the tickets and the Data Holder From 1 To _nbrTicket.
            for (int i = 1; i <= _nbrTicket; i++)
            {
                _requestTickets.Enqueue(i);
                Data[i] = null;
            }

            //Get a specific chunk
            var commandText = "SELECT [ChunkId], [X], [Z], [md5hash], [data] FROM CHUNKS WHERE (CHUNKID = @CHUNKID)";
            _landscapeGetCmd = new SQLiteCommand(commandText, Connection);
            _landscapeGetCmd.Parameters.Add("@CHUNKID", System.Data.DbType.Int64);

            //Get All modified chunks Hashs
            commandText = "SELECT [ChunkId], [md5hash] FROM CHUNKS";
            _landscapeGetHash = new SQLiteCommand(commandText, Connection);

            //Upsert a specific chunk
            commandText = "INSERT OR REPLACE INTO CHUNKS ([ChunkId],[X], [Z], [md5hash], [data]) VALUES (@CHUNKID, @X, @Z, @MD5, @DATA)";
            _landscapeInsertCmd = new SQLiteCommand(commandText, Connection);
            _landscapeInsertCmd.Parameters.Add("@CHUNKID", System.Data.DbType.Int64);
            _landscapeInsertCmd.Parameters.Add("@X", System.Data.DbType.Int32);
            _landscapeInsertCmd.Parameters.Add("@Z", System.Data.DbType.Int32);
            _landscapeInsertCmd.Parameters.Add("@MD5", System.Data.DbType.Binary);
            _landscapeInsertCmd.Parameters.Add("@DATA", System.Data.DbType.Binary);
        }

        public int RequestDataTicket_async(long chunkID)
        {
            int ticket = _requestTickets.Dequeue();
            _dataRequestQueue.Enqueue(new CubeRequest { ChunkId = chunkID, Ticket = ticket });
            Data[ticket] = null;
            return ticket;
        }

        public void FreeTicket(int ticket)
        {
            _requestTickets.Enqueue(ticket);
        }

        private void ProcessRequestQueue()
        {
            CubeRequest processingRequest;
            if (_dataRequestQueue.TryDequeue(out processingRequest))
            {
                ProcessRequest(ref processingRequest);
            }
        }

        private void ProcessRequest(ref CubeRequest processingRequest)
        {
            _landscapeGetCmd.Parameters[0].Value = processingRequest.ChunkId;

            ChunkDataStorage cubeDataStorage = null;
            using (var dataReader = _landscapeGetCmd.ExecuteReader())
            {
                if (dataReader.Read())
                {
                    cubeDataStorage = new ChunkDataStorage
                                          {
                                              ChunkId = dataReader.GetInt64(0),
                                              ChunkX = dataReader.GetInt32(1),
                                              ChunkZ = dataReader.GetInt32(2),
                                              Md5Hash = !dataReader.IsDBNull(3) ? new Md5Hash((byte[]) dataReader.GetValue(3)) : null,
                                              CubeData = (byte[]) dataReader.GetValue(4)
                                          };
                }
            }
            
            Data[processingRequest.Ticket] = cubeDataStorage;
        }
        //===============================================================================================

        //Set Chunk Data ================================================================================
        public void StoreData_async(ChunkDataStorage data)
        {
            _dataStoreQueue.Enqueue(data);
        }

        private void ProcessStoreQueue()
        {
            ChunkDataStorage data;
            if (_dataStoreQueue.TryDequeue(out data))
            {
                SaveObject(ref data);
            }
        }

        private void SaveObject(ref ChunkDataStorage data)
        {
            _landscapeInsertCmd.Parameters[0].Value = data.ChunkId;
            _landscapeInsertCmd.Parameters[1].Value = data.ChunkX;
            _landscapeInsertCmd.Parameters[2].Value = data.ChunkZ;
            if (data.Md5Hash != null) _landscapeInsertCmd.Parameters[3].Value = data.Md5Hash.Bytes;
            else _landscapeInsertCmd.Parameters[3].Value = null;
            _landscapeInsertCmd.Parameters[4].Value = data.CubeData; //Chunk + Entities Data under compressed form stored

            _landscapeInsertCmd.ExecuteNonQuery();

            //Add or update the Dictionnary
            //The dictionnary reflect the Chunks saved on SQLite database.
            //For a specified ChunkID it give the MD5Hash.
            var chunkHash = data.Md5Hash;

            ChunkHashes.AddOrUpdate(data.ChunkId, chunkHash, (id, hash) => chunkHash);
        }
        //===============================================================================================

        #endregion

        public override void Dispose()
        {
            IsRunning = false;
            //Wait thread the exit
            while (_storageThread.ThreadState == ThreadState.Running) { }
            _landscapeInsertCmd.Dispose();
            _landscapeGetCmd.Dispose();
            _landscapeGetHash.Dispose();
            base.Dispose();
        }
    }
}
