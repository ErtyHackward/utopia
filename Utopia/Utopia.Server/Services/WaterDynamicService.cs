using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using S33M3Resources.Structs;
using Utopia.Shared.Chunks.Tags;
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
        
        private bool _updating = false;
        private Server _server;

        public override string ServiceName
        {
            get { return "Dynamic water"; }
        }

        public override void Initialize(Server server)
        {
            _server = server;
            _server.LandscapeManager.BlockChanged += LandscapeManagerBlockChanged;
            _server.Scheduler.AddTaskPeriodic("Water update", DateTime.Now, TimeSpan.FromSeconds(0.5), Update);
        }

        private void Update()
        {
            if (_updateList.Count == 0) return;

            

            lock (_updateList)
            {
                _updating = true;

                var node = _updateList.Last;

                var cursor = _server.LandscapeManager.GetCursor(new Vector3I());

                for (; node != null; node = node.Previous)
                {
                    cursor.GlobalPosition = node.Value;

                    var active = false;

                    var currentTag = (LiquidTag)cursor.ReadTag();

                    // check if this water can fall down
                    if (cursor.PeekDown() == CubeId.Air)
                    {
                        node.Value = cursor.GlobalPosition - new Vector3I(0, 1, 0);
                        _updateSet.Remove(cursor.GlobalPosition);
                        _updateSet.Add(cursor.GlobalPosition - new Vector3I(0, 1, 0));
                        continue;
                    }

                    // water can not fall, try to spread in sides
                    //CanFlowTo(currentTag.Pressure, new Vector3I(0, -1, 0));

                }



                _updating = false;
            }
        }
        
        void LandscapeManagerBlockChanged(object sender, Shared.Chunks.ChunkDataProviderDataChangedEventArgs e)
        {
            lock (_updateList)
            {
                // don't handle our own changes
                if (_updating) return;

                for (int i = 0; i < e.Count; i++)
                {
                    // check if new water block was created
                    if (e.Bytes[i] == CubeId.DynamicWater)
                    {
                        AddToUpdateList(e.Locations[i]);
                    }

                    // check if we touch surrounding water block
                    var cursor = _server.LandscapeManager.GetCursor(e.Locations[i]);

                    // up
                    CheckDirection(cursor, new Vector3I(0, 1, 0));

                    // sides
                    CheckDirection(cursor, new Vector3I(1, 0, 0));
                    CheckDirection(cursor, new Vector3I(-1, 0, 0));
                    CheckDirection(cursor, new Vector3I(0, 0, 1));
                    CheckDirection(cursor, new Vector3I(0, 0, -1));

                    // down
                    CheckDirection(cursor, new Vector3I(0, -1, 0));
                }
            }
        }

        private void CheckDirection(ILandscapeCursor cursor, Vector3I move)
        {
            if (cursor.PeekValue(move) == CubeId.DynamicWater)
                AddToUpdateList(move);
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
            _server.Scheduler.RemoveByName("Water update");
        }
    }
}
