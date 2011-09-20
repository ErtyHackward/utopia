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
using Utopia.Shared.Structs;
using Utopia.SQLite;

namespace Utopia.Worlds.Storage
{
    public class SQLiteWorldStorageManager : SQLiteManager, IChunkStorageManager, IDisposable
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
        /// <param name="path">The SQLite database path</param>
        public SQLiteWorldStorageManager(WorldParameters worldParameters, string UserName, bool forceNew = false)
            : base(@"Chunk_" + worldParameters.Seed + ".dat",
                   Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + @"\Utopia\Storage\" + UserName.Replace(@"\", "_") + @"\" + @"Worlds\", 
                   forceNew)
        {
            Init();
        }

        #region Public Methods
        protected override void CreateDataBase(SQLiteConnection conn)
        {
            var command = conn.CreateCommand();
            command = conn.CreateCommand();
            command.CommandText = @"CREATE TABLE CHUNKS([ChunkId] BIGINT PRIMARY KEY NOT NULL, [X] integer NOT NULL, [Z] integer NOT NULL, [md5hash] blob, [data] blob NOT NULL);";
            command.CommandType = CommandType.Text;
            command.ExecuteNonQuery();
        }
        #endregion

        #region Private Methods
        private void Init()
        {
            //Initiliaz the IChunkStorageManager
            IChunkStorageManager_Init();

            //Init the MD5 chunks hashes code
            GetChunksMd5();

            IsRunning = true;
            _storageThread = new Thread(new ThreadStart(StorageMainLoop)); //Start the main loop
            _storageThread.Start();
        }

        private void GetChunksMd5()
        {
            ChunkHashes = new ConcurrentDictionary<long, Md5Hash>();

            SQLiteDataReader dataReader = _landscapeGetHash.ExecuteReader();

            while (dataReader.Read())
            {
                while (!ChunkHashes.TryAdd(dataReader.GetInt64(0), dataReader.IsDBNull(1) ? null : new Shared.Structs.Md5Hash((byte[])dataReader.GetValue(1)))) ;
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
            processRequestQueue();
            processStoreQueue();
        }

        //GET + REQUEST Chunk DATA ======================================================================
        private void IChunkStorageManager_Init()
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

            string CommandText;
            //Get a specific chunk
            CommandText = "SELECT [ChunkId], [X], [Z], [md5hash], [data] FROM CHUNKS WHERE (CHUNKID = @CHUNKID)";
            _landscapeGetCmd = new SQLiteCommand(CommandText, _connection);
            _landscapeGetCmd.Parameters.Add("@CHUNKID", System.Data.DbType.Int64);

            //Get All modified chunks Hashs
            CommandText = "SELECT [ChunkId], [md5hash] FROM CHUNKS";
            _landscapeGetHash = new SQLiteCommand(CommandText, _connection);

            //Upsert a specific chunk
            CommandText = "INSERT OR REPLACE INTO CHUNKS ([ChunkId],[X], [Z], [md5hash], [data]) VALUES (@CHUNKID, @X, @Z, @MD5, @DATA)";
            _landscapeInsertCmd = new SQLiteCommand(CommandText, _connection);
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

            ChunkDataStorage cubeDataStorage = null;
            if (dataReader.Read())
            {
                cubeDataStorage = new ChunkDataStorage()
                {
                    ChunkId = dataReader.GetInt64(0),
                    ChunkX = dataReader.GetInt32(1),
                    ChunkZ = dataReader.GetInt32(2),
                    Md5Hash = !dataReader.IsDBNull(3) ? new Shared.Structs.Md5Hash((byte[])dataReader.GetValue(3)) : null,
                    CubeData = (byte[])dataReader.GetValue(4)
                };
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
            _landscapeInsertCmd.Parameters[4].Value = data.CubeData;

            _landscapeInsertCmd.ExecuteNonQuery();

            //Add or update the Dictionnary
            if (ChunkHashes.ContainsKey(data.ChunkId))
            {
                ChunkHashes[data.ChunkId] = data.Md5Hash;
            }
            else
            {
                while (!ChunkHashes.TryAdd(data.ChunkId, data.Md5Hash));
            }

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
