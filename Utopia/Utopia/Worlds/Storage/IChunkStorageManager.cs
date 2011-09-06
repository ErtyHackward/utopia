using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Utopia.Worlds.Storage.Structs;
using Utopia.Shared.Structs;
using System.Collections.Concurrent;

namespace Utopia.Worlds.Storage
{
    public interface IChunkStorageManager
    {
        //Async data request
        int RequestDataTicket_async(long chunkID); // You request your data with this interface, you receive a Ticket
        ChunkDataStorage[] Data { get; }           // You get your data inside this array using the ticket is index
        void FreeTicket(int ticket);               // When data received your free the ticket

        //Async Store chunk data
        void StoreData_async(ChunkDataStorage data);

        //Contains the list of all chunks inside the DB, this remove the needs to query/wait the database to get the MD5 hash
        ConcurrentDictionary<long, Md5Hash> ChunkHashes { get; }
    }
}
