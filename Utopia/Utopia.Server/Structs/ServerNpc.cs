﻿using System;
using System.Collections.Generic;
using System.Linq;
using Utopia.Server.AStar;
using Utopia.Shared.Configuration;
using Utopia.Shared.Entities.Concrete;
using Utopia.Shared.Entities.Dynamic;
using Utopia.Shared.Entities.Interfaces;
using Utopia.Shared.Entities.Inventory;
using Utopia.Shared.Structs;
using S33M3Resources.Structs;

namespace Utopia.Server.Structs
{
    /// <summary>
    /// Basic NPC server logic
    /// </summary>
    public class ServerNpc : ServerDynamicEntity, INpc
    {
        public static Vector3D Near = new Vector3D(0.02d);
        private List<MapArea> _mapAreas = new List<MapArea>();
        private int _seed;
        private Random _random;

        private CharacterEntity _character;

        /// <summary>
        /// Indicates if the npc is going to the aim (true) or alredy near it (false)
        /// </summary>
        public bool Coming { get; set; }

        /// <summary>
        /// Gets current NPC state
        /// </summary>
        public ServerNpcState State { get; private set; }
        
        public MoveAI Movement { get; private set; }

        IMoveAI INpc.Movement
        {
            get { return Movement; }
        }
        
        public FocusAI Focus { get; private set; }

        public Random Random { get { return _random; } }

        /// <summary>
        /// Gets wrapped character
        /// </summary>
        public CharacterEntity Character { get { return _character; } }

        public int Seed
        {
            get { return _seed; }
            set
            {
                _seed = value;
                _random = new Random(_seed);
            }
        }

        public ServerNpc(Server server, CharacterEntity z)
            : base(server, z)
        {
            Seed = 0;

            _character = z;
            _random = new Random();

            Movement = new MoveAI(this);
            Focus = new FocusAI(this);
        }
        
        public override void AddArea(MapArea area)
        {
            _mapAreas.Add(area);
        }

        public override void RemoveArea(MapArea area)
        {
            _mapAreas.Remove(area);
        }
        
        /// <summary>
        /// Perform AI operations...
        /// </summary>
        /// <param name="gameTime"></param>
        public override void Update(DynamicUpdateState gameTime)
        {
            if (gameTime.ElapsedTime.TotalSeconds < 2)
                return;
            if (gameTime.ElapsedTime.TotalSeconds > 100)
                return;

            Movement.Update(gameTime);
            Focus.Update(gameTime);

            AISelect();
        }

        private bool EquipItem<T>() where T : Item
        {
            if (_character.Equipment.RightTool is T)
                return true;

            // we have a job guys!
            var collectorSlot = _character.Slots().FirstOrDefault(s => s.Item is T);

            if (collectorSlot == null)
                return false; // but we haven't tools to do it

            ContainedSlot slot;

            if (!_character.Equipment.Equip(EquipmentSlotType.Hand, collectorSlot, out slot))
                return false;

            if (slot != null)
                _character.Equipment.PutItem(slot.Item, slot.ItemsCount);

            return true;            
        }

        /// <summary>
        /// Choose what to do next
        /// </summary>
        private void AISelect()
        {
            if (State == ServerNpcState.Idle)
            {
                // we need to find a job
                
            }

            switch (State)
            {
                case ServerNpcState.Idle:
                    break;
                case ServerNpcState.UsingItem:
                    break;
                case ServerNpcState.UsingBlock:
                    break;
                case ServerNpcState.Following:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            if (!Movement.IsActive)
            {
                if (Faction.BlocksToRemove.Count > 0)
                {
                    if (!EquipItem<BasicCollector>())
                        return;
                    
                    Vector3I pos;

                    // check whether we close enough to start working
                    if (State != ServerNpcState.Idle && Movement.CurrentPath != null && Movement.CurrentPath.Exists && IsGoalNodeNear(_character.Position.ToCubePosition(), out pos))
                    {
                        if (State == ServerNpcState.GoingToWork)
                        {
                            DynamicEntity.EntityState.IsBlockPicked = true;
                            DynamicEntity.EntityState.PickedBlockPosition = pos;
                            DynamicEntity.EntityState.MouseUp = false;

                            var tool = (BasicCollector)collectorSlot.Item;
                            State = ServerNpcState.Digging;
                            
                            // start digging this block
                            DynamicEntity.ToolUse(tool);
                            Focus.LookAt(pos);
                        }

                        if (State == ServerNpcState.Digging)
                        {
                            var cursor = Server.LandscapeManager.GetCursor(DynamicEntity.EntityState.PickedBlockPosition);

                            if (cursor.Read() == WorldConfiguration.CubeId.Air)
                            {
                                // stop digging

                                DynamicEntity.EntityState.IsBlockPicked = false;
                                DynamicEntity.EntityState.PickedBlockPosition = pos;
                                DynamicEntity.EntityState.MouseUp = true;
                                
                                var tool = (BasicCollector)collectorSlot.Item;
                                DynamicEntity.ToolUse(tool);

                                Faction.BlocksToRemove.Remove(pos);
                                State = ServerNpcState.Idle;
                            }
                        }
                    }
                    else
                    {
                        // will try to go to the closest block to remove

                        State = ServerNpcState.GoingToWork;
                        var location = Faction.BlocksToRemove.OrderBy(v => Vector3I.DistanceSquared(v, (Vector3I)_character.Position)).First();
                        Movement.Goto(location, IsGoalNode);
                    }
                }
            }
        }

        private bool IsGoalNode(AStarNode3D node)
        {
            var pos = node.Cursor.GlobalPosition;
            Vector3I res;
            return IsGoalNodeNear(pos, out res);
        }

        private bool IsGoalNodeNear(Vector3I pos, out Vector3I result)
        {
            for (int x = -1; x <= 1; x++)
            {
                for (int z = -1; z <= 1; z++)
                {
                    if (x != 0 && z != 0)
                        continue;

                    for (int y = 0; y <= 1; y++)
                    {
                        result = pos + new Vector3I(x, y, z);
                        if (Faction.BlocksToRemove.Contains(result))
                            return true;
                    }
                }
            }

            result = pos + new Vector3I(0, 2, 0);
            if (Faction.BlocksToRemove.Contains(result))
                return true;

            return false;
        }
    }
}
