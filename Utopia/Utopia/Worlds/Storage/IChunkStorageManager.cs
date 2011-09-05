using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Utopia.Worlds.Storage.Structs;

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
    }
}
