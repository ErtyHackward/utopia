using System;
using System.Collections.Generic;
using SharpDX;
using Utopia.Server.AStar;
using Utopia.Server.Managers;
using Utopia.Server.Structs;
using Utopia.Shared.Chunks;
using Utopia.Shared.ClassExt;
using Utopia.Shared.Entities.Concrete;
using Utopia.Shared.Entities.Interfaces;
using Utopia.Shared.Structs;
using S33M3Resources.Structs;

namespace Utopia.Server.Entities
{
    /// <summary>
    /// Sample server mob
    /// </summary>
    public class TestNpc : ServerDynamicEntity
    {
        public static Vector3D CubeCenter = new Vector3D(0.5d, 0.0d, 0.5d);
        public static Vector3D Near = new Vector3D(0.02d);

        private readonly Server _server;
        private List<MapArea> _mapAreas = new List<MapArea>();
        private Vector3D _moveDirection;

        private int _seed;

        private int _checkCounter = 0;

        private IDynamicEntity _target;
        private Random _random;

        private Path3D _path;
        private int _targetPathNodeIndex = -1;
        Vector3D _pathTargetPoint;
        private double _moveValue;

        public TestNpcState State { get; set; }

        private Vector3D _prevPoint;

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
        
        public TestNpc(Server server, Zombie z) : base(z)
        {
            _server = server;
            Seed = 0;
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

                    if (curPoint.Y != nextPoint.Y && (curPoint.X != nextPoint.X || curPoint.Z != nextPoint.Z))
                    {
                        Vector3I pos;
                        // add intermediate point
                        if (nextPoint.Y < curPoint.Y)
                        {
                            // falling down
                            pos = new Vector3I(nextPoint.X, curPoint.Y, nextPoint.Z);
                        }
                        else
                        {
                            // lifting up
                            pos = new Vector3I(curPoint.X, nextPoint.Y+1, curPoint.Z);
                        }
                        _path.Points.Insert(i + 1, pos);
                        i++;
                    }
                }



                State = TestNpcState.FollowPath;
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
                State = TestNpcState.Staying;
                return;
            }

            var vec3d = _path.Points[_targetPathNodeIndex];

            var dynPos = DynamicEntity.Position;

            _prevPoint = _pathTargetPoint;
            _pathTargetPoint = new Vector3D(vec3d.X, vec3d.Y, vec3d.Z) + CubeCenter;
            _moveDirection = _pathTargetPoint - dynPos;
            _moveDirection.Normalize();

            if (Math.Abs(_moveDirection.Y) < 0.1f)
            {
                var q = Quaternion.RotationMatrix(Matrix.LookAtLH(dynPos.AsVector3(),
                                                                  dynPos.AsVector3() +
                                                                  _moveDirection.AsVector3(),
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
            {
                return;
            }

            #region Falling
            var current = _server.LandscapeManager.GetCursor(DynamicEntity.Position);
            if (State == TestNpcState.Staying && !current.PeekProfile(Vector3I.Down).IsSolidToEntity)
            {
                var pos = DynamicEntity.Position;
                pos.Y = Math.Round(DynamicEntity.Position.Y);
                pos += new Vector3D(0, -1, 0);
                DynamicEntity.Position = pos;
            }
            #endregion

            if (State == TestNpcState.FollowPath)
            {
                _moveValue += _server.Clock.GameToReal(gameTime.ElapsedTime).TotalSeconds * 3;
                if (_moveValue >= 1)
                {
                    FollowNextPoint();
                }
                DynamicEntity.Position = Vector3D.Lerp(_prevPoint, _pathTargetPoint, _moveValue);
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
                        // try to find target
                        _mapAreas.Find(area =>
                                           {
                                               foreach (var serverEntity in area.Enumerate())
                                               {
                                                   if (serverEntity.GetType() != this.GetType() &&
                                                       Vector3D.Distance(serverEntity.DynamicEntity.Position, DynamicEntity.Position) < 10)
                                                   {
                                                       _target = serverEntity.DynamicEntity;
                                                       return true;
                                                   }
                                               }
                                               return false;
                                           });
                    }
                }
            }


            #region old
            //if (gameTime.ElapsedTime.TotalSeconds < 2)
            //    return;
            //if (gameTime.ElapsedTime.TotalSeconds > 100)
            //{
            //    return;
            //}

            //if (_target != null)
            //{
            //    if (DVector3.Distance(_target.Position, DynamicEntity.Position) < 10)
            //    {
            //        _moveDirection = new Vector2((float)(_target.Position.X - DynamicEntity.Position.X),
            //                                     (float)(_target.Position.Z - DynamicEntity.Position.Z));
            //        _moveDirection.Normalize();
            //        DynamicEntity.Rotation = Quaternion.RotationMatrix(Matrix.LookAtRH(DynamicEntity.Position.AsVector3(), DynamicEntity.Position.AsVector3() + new Vector3(_moveDirection.X, 0, _moveDirection.Y), DVector3.Up.AsVector3()));
            //        DynamicEntity.Rotation = Quaternion.Invert(DynamicEntity.Rotation); //Transform the rotation from a world rotatino to a local rotation
            //    }
            //    else
            //    {
            //        _target = null;
            //        _moveDirection = Vector2.Zero;
            //        _moveDirection.Normalize();
            //    }
            //}
            //else
            //{
            //    if (_checkCounter++ > 20)
            //    {
            //        _checkCounter = 0;
            //        // try to find target
            //        _mapAreas.Find(area =>
            //                           {
            //                               foreach (var serverEntity in area.Enumerate())
            //                               {
            //                                   if (serverEntity.GetType() != this.GetType() &&
            //                                       DVector3.Distance(serverEntity.DynamicEntity.Position, DynamicEntity.Position) < 10)
            //                                   {
            //                                       _target = serverEntity.DynamicEntity;
            //                                       return true;
            //                                   }
            //                               }
            //                               return false;
            //                           });
            //    }
            //}

            //if(_moveDirection.X == 0 && _moveDirection.Y == 0)
            //{
            //    _moveDirection = new Vector2(_random.Next(-100, 100) / 100f, _random.Next(-100, 100) / 100f);
            //    _moveDirection.Normalize();
            //    DynamicEntity.Rotation = Quaternion.RotationMatrix(Matrix.LookAtRH(DynamicEntity.Position.AsVector3(), DynamicEntity.Position.AsVector3() + new Vector3(_moveDirection.X, 0, _moveDirection.Y), DVector3.Up.AsVector3()));
            //    DynamicEntity.Rotation = Quaternion.Invert(DynamicEntity.Rotation);
            //}

            //var nextPosition = DynamicEntity.Position + new DVector3(_moveDirection.X, 0, _moveDirection.Y) * _server.Clock.GameToReal(gameTime.ElapsedTime).TotalSeconds * 1.5;

            //// check if we can go to desired position
            //var cursor = _server.LandscapeManager.GetCursor(nextPosition);

            //var next = cursor.Value;
            //var nextDown = cursor.PeekDown();
            //var nextUp = cursor.PeekUp();

            //var current = _server.LandscapeManager.GetCursor(DynamicEntity.Position);
            //if (!RealmConfiguration.CubeProfiles[current.PeekDown()].IsSolidToEntity)
            //{
            //    var pos = DynamicEntity.Position;
            //    pos.Y = Math.Round(DynamicEntity.Position.Y);
            //    pos += new DVector3(0, -1, 0);
            //    DynamicEntity.Position = pos;
            //}


            //// next down cube should be solid, and two upper cubes should be transparent
            //if (!RealmConfiguration.CubeProfiles[next].IsSolidToEntity && RealmConfiguration.CubeProfiles[nextDown].IsSolidToEntity && !RealmConfiguration.CubeProfiles[nextUp].IsSolidToEntity)
            //{
            //    // move 
            //    DynamicEntity.Position = nextPosition;
            //}
            //else
            //{
            //    _moveDirection = new Vector2();
            //}
            #endregion
        }
    }
}
