using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Threading;
using ProtoBuf;
using S33M3Resources.Structs;
using Utopia.Shared.Chunks;
using Utopia.Shared.Chunks.Tags;
using Utopia.Shared.Configuration;
using Utopia.Shared.Interfaces;
using Utopia.Shared.Server;
using Utopia.Shared.Services.Interfaces;
using Utopia.Shared.Structs.Helpers;
using System.Linq;

namespace Utopia.Shared.Services
{
    /// <summary>
    /// Handles dynamic water
    /// </summary>
    [ProtoContract]
    [Description("Provides water flow feature to the game")]
    public class WaterDynamicService : Service
    {
        private readonly LinkedList<Vector3I> _updateList = new LinkedList<Vector3I>();
        private readonly HashSet<Vector3I> _updateSet = new HashSet<Vector3I>();
        private readonly Dictionary<Vector3I, InsideDataProvider> _affectedChunks = new Dictionary<Vector3I, InsideDataProvider>();
        
        private bool _updating;
        private ServerCore _server;
        private Timer _updateTimer;

        private byte _stillWater;
        private byte _dynamicWater;

        private Vector3I[] directions = new[] 
        { 
            new Vector3I(1, 0, 0), 
            new Vector3I(0, 0, 1), 
            new Vector3I(-1, 0, 0), 
            new Vector3I(0, 0, -1) 
        };

        public override void Initialize(ServerCore server)
        {
            _server = server;
            _server.LandscapeManager.BlockChanged += LandscapeManagerBlockChanged;
            _updateTimer = new Timer(o => Update(), null, 0, 500);

            //If no water block define, stop the Water services !

            //Get Id of FIRST Liquid and Still cube from collection
            _stillWater = _server.WorldParameters.Configuration.BlockProfiles.Where(x => x.CubeFamilly == Shared.Enums.enuCubeFamilly.Liquid && x.IsTaggable == false).First().Id;
            //Get Id of FIRST Liquid and Dynamic cube from collection
            _dynamicWater = _server.WorldParameters.Configuration.BlockProfiles.Where(x => x.CubeFamilly == Shared.Enums.enuCubeFamilly.Liquid && x.IsTaggable == true).First().Id;
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

                var spreadList = new List<Vector3I>(6);
                var cursor = _server.LandscapeManager.GetCursor(new Vector3I());
                cursor.BeforeWrite += CursorBeforeWrite;

                for (; node != null; node = node.Next)
                {
                    cursor.GlobalPosition = node.Value;

                    var currentValue = cursor.Read();

                    if (currentValue != _dynamicWater)
                    {
                        var prevNode = node.Previous;
                        _updateList.Remove(node);
                        _updateSet.Remove(node.Value);
                        node = prevNode ?? _updateList.First;
                        if (node == null) break;
                        continue;
                    }

                    var currentTag = (LiquidTag)cursor.ReadTag() ?? new LiquidTag { Pressure = 10, Sourced = false };

                    // water always tries to spread until it have pressure lower than 0.11

                    #region falling
                    BlockTag tag;
                    var value = cursor.PeekValue(new Vector3I(0, -1, 0), out tag);

                    if (value == WorldConfiguration.CubeId.Air)
                    {
                        cursor.Write(WorldConfiguration.CubeId.Air);
                        cursor.Move(new Vector3I(0, -1, 0));
                        cursor.Write(_dynamicWater, currentTag);
                        PropagateUpdate(cursor.GlobalPosition);
                        continue;
                    }

                    if (value == _stillWater)
                    {
                        // disappear
                        cursor.Write(WorldConfiguration.CubeId.Air);
                        continue;
                    }

                    if (value == _dynamicWater)
                    {
                        var ltag = tag as LiquidTag;
                        // merge if possible
                        if (ltag != null && ltag.Pressure < 10)
                        {
                            var need = (ushort)(10 - ltag.Pressure);

                            if (need >= currentTag.Pressure)
                            {
                                need = currentTag.Pressure;
                                cursor.Write(WorldConfiguration.CubeId.Air);
                            }
                            else
                            {
                                currentTag.Pressure -= need;
                                cursor.Write(_dynamicWater, currentTag);
                            }
                            ltag.Pressure += need;
                            cursor.Move(new Vector3I(0, -1, 0)).Write(_dynamicWater, ltag);
                            PropagateUpdate(cursor.GlobalPosition);
                            continue;
                        }
                    }

                    #endregion

                    #region spreading

                    if (currentTag.Pressure > 1)
                    {
                        spreadList.Clear();
                        // detect all possible ways
                        foreach (var direction in directions)
                        {
                            if (CanFlowTo(cursor, direction, currentTag.Pressure))
                                spreadList.Add(direction);
                        }

                        // move up only if pressure is too high
                        if ((spreadList.Count + 1) * 10 < currentTag.Pressure && CanFlowTo(cursor, new Vector3I(0, 1, 0), currentTag.Pressure))
                            spreadList.Add(new Vector3I(0, 1, 0));

                        // oki spread

                        if (spreadList.Count > 0)
                        {
                            int outPressure = 0;
                            int limit = spreadList.Count + 1;

                            for (; outPressure == 0 && limit > 0; limit--)
                            {
                                outPressure = currentTag.Pressure / limit;
                            }

                            var r = new Random();
                            if (outPressure > 0)
                            {
                                for (int i = r.Next(0, limit), l = 0 ; l < limit; i++, l++) // a bit of voodoo ;)
                                {
                                    if (i == spreadList.Count) i = 0;
                                    var vector3I = spreadList[i];
                                    cursor.Move(vector3I);

                                    LiquidTag sideTag;

                                    if (cursor.Read(out sideTag) != _dynamicWater)
                                    {
                                        sideTag = new LiquidTag { Pressure = 0 };
                                    }
                                    else if (sideTag == null)
                                    {
                                        sideTag = new LiquidTag { Pressure = 1 };
                                    }

                                    var wave = outPressure;

                                    if (wave > currentTag.Pressure - sideTag.Pressure)
                                        wave = currentTag.Pressure - sideTag.Pressure;

                                    sideTag.Pressure = (ushort)(sideTag.Pressure + wave);
                                    currentTag.Pressure = (ushort)(currentTag.Pressure - wave);

                                    cursor.Write(_dynamicWater, sideTag);
                                    PropagateUpdate(cursor.GlobalPosition);
                                    // move cursor back
                                    cursor.Move(Vector3I.Zero - vector3I);
                                }

                                if (spreadList.Count > 0)
                                {
                                    cursor.Write(_dynamicWater, currentTag);
                                }

                                if (spreadList.Count != 0)
                                    continue;
                            }
                        }
                    }

                    #endregion

                    {
                        var prevNode = node.Previous;
                        _updateSet.Remove(node.Value);
                        _updateList.Remove(node);
                        node = prevNode ?? _updateList.First;
                        if (node == null) break;
                    }
                }

                cursor.BeforeWrite -= CursorBeforeWrite;

                Console.WriteLine("Water cycle update " + sw.Elapsed.TotalMilliseconds + " ms Items: " + _updateList.Count);

                // commit all changes
                foreach (var pair in _affectedChunks)
                {
                    pair.Value.CommitTransaction();
                }

                _affectedChunks.Clear();

                _updating = false;
                
            }
        }

        bool CanFlowTo(ILandscapeCursor cursor, Vector3I move, ushort pressure)
        {
            LiquidTag tag;
            var value = cursor.PeekValue(move, out tag);

            if (value == WorldConfiguration.CubeId.Air)
                return true;

            if (value == _dynamicWater)
            {
                if (tag == null)
                    return false;

                if (tag.Pressure + 1 < pressure)
                    return true;
                return false;
            }

            return false;
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
            if (cursor.PeekValue(move) == WorldConfiguration.CubeId.Air)
            {
                var prevPosition = cursor.GlobalPosition;

                node.Value = cursor.GlobalPosition + move;
                _updateSet.Remove(cursor.GlobalPosition);
                _updateSet.Add(node.Value);

                //cursor.Write(CubeId.DynamicWater, new LiquidTag { LiquidType = 0, Pressure = 0.5f, Sourced = false });
                //cursor.Move(move).Write(CubeId.DynamicWater, new LiquidTag { LiquidType = 0, Pressure = 0.5f, Sourced = false });

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
                    if (e.Values[i] == _dynamicWater)
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

            if (cursor.Read() == _dynamicWater)
                AddToUpdateList(globalPos);

            // up
            CheckDirection(cursor, new Vector3I(0, 1, 0));

            // sides
            CheckDirection(cursor, new Vector3I( 1, 0,  0));
            CheckDirection(cursor, new Vector3I(-1, 0,  0));
            CheckDirection(cursor, new Vector3I( 0, 0,  1));
            CheckDirection(cursor, new Vector3I( 0, 0, -1));

            //CheckDirection(cursor, new Vector3I( 1, 1,  0));
            //CheckDirection(cursor, new Vector3I(-1, 1,  0));
            //CheckDirection(cursor, new Vector3I( 0, 1,  1));
            //CheckDirection(cursor, new Vector3I( 0, 1, -1));

            // down
            CheckDirection(cursor, new Vector3I(0, -1, 0));
        }

        private void CheckDirection(ILandscapeCursor cursor, Vector3I move)
        {
            if (cursor.PeekValue(move) == _dynamicWater)
                AddToUpdateList(cursor.GlobalPosition + move);
        }

        private void AddToUpdateList(Vector3I vec)
        {
            if (!_updateSet.Contains(vec))
            {
                _updateList.AddFirst(vec);
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
