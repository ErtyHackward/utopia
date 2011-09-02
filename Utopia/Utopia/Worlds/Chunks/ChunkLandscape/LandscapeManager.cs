using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using S33M3Engines.Threading;
using System.Threading.Tasks;
using Utopia.Shared.World;
using Utopia.Shared.Chunks;
using Amib.Threading;
using Utopia.Network;
using Utopia.Net.Messages;

namespace Utopia.Worlds.Chunks.ChunkLandscape
{
    public class LandscapeManager : ILandscapeManager
    {
        #region Private variable
        private CreateLandScapeDelegate _createLandScapeDelegate;
        private delegate object CreateLandScapeDelegate(object chunk);
        private WorldGenerator _worldGenerator;
        private Server _server;
        private bool _serverCreation;
        private Dictionary<long, ChunkDataMessage> _receivedServerChunks;
        #endregion

        #region Public variables/properties
        public WorldGenerator WorldGenerator
        {
            get { return _worldGenerator; }
            set { _worldGenerator = value; }
        }
        #endregion

        public LandscapeManager(Server server)
        {
            if (server.Connected)
            {
                _server = server;
                _serverCreation = true;
                _receivedServerChunks = new Dictionary<long, ChunkDataMessage>(1024);
                _server.ServerConnection.MessageChunkData += ServerConnection_MessageChunkData;
            }
            else
            {
                _serverCreation = false;
            }

            Intialize();
        }

        public void Dispose()
        {
        }

        #region Public methods
        //Create the landscape for the chunk
        public void CreateLandScape(VisualChunk chunk, bool Async)
        {
            if (_serverCreation == false)// Not server connected, the chunks will be auto generated
            {
                CreateLandscapeFromGenerator(chunk, Async);
            }
            else 
            {
                CheckServerReceivedData(chunk, Async);
            }
        }

        private void CheckServerReceivedData(VisualChunk chunk, bool Async)
        {
            //Is this chunk server requested ==> Then check if the result is buffered
            if (chunk.IsServerRequested)
            {
                ChunkDataMessage message;
                if (_receivedServerChunks.TryGetValue(chunk.ChunkID, out message))
                {
                    chunk.IsServerRequested = false;
                    switch (message.Flag)
                    {
                        //In this case the message contains the data from the landscape !
                        case ChunkDataMessageFlag.ChunkWasModified:
                            chunk.Decompress(message.Data); //Set the data into the "Big Array"
                            _receivedServerChunks.Remove(chunk.ChunkID);
                            chunk.RefreshBorderChunk();
                            chunk.State = ChunkState.LandscapeCreated;
                            chunk.ThreadStatus = ThreadStatus.Idle;
                            break;
                        case ChunkDataMessageFlag.ChunkCanBeGenerated:
                            CreateLandscapeFromGenerator(chunk, Async);
                            break;
                        case ChunkDataMessageFlag.ChunkMd5Equal:
                            //TODO Check the generated chunk against the server MD5 Hash code
                            CreateLandscapeFromGenerator(chunk, Async);
                            break;
                        default:
                            break;
                    }
                }
            }
            else
            {
                chunk.IsServerRequested = true;
                //Request the chunk data to the server
                _server.ServerConnection.SendAsync(new Utopia.Net.Messages.GetChunksMessage()
                {
                    StartPosition = chunk.ChunkPosition,
                    EndPosition = chunk.ChunkPosition + 1,
                    Flag = Net.Messages.GetChunksMessageFlag.DontSendChunkDataIfNotModified
                });
            }
        }

        #endregion

        #region Private methods
        private void Intialize()
        {
            _createLandScapeDelegate = new CreateLandScapeDelegate(createLandScape_threaded);
        }

        //New chunk Received !
        private void ServerConnection_MessageChunkData(object sender, Net.Connections.ProtocolMessageEventArgs<Net.Messages.ChunkDataMessage> e)
        {
            //Bufferize the Data here
            //Console.WriteLine("== New Chunk data receive : " + e.Message.Position.ToString());
            if(_receivedServerChunks.ContainsKey(e.Message.Position.GetID())) _receivedServerChunks.Remove(e.Message.Position.GetID());
            _receivedServerChunks.Add(e.Message.Position.GetID(), e.Message);
        }

        //TODO call from Time to Time ?? how ?? When ??
        private void ChunkBufferCleanup()
        {
            IEnumerable<long> expiredIndex = _receivedServerChunks.Where(x => DateTime.Now.Subtract(x.Value.MessageRecTime).TotalSeconds > 10).Select(x => x.Key);
            foreach (var index in expiredIndex) _receivedServerChunks.Remove(index);
        }

        //Auto generate a chunk
        private void CreateLandscapeFromGenerator(VisualChunk chunk, bool Async)
        {
            //1) Request Server the chunk
            //2) If chunk is a "pure" chunk on the server, then generate it localy.
            //2b) If chunk is not pure, we will have received the data inside a "GeneratedChunk" that we will copy inside the big buffe array.
            if (Async)
            {
                WorkQueue.DoWorkInThread(new WorkItemCallback(createLandScape_threaded), chunk, chunk as IThreadStatus, chunk.ThreadPriority);
            }
            else
            {
                _createLandScapeDelegate.Invoke(chunk);
            }

            chunk.RefreshBorderChunk();
        }

        //Create the landscape for the chunk
        private object createLandScape_threaded(object chunk)
        {
            VisualChunk visualChunk = (VisualChunk)chunk;
            GeneratedChunk generatedChunk = _worldGenerator.GetChunk(visualChunk.ChunkPosition);
            visualChunk.BlockData.SetBlockBytes(generatedChunk.BlockData.GetBlocksBytes());

            visualChunk.State = ChunkState.LandscapeCreated;
            visualChunk.ThreadStatus = ThreadStatus.Idle;

            return null;
        }

        #endregion
    }
}
