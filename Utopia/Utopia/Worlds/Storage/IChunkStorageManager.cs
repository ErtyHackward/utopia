using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using S33M3Resources.Structs;
using Utopia.Worlds.Storage.Structs;
using Utopia.Shared.Structs;
using System.Collections.Concurrent;

namespace Utopia.Worlds.Storage
{
    public interface IChunkStorageManager : IDisposable
    {
        //Async data request
        int RequestDataTicket_async(Vector3I chunkPos); // You request your data with this interface, you receive a Ticket
        ChunkDataStorage[] Data { get; }                // You get your data inside this array using the ticket is index
        void FreeTicket(int ticket);                    // When data received your free the ticket

        //Async Store chunk data
        void StoreData_async(ChunkDataStorage data);

        //Contains the list of all chunks inside the DB, this remove the needs to query/wait the database to get the MD5 hash
        ConcurrentDictionary<Vector3I, Md5Hash> ChunkHashes { get; }
    }
}
