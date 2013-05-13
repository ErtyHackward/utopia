using System;
using System.Collections.Generic;
using S33M3CoreComponents.Physics;
using S33M3CoreComponents.Physics.Verlet;
using SharpDX;
using Utopia.Server.AStar;
using Utopia.Shared.Entities.Concrete;
using Utopia.Shared.Entities.Dynamic;
using Utopia.Shared.Entities.Interfaces;
using Utopia.Shared.Structs;
using S33M3Resources.Structs;

namespace Utopia.Server.Structs
{
    /// <summary>
    /// Basic NPC server logic
    /// </summary>
    public class Npc : ServerDynamicEntity, INpc
    {
        public static Vector3D CubeCenter = new Vector3D(0.5d, 0.0d, 0.5d);
        public static Vector3D Near = new Vector3D(0.02d);

        private readonly Server _server;
        private List<MapArea> _mapAreas = new List<MapArea>();
        private Vector3D _moveDirection;
        private bool _jump = false;

        private int _seed;

        private int _checkCounter = 0;

        private IDynamicEntity _target;
        private Random _random;

        private Path3D _path;
        private int _targetPathNodeIndex = -1;
        Vector3D _pathTargetPoint;
        private double _moveValue;
        private Vector3D _prevPoint;
        
        /// <summary>
        /// Gets current NPC state
        /// </summary>
        public NpcState State { get; private set; }

        public VerletSimulator VerletSimulator { get; private set; }
        
        public Vector3D MoveVector
        {
            get { return _moveDirection; }
            set { _moveDirection = value; }
        }

        public int Seed
        {
            get { return _seed; }
            set
            {
                _seed = value;
                _random = new Random(_seed);
                _checkCounter = _random.Next(0, 20);
            }
        }
        
        public Npc(Server server, Dwarf z) : base(z)
        {
            _server = server;
            Seed = 0;

            var size = z.DefaultSize;

            var bb = new BoundingBox(Vector3.Zero, size);

            size.Y = 0;

            bb = bb.Offset(-size / 2);
            
            VerletSimulator = new VerletSimulator(ref bb);
            VerletSimulator.ConstraintFct += _server.LandscapeManager.IsCollidingWithTerrain;
            VerletSimulator.StartSimulation(z.Position);
        }

        public void Goto(Vector3I location)
        {
            _server.LandscapeManager.CalculatePathAsync(DynamicEntity.Position.ToCubePosition(), location, PathCalculated);
        }

        private void PathCalculated(Path3D path)
        {
            if (path.Exists)
            {
                _path = path;
#if DEBUG
                _server.ChatManager.Broadcast(string.Format("Path found at {0} ms {1} iterations", _path.PathFindTime, _path.IterationsPerformed));
#endif

                // fix the path
                for (int i = 0; i < _path.Points.Count - 1; i++)
                {
                    var curPoint = _path.Points[i];
                    var nextPoint = _path.Points[i + 1];

                    //if (curPoint.Y != nextPoint.Y && (curPoint.X != nextPoint.X || curPoint.Z != nextPoint.Z))
                    //{
                    //    Vector3I pos;
                    //    // add intermediate point
                    //    if (nextPoint.Y < curPoint.Y)
                    //    {
                    //        // falling down
                    //        pos = new Vector3I(nextPoint.X, curPoint.Y, nextPoint.Z);
                    //    }
                    //    else
                    //    {
                    //        // lifting up
                    //        pos = new Vector3I(curPoint.X, nextPoint.Y + 1, curPoint.Z);
                    //    }
                    //    _path.Points.Insert(i + 1, pos);
                    //    i++;
                    //}
                }
                
                State = NpcState.FollowingPath;
                _targetPathNodeIndex = 0;
                _moveValue = 0;
                _prevPoint = DynamicEntity.Position;
                _pathTargetPoint = new Vector3D(_path.Points[1].X, _path.Points[1].Y, _path.Points[1].Z) + CubeCenter;
                _moveDirection = _pathTargetPoint - DynamicEntity.Position;
                _moveDirection.Normalize();
                var q = Quaternion.RotationMatrix(Matrix.LookAtLH(DynamicEntity.Position.AsVector3(),
                                                              DynamicEntity.Position.AsVector3() +
                                                              _moveDirection.AsVector3(), Vector3D.Up.AsVector3()));
                DynamicEntity.HeadRotation = q;
                //Transform the rotation from a world rotatino to a local rotation
            }
            else
            {
                _server.ChatManager.Broadcast("there is no path there...");
            }
        }

        public override void AddArea(MapArea area)
        {
            _mapAreas.Add(area);
        }

        public override void RemoveArea(MapArea area)
        {
            _mapAreas.Remove(area);
        }

        private void FollowNextPoint()
        {
            if (++_targetPathNodeIndex >= _path.Points.Count)
            {
                State = NpcState.Idle;
                return;
            }

            var vec3d = _path.Points[_targetPathNodeIndex];

            var dynPos = DynamicEntity.Position;

            _prevPoint = _pathTargetPoint;
            _pathTargetPoint = new Vector3D(vec3d.X, vec3d.Y, vec3d.Z) + CubeCenter;
            _moveDirection = _pathTargetPoint - dynPos;
            _jump = _moveDirection.Y > 0;
            _moveDirection.Y = 0;
            _moveDirection.Normalize();
            
            if (Math.Abs(_moveDirection.Y) < 0.1f)
            {
                var q = Quaternion.RotationMatrix(Matrix.LookAtLH(dynPos.AsVector3(),
                                                                  _pathTargetPoint.AsVector3(),
                                                                  Vector3D.Up.AsVector3()));
                DynamicEntity.HeadRotation = q;
                //Transform the rotation from a world rotatino to a local rotation
            }
            _moveValue -= 1;
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

            var elapsedS = (float)_server.Clock.GameToReal(gameTime.ElapsedTime).TotalSeconds;

            Vector3D newPos;
            VerletSimulator.Simulate(elapsedS, out newPos);
            DynamicEntity.Position = newPos;

            VerletSimulator.CurPosition = DynamicEntity.Position;

            if (State == NpcState.FollowingPath)
            {
                if (Vector3D.DistanceSquared(_pathTargetPoint, DynamicEntity.Position) < 0.1d)
                    FollowNextPoint();

                _moveDirection = _pathTargetPoint - DynamicEntity.Position;

                _jump = _moveDirection.Y > 0;
                _moveDirection.Y = 0;

                //if (Vector3D.DistanceSquared(VerletSimulator.PrevPosition, VerletSimulator.CurPosition) < 0.01f)
                //{
                //    if (Math.Abs(_moveDirection.X) < Math.Abs(_moveDirection.Z))
                //        _moveDirection.Z = 0.1f * Math.Sign(_moveDirection.Z);
                //    else
                //        _moveDirection.X = 0.1f * Math.Sign(_moveDirection.X); ;

                //}

                _moveDirection.Normalize();

                VerletSimulator.Impulses.Add(new Impulse(elapsedS) { ForceApplied = _moveDirection.AsVector3() * 1.6f });

                if (_jump && VerletSimulator.OnGround)
                    VerletSimulator.Impulses.Add(new Impulse(elapsedS) { ForceApplied = Vector3.UnitY * 22 });
            }
            else
            {
                if (_target != null)
                {
                    if (Vector3D.Distance(_target.Position, DynamicEntity.Position) < 10)
                    {
                        _moveDirection = _target.Position - DynamicEntity.Position;
                        _moveDirection.Normalize();
                        DynamicEntity.HeadRotation = Quaternion.RotationMatrix(Matrix.LookAtLH(DynamicEntity.Position.AsVector3(), DynamicEntity.Position.AsVector3() + _moveDirection.AsVector3(), Vector3D.Up.AsVector3()));
                    }
                    else
                    {
                        _target = null;
                    }
                }
                else
                {
                    if (_checkCounter++ > 10)
                    {
                        _checkCounter = 0;
                        //// try to find target
                        //_mapAreas.Find(area =>
                        //                   {
                        //                       foreach (var serverEntity in area.Enumerate())
                        //                       {
                        //                           if (serverEntity.GetType() != this.GetType() &&
                        //                               Vector3D.Distance(serverEntity.DynamicEntity.Position, DynamicEntity.Position) < 10)
                        //                           {
                        //                               _target = serverEntity.DynamicEntity;
                        //                               return true;
                        //                           }
                        //                       }
                        //                       return false;
                        //                   });
                    }
                }
            }


        }
    }
}
