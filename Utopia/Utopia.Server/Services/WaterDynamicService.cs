using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using S33M3Resources.Structs;
using Utopia.Server.Managers;
using Utopia.Server.Utils;
using Utopia.Shared.Chunks;
using Utopia.Shared.Cubes;
using Utopia.Shared.Interfaces;

namespace Utopia.Server.Services
{
    /// <summary>
    /// Handles dynamic water
    /// </summary>
    public class WaterDynamicService : Service
    {
        private LinkedList<Vector3I> _updateList = new LinkedList<Vector3I>();
        private HashSet<Vector3I> _updateSet = new HashSet<Vector3I>();
        private Dictionary<Vector2I, InsideDataProvider> _affectedChunks = new Dictionary<Vector2I, InsideDataProvider>();


        private bool _updating = false;
        private Server _server;
        private Timer _updateTimer;

        public override string ServiceName
        {
            get { return "Dynamic water"; }
        }

        public override void Initialize(Server server)
        {
            _server = server;
            _server.LandscapeManager.BlockChanged += LandscapeManagerBlockChanged;
            _updateTimer = new Timer(o => Update(), null, 0, 500);
        }

        private void Update()
        {
            if (_updateList.Count == 0) return;

            if (_updating) return;

            lock (_updateList)
            {
                _updating = true;
                var sw = Stopwatch.StartNew();
                
                var node = _updateList.First;

                var cursor = _server.LandscapeManager.GetCursor(new Vector3I());
                cursor.BeforeWrite += CursorBeforeWrite;

                for (; node != null; node = node.Next)
                {
                    cursor.GlobalPosition = node.Value;

                    //var currentTag = (LiquidTag)cursor.ReadTag();

                    // check if this water can fall down
                    if (TryFallTo(node, cursor, new Vector3I(0, -1, 0)))
                        continue;

                    // fall in sides
                    if (cursor.PeekValue(new Vector3I(1, 0, 0)) == CubeId.Air && TryFallTo(node, cursor, new Vector3I(1, -1, 0)))
                        continue;
                    if (cursor.PeekValue(new Vector3I(-1, 0, 0)) == CubeId.Air && TryFallTo(node, cursor, new Vector3I(-1, -1, 0)))
                        continue;
                    if (cursor.PeekValue(new Vector3I(0, 0, 1)) == CubeId.Air && TryFallTo(node, cursor, new Vector3I(0, -1, 1)))
                        continue;
                    if (cursor.PeekValue(new Vector3I(0, 0, -1)) == CubeId.Air && TryFallTo(node, cursor, new Vector3I(0, -1, -1)))
                        continue;

                    _updateSet.Remove(node.Value);
                    _updateList.Remove(node);
                    
                }

                cursor.BeforeWrite -= CursorBeforeWrite;

                // commit all changes
                foreach (var pair in _affectedChunks)
                {
                    pair.Value.CommitTransaction();
                }

                _affectedChunks.Clear();

                _updating = false;
                Console.WriteLine("Water cycle update " + sw.Elapsed.TotalMilliseconds + " ms");
            }
        }

        void CursorBeforeWrite(object sender, LandscapeCursorBeforeWriteEventArgs e)
        {
            var chunkPos = BlockHelper.BlockToChunkPosition(e.GlobalPosition);
            if (!_affectedChunks.ContainsKey(chunkPos))
            {
                var chunk = _server.LandscapeManager.GetChunk(chunkPos);
                var chunkDataProvider = (InsideDataProvider)chunk.BlockData;

                chunkDataProvider.BeginTransaction();

                _affectedChunks.Add(chunkPos, chunkDataProvider);
            }
        }

        bool TryFallTo(LinkedListNode<Vector3I> node, ILandscapeCursor cursor, Vector3I move)
        {
            if (cursor.PeekValue(move) == CubeId.Air)
            {
                var prevPosition = cursor.GlobalPosition;

                node.Value = cursor.GlobalPosition + move;
                _updateSet.Remove(cursor.GlobalPosition);
                _updateSet.Add(node.Value);
                
                cursor.Write(CubeId.Air);
                cursor.Move(move).Write(CubeId.DynamicWater);

                PropagateUpdate(prevPosition);
                PropagateUpdate(node.Value);
                return true;
            }
            return false;
        }

        void LandscapeManagerBlockChanged(object sender, ServerLandscapeManagerBlockChangedEventArgs e)
        {
            lock (_updateList)
            {
                // don't handle our own changes
                if (_updating) return;
                
                for (int i = 0; i < e.Count; i++)
                {

                    // check if new water block was created
                    if (e.Values[i] == CubeId.DynamicWater)
                    {
                        AddToUpdateList(e.Locations[i]);
                    }

                    PropagateUpdate(e.Locations[i]);
                }
            }
        }

        private void PropagateUpdate(Vector3I globalPos)
        {
            // check if we touch surrounding water block
            var cursor = _server.LandscapeManager.GetCursor(globalPos);

            // up
            CheckDirection(cursor, new Vector3I(0, 1, 0));

            // sides
            CheckDirection(cursor, new Vector3I(1, 0, 0));
            CheckDirection(cursor, new Vector3I(-1, 0, 0));
            CheckDirection(cursor, new Vector3I(0, 0, 1));
            CheckDirection(cursor, new Vector3I(0, 0, -1));

            CheckDirection(cursor, new Vector3I(1, 1, 0));
            CheckDirection(cursor, new Vector3I(-1, 1, 0));
            CheckDirection(cursor, new Vector3I(0, 1, 1));
            CheckDirection(cursor, new Vector3I(0, 1, -1));

            // down
            CheckDirection(cursor, new Vector3I(0, -1, 0));
        }

        private void CheckDirection(ILandscapeCursor cursor, Vector3I move)
        {
            if (cursor.PeekValue(move) == CubeId.DynamicWater)
                AddToUpdateList(cursor.GlobalPosition + move);
        }

        private void AddToUpdateList(Vector3I vec)
        {
            if (!_updateSet.Contains(vec))
            {
                _updateList.AddLast(vec);
                _updateSet.Add(vec);
            }
        }

        public override void Dispose()
        {
            _server.LandscapeManager.BlockChanged -= LandscapeManagerBlockChanged;
            _updateTimer.Dispose();
        }
    }
}
