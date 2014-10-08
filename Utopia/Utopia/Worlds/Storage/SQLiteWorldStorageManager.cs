using System.Collections.Generic;
using System.Data.SQLite;
using System.Threading;
using System.Collections.Concurrent;
using S33M3Resources.Structs;
using Utopia.Shared;
using Utopia.Worlds.Storage.Structs;
using Utopia.Shared.Structs;
using S33M3DXEngine;

namespace Utopia.Worlds.Storage
{
    public class SQLiteWorldStorageManager : SQLiteStorage, IChunkStorageManager
    {
        #region Private variables
        private Thread _storageThread;
        private readonly static int _nbrTicket = 4096;
        private BlockingCollection<int> _requestTickets;
        private ConcurrentQueue<CubeRequest> _dataRequestQueue;
        private ConcurrentQueue<ChunkDataStorage> _dataStoreQueue;
        private SQLiteCommand _landscapeGetHash;
        private SQLiteCommand _landscapeGetCmd;
        private SQLiteCommand _landscapeInsertCmd;
        private ManualResetEvent _threadSync;
        private D3DEngine _d3dEngine;
        #endregion

        #region Public Properties/Variables
        public bool IsRunning { get; set; }
        public ChunkDataStorage[] Data { get; private set; }
        public ConcurrentDictionary<Vector3I, Md5Hash> ChunkHashes { get; private set; }
        #endregion

        /// <summary>
        /// Sqlite Manager
        /// </summary>
        /// <param name="fileName">The SQLite database path</param>
        /// <param name="forceNew"></param>
        public SQLiteWorldStorageManager(D3DEngine d3dEngine, string fileName, bool forceNew = false)
            : base(fileName, forceNew)
        {
            _d3dEngine = d3dEngine;

            //Initiliaz the IChunkStorageManager
            ChunkStorageManagerInit();

            //Init the MD5 chunks hashes code
            GetChunksMd5();

            IsRunning = true;
            _d3dEngine.RunningThreadedWork.Add("SQLIteWorldStorageManager");
            _d3dEngine.OnShuttingDown += d3dEngine_OnShuttingDown;
            _storageThread = new Thread(StorageMainLoop) { Name = "SQLLite Client" }; //Start the main loop
            _storageThread.Start();
        }

        protected override string CreateDataBase()
        {
            return @"CREATE TABLE CHUNKS([X] integer NOT NULL, [Y] integer NOT NULL, [Z] integer NOT NULL, [md5hash] blob, [data] blob NOT NULL, PRIMARY KEY(X,Y,Z));";
        }

        #region Private Methods

        private void GetChunksMd5()
        {
            ChunkHashes = new ConcurrentDictionary<Vector3I, Md5Hash>();

            using (var dataReader = _landscapeGetHash.ExecuteReader())
            {
                while (dataReader.Read())
                {
                    var chunkId = new Vector3I(dataReader.GetInt32(0),dataReader.GetInt32(1), dataReader.GetInt32(2));
                    var md5Hash = dataReader.IsDBNull(3) ? null : new Md5Hash((byte[]) dataReader.GetValue(3));

                    ChunkHashes.TryAdd(chunkId, md5Hash);
                }
            }
        }

        private void StorageMainLoop()
        {
            while (IsRunning && !_d3dEngine.IsShuttingDownRequested)
            {
                if (ProcessQueues() == false) _threadSync.Reset();
                _threadSync.WaitOne();
            }
            _d3dEngine.RunningThreadedWork.Remove("SQLIteWorldStorageManager");
        }

        private void d3dEngine_OnShuttingDown(object sender, System.EventArgs e)
        {
            _threadSync.Set();
        }
        #endregion

        #region IChunkStorageManager implementation

        private bool ProcessQueues()
        {
            return ProcessRequestQueue() || ProcessStoreQueue();
        }

        //GET + REQUEST Chunk DATA ======================================================================
        private void ChunkStorageManagerInit()
        {
            Data = new ChunkDataStorage[_nbrTicket + 1];
            _requestTickets = new BlockingCollection<int>(_nbrTicket);
            _dataRequestQueue = new ConcurrentQueue<CubeRequest>();
            _dataStoreQueue = new ConcurrentQueue<ChunkDataStorage>();

            //Create the tickets and the Data Holder From 1 To _nbrTicket.
            for (int i = 1; i <= _nbrTicket; i++)
            {
                _requestTickets.Add(i);
                Data[i] = null;
            }

            //Get a specific chunk
            var commandText = "SELECT [X], [Y], [Z], [md5hash], [data] FROM CHUNKS WHERE (X = @X AND Y = @Y AND Z = @Z)";
            _landscapeGetCmd = new SQLiteCommand(commandText, Connection);
            _landscapeGetCmd.Parameters.Add("@X", System.Data.DbType.Int32);
            _landscapeGetCmd.Parameters.Add("@Y", System.Data.DbType.Int32);
            _landscapeGetCmd.Parameters.Add("@Z", System.Data.DbType.Int32);

            //Get All modified chunks Hashs
            commandText = "SELECT [X], [Y], [Z], [md5hash] FROM CHUNKS";
            _landscapeGetHash = new SQLiteCommand(commandText, Connection);

            //Upsert a specific chunk
            commandText = "INSERT OR REPLACE INTO CHUNKS ([X], [Y], [Z], [md5hash], [data]) VALUES (@X, @Y, @Z, @MD5, @DATA)";
            _landscapeInsertCmd = new SQLiteCommand(commandText, Connection);
            _landscapeInsertCmd.Parameters.Add("@X", System.Data.DbType.Int32);
            _landscapeInsertCmd.Parameters.Add("@Y", System.Data.DbType.Int32);
            _landscapeInsertCmd.Parameters.Add("@Z", System.Data.DbType.Int32);
            _landscapeInsertCmd.Parameters.Add("@MD5", System.Data.DbType.Binary);
            _landscapeInsertCmd.Parameters.Add("@DATA", System.Data.DbType.Binary);

            _threadSync = new ManualResetEvent(false);
        }

        public int RequestDataTicket_async(Vector3I chunkPos)
        {
            int ticket = _requestTickets.Take();
            _dataRequestQueue.Enqueue(new CubeRequest { ChunkId = chunkPos, Ticket = ticket });
            Data[ticket] = null;
            _threadSync.Set();
            return ticket;
        }

        public void FreeTicket(int ticket)
        {
            _requestTickets.Add(ticket);
        }

        private bool ProcessRequestQueue()
        {
            CubeRequest processingRequest;
            if (_dataRequestQueue.TryDequeue(out processingRequest))
            {
                ProcessRequest(ref processingRequest);
            }

            if (_dataRequestQueue.Count > 0) return true;
            return false;
        }

        private void ProcessRequest(ref CubeRequest processingRequest)
        {
            _landscapeGetCmd.Parameters[0].Value = processingRequest.ChunkId.X;
            _landscapeGetCmd.Parameters[1].Value = processingRequest.ChunkId.Y;
            _landscapeGetCmd.Parameters[2].Value = processingRequest.ChunkId.Z;

            ChunkDataStorage cubeDataStorage = null;
            using (var dataReader = _landscapeGetCmd.ExecuteReader())
            {
                if (dataReader.Read())
                {
                    cubeDataStorage = new ChunkDataStorage
                                          {
                                              ChunkPos = new Vector3I(dataReader.GetInt32(0), dataReader.GetInt32(1), dataReader.GetInt32(2)),
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

            _threadSync.Set();
        }

        private bool ProcessStoreQueue()
        {
            ChunkDataStorage data;
            if (_dataStoreQueue.TryDequeue(out data))
            {
                SaveObject(ref data);
            }

            if (_dataStoreQueue.Count > 0) return true;
            return false;
        }

        private void SaveObject(ref ChunkDataStorage data)
        {
            _landscapeInsertCmd.Parameters[0].Value = data.ChunkPos.X;
            _landscapeInsertCmd.Parameters[1].Value = data.ChunkPos.Y;
            _landscapeInsertCmd.Parameters[2].Value = data.ChunkPos.Z;
            if (data.Md5Hash != null) _landscapeInsertCmd.Parameters[3].Value = data.Md5Hash.Bytes;
            else _landscapeInsertCmd.Parameters[3].Value = null;
            _landscapeInsertCmd.Parameters[4].Value = data.CubeData; //Chunk + Entities Data under compressed form stored

            _landscapeInsertCmd.ExecuteNonQuery();

            //Add or update the Dictionnary
            //The dictionnary reflect the Chunks saved on SQLite database.
            //For a specified ChunkID it give the MD5Hash.
            var chunkHash = data.Md5Hash;

            ChunkHashes.AddOrUpdate(data.ChunkPos, chunkHash, (id, hash) => chunkHash);
        }
        //===============================================================================================

        #endregion

        public override void Dispose()
        {
            _d3dEngine.OnShuttingDown -= d3dEngine_OnShuttingDown;
            IsRunning = false;
            //Wait thread the exit
            _threadSync.Set();
            while (_storageThread.ThreadState == ThreadState.Running) { }
            _landscapeInsertCmd.Dispose();
            _landscapeGetCmd.Dispose();
            _landscapeGetHash.Dispose();
            _threadSync.Dispose();
            base.Dispose();
        }
    }
}
