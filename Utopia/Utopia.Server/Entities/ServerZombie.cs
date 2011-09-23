﻿using System;
using System.Collections.Generic;
using S33M3Engines.Shared.Math;
using SharpDX;
using Utopia.Server.AStar;
using Utopia.Server.Managers;
using Utopia.Server.Structs;
using Utopia.Shared.Chunks.Entities.Concrete;
using Utopia.Shared.Chunks.Entities.Interfaces;
using Utopia.Shared.Structs;
using Utopia.Shared.Structs.Landscape;

namespace Utopia.Server.Entities
{
    /// <summary>
    /// Experimental server mob
    /// </summary>
    public class ServerZombie : ServerDynamicEntity
    {
        public static DVector3 CubeCenter = new DVector3(0.5d, 0.0d, 0.5d);

        private readonly Server _server;
        private List<MapArea> _mapAreas = new List<MapArea>();
        private DVector3 _moveDirection;

        private int _seed;

        private int _checkCounter = 0;

        private IDynamicEntity _target;
        private Random _random;

        private Path3D _path;
        private int _targetPathNodeIndex = -1;
        DVector3 _pathTargetPoint;

        public ZombieState State { get; set; }

        public DVector3 MoveVector
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
        
        public ServerZombie(Server server, Zombie z) : base(z)
        {
            _server = server;
            Seed = 0;
        }

        public void Goto(Location3<int> location)
        {
            _server.LandscapeManager.CalculatePathAsync(LandscapeManager.EntityToBlockPosition(this.DynamicEntity.Position), location, PathCalculated);
        }

        private void PathCalculated(Path3D path)
        {
            if (path.Exists)
                _path = path;

            State = ZombieState.FollowPath;
            _targetPathNodeIndex = 0;
            _pathTargetPoint = new DVector3(_path.Points[0].X, _path.Points[0].Y, _path.Points[0].Z) + CubeCenter;
            _moveDirection = _pathTargetPoint - DynamicEntity.Position;
            _moveDirection.Normalize();
            var q = Quaternion.RotationMatrix(Matrix.LookAtRH(DynamicEntity.Position.AsVector3(), DynamicEntity.Position.AsVector3() + _moveDirection.AsVector3(), DVector3.Up.AsVector3()));
            DynamicEntity.Rotation = Quaternion.Invert(q); //Transform the rotation from a world rotatino to a local rotation
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
            #region Falling
            var current = _server.LandscapeManager.GetCursor(DynamicEntity.Position);
            if (!current.IsSolidDown())
            {
                var pos = DynamicEntity.Position;
                pos.Y = Math.Round(DynamicEntity.Position.Y);
                pos += new DVector3(0, -1, 0);
                DynamicEntity.Position = pos;
            }
            #endregion

            if (State == ZombieState.FollowPath)
            {
                if (DynamicEntity.Position == _pathTargetPoint)
                {
                    if (_targetPathNodeIndex++ == _path.Points.Count)
                    {
                        State = ZombieState.Staying;
                        return;
                    }
                    else
                    {
                        var vec3d = _path.Points[_targetPathNodeIndex];
                        _pathTargetPoint = new DVector3(vec3d.X, vec3d.Y, vec3d.Z) + CubeCenter;
                        _moveDirection = _pathTargetPoint - DynamicEntity.Position;
                        _moveDirection.Normalize();
                        var q = Quaternion.RotationMatrix(Matrix.LookAtRH(DynamicEntity.Position.AsVector3(), DynamicEntity.Position.AsVector3() + _moveDirection.AsVector3(), DVector3.Up.AsVector3()));
                        DynamicEntity.Rotation = Quaternion.Invert(q); //Transform the rotation from a world rotatino to a local rotation
                    }
                }
                
                DynamicEntity.Position += _moveDirection * _server.Clock.GameToReal(gameTime.ElapsedTime).TotalSeconds * 1.5;
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
            //if (!CubeProfile.CubesProfile[current.PeekDown()].IsSolidToEntity)
            //{
            //    var pos = DynamicEntity.Position;
            //    pos.Y = Math.Round(DynamicEntity.Position.Y);
            //    pos += new DVector3(0, -1, 0);
            //    DynamicEntity.Position = pos;
            //}


            //// next down cube should be solid, and two upper cubes should be transparent
            //if (!CubeProfile.CubesProfile[next].IsSolidToEntity && CubeProfile.CubesProfile[nextDown].IsSolidToEntity && !CubeProfile.CubesProfile[nextUp].IsSolidToEntity)
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
