using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using S33M3CoreComponents.Maths;
using S33M3Resources.Structs;
using SharpDX;
using Utopia.Shared.Chunks;
using Utopia.Shared.ClassExt;
using Utopia.Shared.Configuration;
using Utopia.Shared.Entities;
using Utopia.Shared.Entities.Concrete;
using Utopia.Shared.Entities.Dynamic;
using Utopia.Shared.Entities.Interfaces;
using Utopia.Shared.Entities.Inventory;
using Utopia.Shared.Interfaces;
using Utopia.Shared.Server.AStar;
using Utopia.Shared.Structs;
using Utopia.Shared.Structs.Helpers;

namespace Utopia.Shared.Server.Structs
{
    /// <summary>
    /// Basic NPC server logic
    /// </summary>
    public class ServerNpc : ServerDynamicEntity, INpc
    {
        public static Vector3D Near = new Vector3D(0.02d);
        private readonly List<MapArea> _mapAreas = new List<MapArea>();
        private int _seed;
        private Random _random;

        private readonly CharacterEntity _character;
        private Designation _designation;
        private Stopwatch _timer;

        private int _randomRadius = 16;

        /// <summary>
        /// Indicates if the npc is going to the aim (true) or alredy near it (false)
        /// Used as a trigger to start action when the NPC will reach the target
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

        /// <summary>
        /// Gets or sets current designation
        /// </summary>
        public Designation Designation
        {
            get { return _designation; }
            set {

                if (_designation == value)
                    return;

                if (_designation != null)
                {
                    _designation.Owner = 0;
                }

                if (value != null && value.Owner != 0 && value.Owner != _character.DynamicId)
                    throw new InvalidOperationException("Current designation is already assigned to someone else");

                _designation = value;
                
                if (_designation != null)
                {
                    _designation.Owner = _character.DynamicId;
                }
            }
        }

        public int Seed
        {
            get { return _seed; }
            set
            {
                _seed = value;
                _random = new Random(_seed);
            }
        }

        public ServerNpc(ServerCore server, CharacterEntity z)
            : base(server, z)
        {
            Seed = 0;

            _character = z;

            _random = new Random();
            _timer = new Stopwatch();

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
            DoAction();
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
            if (State != ServerNpcState.Idle)
                return;

            if (_random.NextDouble() < 0.3)
            {
                State = ServerNpcState.Walking;
                GoToRandomPoint();
                return;
            }

            State = ServerNpcState.LookingAround;
            _timer.Restart();
        }

        private void GoToRandomPoint()
        {
            _timer.Restart();

            var moveVector = _random.NextVector2IOnRadius(16);
            var curPos = BlockHelper.EntityToBlock(Character.Position);
            var movePos = curPos + new Vector3I(moveVector.X, 0, moveVector.Y);

            var cursor = Server.LandscapeManager.GetCursor(movePos);
            
            for (int m = 0; m < 4; m++)
            {
                var checkVectorUp = movePos + new Vector3I(0, m, 0);
                var checkVectorDown = movePos + new Vector3I(0, -m, 0);

                if (checkVectorUp.Y < AbstractChunk.ChunkSize.Y - 1)
                {
                    if (CanStandThere(cursor, checkVectorUp))
                    {
                        Movement.Goto(checkVectorUp);

                        if (_randomRadius < 16)
                            _randomRadius++;

                        return;
                    }
                }

                if (checkVectorDown.Y > 1)
                {
                    if (CanStandThere(cursor, checkVectorDown))
                    {
                        Movement.Goto(checkVectorDown);

                        if (_randomRadius < 16)
                            _randomRadius++;

                        return;
                    }
                }
            }

            if (_randomRadius > 2)
                _randomRadius--;

            

        }

        private bool CanStandThere(ILandscapeCursor cursor, Vector3I pos)
        {
            cursor.GlobalPosition = pos;
            
            return  cursor.Read() == WorldConfiguration.CubeId.Air &&
                    cursor.PeekValue(Vector3I.Up) == WorldConfiguration.CubeId.Air && 
                    cursor.PeekValue(Vector3I.Down) != WorldConfiguration.CubeId.Air;
        }

        /// <summary>
        /// Perform choosed action
        /// </summary>
        private void DoAction()
        {
            if (State == ServerNpcState.Idle)
                return;

            switch (State)
            {
                case ServerNpcState.Walking:
                    
                    if (!Movement.IsActive || _timer.Elapsed.TotalSeconds > 30)
                    {
                        State = ServerNpcState.Idle;
                    }
                    
                    break;
                case ServerNpcState.LookingAround:

                    if (Focus.Target == null || _timer.Elapsed.TotalSeconds > 10)
                    {
                        if (Focus.Target != null)
                        {
                            if (_random.NextDouble() < 0.3)
                            {
                                State = ServerNpcState.Idle;
                                return;
                            }
                        }

                        // find the entity to look at

                        foreach (var serverDynamicEntity in _server.AreaManager.EnumerateAround(Character.Position, 16).Where(e => e.DynamicEntity != Character))
                        {
                            if (_random.NextDouble() < 0.4)
                            {
                                Focus.LookAt(serverDynamicEntity.DynamicEntity);
                                return;
                            }
                        }

                        foreach (var staticEntity in _server.LandscapeManager.AroundEntities(Character.Position, 16))
                        {
                            if (_random.NextDouble() < 0.2)
                            {
                                Focus.LookAt(staticEntity);
                                return;
                            }
                        }
                    }
                    break;
                case ServerNpcState.UsingItem:
                    break;
                case ServerNpcState.UsingBlock:

                    if (Designation == null)
                    {
                        var designation = Faction.Designations.OfType<DigDesignation>().Where(d => d.Owner == 0).OrderBy(d => Vector3I.DistanceSquared(d.BlockPosition, (Vector3I)_character.Position)).First();
                        var path = Server.LandscapeManager.CalculatePath(_character.Position.ToCubePosition(), designation.BlockPosition, IsGoalNode);

                        if (path.Exists)
                        {
                            // reserve all nearby nodes
                            var near = DigDesignationsNear(path.Goal).ToArray();

                            Designation = near[0];

                            foreach (var des in near)
                            {
                                des.Owner = _character.DynamicId;
                            }

                            Movement.FollowPath(path);
                            Coming = true;
                        }
                        return;
                    }

                    // we need to wait until we will be at the position to start mining
                    if (Movement.IsActive)
                        return;

                    if (Coming)
                    {
                        Coming = false;

                        // in case user changed his mind to remove the block
                        if (!DigDesignationsNear(_character.Position.ToCubePosition(), _character.DynamicId).Any())
                        {
                            Designation = null;
                            State = ServerNpcState.Idle;
                            return;
                        }

                        var firstDes = DigDesignationsNear(_character.Position.ToCubePosition(), _character.DynamicId).First();
                        Designation = firstDes;

                        DynamicEntity.EntityState.IsBlockPicked = true;
                        DynamicEntity.EntityState.PickedBlockPosition = firstDes.BlockPosition;
                        DynamicEntity.EntityState.MouseUp = false;

                        var tool = (BasicCollector)_character.Equipment.RightTool;

                        // start digging this block
                        DynamicEntity.ToolUse(tool);
                        Focus.LookAt(firstDes.BlockPosition);
                        return;
                    }

                    var cursor = Server.LandscapeManager.GetCursor(DynamicEntity.EntityState.PickedBlockPosition);

                    if (cursor.Read() == WorldConfiguration.CubeId.Air)
                    {
                        // stop digging
                        DynamicEntity.EntityState.IsBlockPicked = false;
                        DynamicEntity.EntityState.MouseUp = true;

                        var tool = (BasicCollector)_character.Equipment.RightTool;
                        DynamicEntity.ToolUse(tool);

                        Faction.Designations.Remove(Designation);

                        if (DigDesignationsNear(_character.Position.ToCubePosition(), _character.DynamicId).Any())
                            Coming = true;
                        else
                            State = ServerNpcState.Idle;

                        Designation = null;
                    }

                    break;
                case ServerNpcState.Following:
                    break;
            }
        }

        private bool IsGoalNode(AStarNode3D node)
        {
            var pos = node.Cursor.GlobalPosition;
            return DigDesignationsNear(pos).Any();
        }

        private IEnumerable<DigDesignation> DigDesignationsNear(Vector3I pos, uint ownerId = 0)
        {
            Vector3I result;
            DigDesignation des;

            for (int x = -1; x <= 1; x++)
            {
                for (int z = -1; z <= 1; z++)
                {
                    if (x != 0 && z != 0)
                        continue;

                    for (int y = 0; y <= 1; y++)
                    {
                        result = pos + new Vector3I(x, y, z);
                        des = Faction.Designations.OfType<DigDesignation>().FirstOrDefault(d => d.Owner == ownerId && d.BlockPosition == result);
                        
                        if (des != null)
                            yield return des;
                    }
                }
            }

            result = pos + new Vector3I(0, 2, 0);

            des = Faction.Designations.OfType<DigDesignation>().FirstOrDefault(d => d.Owner == ownerId && d.BlockPosition == result);

            if (des != null)
                yield return des;
        }
    }
}
