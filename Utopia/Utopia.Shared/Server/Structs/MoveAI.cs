using System;
using S33M3CoreComponents.Physics;
using S33M3CoreComponents.Physics.Verlet;
using S33M3Resources.Structs;
using SharpDX;
using Utopia.Shared.Entities;
using Utopia.Shared.Entities.Interfaces;
using Utopia.Shared.Server.AStar;
using Utopia.Shared.Structs;

namespace Utopia.Shared.Server.Structs
{
    /// <summary>
    /// Handles npc movement
    /// Calculates path from one point to another
    /// Allows to follow the leader etc
    /// </summary>
    public class MoveAI : IMoveAI
    {
        public static Vector3D CubeCenter = new Vector3D(0.5d, 0.0d, 0.5d);

        private readonly ServerNpc _parentNpc;
        
        private Path3D _path;
        private int _targetPathNodeIndex = -1;
        private Vector3D _pathTargetPoint;
        private Vector3D _prevPoint;
        private bool _jump;
        private Vector3D _moveDirection;
        private IDynamicEntity _leader;

        public ServerNpc Npc { get { return _parentNpc; } }

        public VerletSimulator VerletSimulator { get; private set; }

        public bool IsMoving { get; private set; }

        /// <summary>
        /// Active means that entity is waiting for a path or has a leader or just moving
        /// </summary>
        public bool IsActive { get { return IsMoving || WaitingForPath || _leader != null; } }

        public Path3D CurrentPath { get { return _path; } }

        /// <summary>
        /// Gets or sets the distance on which entity will stop (following mode)
        /// </summary>
        public float FollowKeepDistance { get; set; }

        /// <summary>
        /// Gets or sets the max distance from the leader to start the movement
        /// </summary>
        public float FollowStayDistance { get; set; }

        /// <summary>
        /// Indicates that path was requested and should arrive soon
        /// </summary>
        public bool WaitingForPath { get; private set; }

        /// <summary>
        /// Gets or sets the entity to follow
        /// </summary>
        public IDynamicEntity Leader
        {
            get { return _leader; }
            set { 
                _leader = value;

#if DEBUGPATHFIND
                if (_leader != null)
                {

                    Npc.Server.ChatManager.Broadcast("Following " + _leader.Name);
                }
#endif
            }
        }

        public Vector3D MoveVector
        {
            get { return _moveDirection; }
            set { _moveDirection = value; }
        }

        public MoveAI(ServerNpc parentNpc)
        {
            _parentNpc = parentNpc;
            
            var size = parentNpc.DynamicEntity.DefaultSize;
            var bb = new BoundingBox(Vector3.Zero, size);
            size.Y = 0;
            bb = bb.Offset(-size / 2);

            VerletSimulator = new VerletSimulator(ref bb);
            VerletSimulator.ConstraintFct += Npc.Server.LandscapeManager.IsCollidingWithTerrain;
            VerletSimulator.StartSimulation(parentNpc.DynamicEntity.Position);

            FollowKeepDistance = 3;
            FollowStayDistance = 5;
        }

        /// <summary>
        /// Forces the entity to find a path to the location or alternative locations
        /// And move there
        /// </summary>
        /// <param name="location"></param>
        /// <param name="isGoal"></param>
        public void Goto(Vector3I location, Predicate<AStarNode3D> isGoal)
        {
            _leader = null;
            Npc.Server.LandscapeManager.CalculatePathAsync(Npc.DynamicEntity.Position.ToCubePosition(), location, FollowPath, isGoal);
            WaitingForPath = true;
        }

        public void Goto(Vector3I location)
        {
            _leader = null;
            MoveTo(location);
        }

        private void MoveTo(Vector3I location)
        {
            Npc.Server.LandscapeManager.CalculatePathAsync(Npc.DynamicEntity.Position.ToCubePosition(), location, FollowPath);
            WaitingForPath = true;
        }
        
        public void FollowPath(Path3D path)
        {
            WaitingForPath = false;

            if (path.Exists)
            {
                _path = path;
#if DEBUGPATHFIND
                Npc.Server.ChatManager.Broadcast(string.Format("Path found at {0} ms {1} iterations", _path.PathFindTime, _path.IterationsPerformed));
#endif
                IsMoving = true;
                _targetPathNodeIndex = -1;
                _pathTargetPoint = Npc.DynamicEntity.Position;

                FollowNextPoint();
            }
            else
            {
#if DEBUGPATHFIND
                Npc.Server.ChatManager.Broadcast("there is no path there...");
#endif
            }
        }

        private void FollowNextPoint()
        {
            // check for the end of the path
            if (++_targetPathNodeIndex >= _path.Points.Count)
            {
                IsMoving = false;
                return;
            }

            // check for leader reaching
            if (_leader != null)
            {
                if (Vector3D.Distance(_leader.Position, Npc.DynamicEntity.Position) <= FollowKeepDistance)
                {
                    IsMoving = false;
                    return;
                }
            }

            var vec3d = _path.Points[_targetPathNodeIndex];

            var dynPos = Npc.DynamicEntity.Position;

            _prevPoint = _pathTargetPoint;
            _pathTargetPoint = new Vector3D(vec3d.X, vec3d.Y, vec3d.Z) + CubeCenter;
            _moveDirection = _pathTargetPoint - dynPos;
            _jump = _moveDirection.Y > 0;
            _moveDirection.Y = 0;
            _moveDirection.Normalize();

            Npc.Focus.LookAt(_pathTargetPoint);
        }

        public void Update(DynamicUpdateState gameTime)
        {
            var elapsedS = (float)gameTime.RealTime.TotalSeconds;

            Vector3D newPos;
            VerletSimulator.Simulate(elapsedS, out newPos);
            Npc.DynamicEntity.Position = newPos;

            VerletSimulator.CurPosition = Npc.DynamicEntity.Position;

            if (IsMoving)
            {
                if (Vector3D.DistanceSquared(_pathTargetPoint, Npc.DynamicEntity.Position) < 0.1d)
                {
                    FollowNextPoint();
                }

                _moveDirection = _pathTargetPoint - Npc.DynamicEntity.Position;

                _jump = _moveDirection.Y > 0;
                _moveDirection.Y = 0;

                //if (Vector3D.DistanceSquared(VerletSimulator.PrevPosition, VerletSimulator.CurPosition) < 0.01f)
                //{
                //    if (Math.Abs(_moveDirection.X) < Math.Abs(_moveDirection.Z))
                //        _moveDirection.Z = 0.1f * Math.Sign(_moveDirection.Z);
                //    else
                //        _moveDirection.X = 0.1f * Math.Sign(_moveDirection.X);
                //}

                _moveDirection.Normalize();

                VerletSimulator.Impulses.Add(new Impulse(elapsedS) { ForceApplied = _moveDirection.AsVector3() * Npc.DynamicEntity.MoveSpeed });

                if (_jump && VerletSimulator.OnGround)
                    VerletSimulator.Impulses.Add(new Impulse(elapsedS) { ForceApplied = Vector3.UnitY * 22 });
            }

            if (_leader != null && Vector3D.Distance(_leader.Position, Npc.DynamicEntity.Position) > FollowStayDistance)
            {
                if (IsMoving && Vector3D.Distance(new Vector3D(_path.Goal) + CubeCenter, _leader.Position) < FollowStayDistance)
                    return;

                MoveTo(_leader.Position.ToCubePosition());
            }
        }
    }
}