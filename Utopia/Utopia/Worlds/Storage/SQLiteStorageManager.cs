using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Utopia.Shared.World;
using System.Threading;
using System.Windows.Forms;
using System.IO;
using Utopia.Worlds.Storage.SQLite;
using System.Data.SQLite;
using Utopia.Worlds.Storage.Structs;
using System.Collections.Concurrent;

namespace Utopia.Worlds.Storage
{
    /// <summary>
    /// Use to store the game data locally.
    /// It is linked to a specific world
    /// </summary>
    public class SQLiteStorageManager : IStorageManager
    {
        #region private variable
        private Thread _storageThread;
        private WorldParameters _worldParameters;
        private string _worldPath;
        private SQLiteHelper _sqLiteHelper = new SQLiteHelper();
        private SQLiteConnection _dbConnection;

        private readonly static int _nbrTicket = 2000;
        private Queue<int> _requestTickets;
        private ConcurrentQueue<CubeRequest> _dataRequestQueue;
        private ConcurrentQueue<ChunkDataStorage> _dataStoreQueue;
        private SQLiteCommand _landscapeGetCmd;
        private SQLiteCommand _landscapeInsertCmd;
        #endregion

        #region Public properties / Variables
        public ChunkDataStorage[] Data { get; private set; }

        private bool IsRunning { get; set; }
        public WorldParameters WorldParameters
        {
            get { return _worldParameters; }
            set { _worldParameters = value; ChangeDBFile(); }
        }
        #endregion

        public SQLiteStorageManager(WorldParameters worldParameters, bool forceNew = false)
        {
            Initilialize(forceNew);

            WorldParameters = worldParameters;
        }

        #region Private Methods
        private void Initilialize(bool forceNew = false)
        {
            //Check if the save directory does exist
            string path = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + @"\Utopia\Storage";
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }

            _worldPath = path + @"\Worlds";
            if (forceNew && Directory.Exists(_worldPath))
            {
                Directory.Delete(_worldPath, true);
            }
            Directory.CreateDirectory(_worldPath);

            //Initiliaz the IChunkStorageManager
            IChunkStorageManager_Init();

            IsRunning = true;
            _storageThread = new Thread(new ThreadStart(StorageMainLoop)); //Start the main loop
            _storageThread.Start();
        }

        private void ChangeDBFile()
        {
            string currentDBFile = _worldPath + @"\Chunks_" + _worldParameters.Seed.ToString() + ".sqlite";

            //Does the file Exist ?
            if (!File.Exists(currentDBFile))
            {
                //If not create the database !
                _sqLiteHelper.CreateNewPlanetDataBase(currentDBFile);
            }

            if (_dbConnection != null && _dbConnection.State != System.Data.ConnectionState.Closed) _dbConnection.Close();
            _dbConnection = _sqLiteHelper.OpenConnection(currentDBFile, false);
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

            string CommandText = "SELECT [ChunkId], [X], [Z], [data] FROM CHUNKS WHERE (CHUNKID = @CHUNKID)";
            _landscapeGetCmd = new SQLiteCommand(CommandText);
            _landscapeGetCmd.Parameters.Add("@CHUNKID", System.Data.DbType.Int64);

            string _landscapeInsert = "INSERT OR REPLACE INTO CHUNKS ([ChunkId],[X], [Z], [md5], [data]) VALUES ";
            _landscapeInsert += "(@CHUNKID, @X, @Z, @MD5, @DATA)";

            
            _landscapeInsertCmd = new SQLiteCommand(_landscapeInsert);
            _landscapeInsertCmd.Parameters.Add("@CHUNKID", System.Data.DbType.Int64);
            _landscapeInsertCmd.Parameters.Add("@X", System.Data.DbType.SByte);
            _landscapeInsertCmd.Parameters.Add("@Z", System.Data.DbType.SByte);
            _landscapeInsertCmd.Parameters.Add("@MD5", System.Data.DbType.SByte);
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
                        md5 = dataReader.GetInt32(3),
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
                SaveObject(ref data);
            }
        }

        private void SaveObject(ref ChunkDataStorage data)
        {
            _landscapeInsertCmd.Parameters[0].Value = data.ChunkId;
            _landscapeInsertCmd.Parameters[1].Value = data.ChunkX;
            _landscapeInsertCmd.Parameters[2].Value = data.ChunkZ;
            _landscapeInsertCmd.Parameters[3].Value = data.md5;
            _landscapeInsertCmd.Parameters[4].Value = data.CubeData;

            _landscapeInsertCmd.ExecuteNonQuery();
        }
        //===============================================================================================

        #endregion

        public void Dispose()
        {
            IsRunning = false;
            _dbConnection.Close();
            _dbConnection.Dispose();
        }
    }
}
