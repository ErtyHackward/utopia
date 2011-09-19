using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using S33M3Engines.Shared.Math;
using SharpDX;
using Utopia.Server.Structs;
using Utopia.Shared.Chunks.Entities;
using Utopia.Shared.Chunks.Entities.Interfaces;
using Utopia.Shared.Chunks.Entities.Management;
using Utopia.Shared.Structs;
using Utopia.Shared.Structs.Landscape;

namespace Utopia.Server.Entities
{
    /// <summary>
    /// Experimental server mob
    /// </summary>
    public class ServerZombie : CharacterEntity
    {
        private readonly Server _server;
        private List<MapArea> _mapAreas = new List<MapArea>();
        private Vector2 _moveDirection;

        private int _seed;

        private int _checkCounter = 0;

        private IDynamicEntity _target;
        private Random _random;

        public Vector2 MoveVector
        {
            get { return _moveDirection; }
            set { _moveDirection = value; }
        }

        public override EntityClassId ClassId
        {
            get { return EntityClassId.Zombie; }
        }

        public override string DisplayName
        {
            get { return "Zombie Bob"; }
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
        
        public ServerZombie(Server server, string name)
        {
            _server = server;
            CharacterName = name;
            Seed = 0;
        }

        public override void AddArea(MapArea area)
        {
            _mapAreas.Add(area);
        }

        public override void RemoveArea(MapArea area)
        {
            _mapAreas.Remove(area);
        }

        public override void Update(DynamicUpdateState gameTime)
        {
            if (gameTime.ElapsedTime.TotalSeconds < 2)
                return;
            if (gameTime.ElapsedTime.TotalSeconds > 100)
            {
                return;
            }

            if (_target != null)
            {
                if (DVector3.Distance(_target.Position, Position) < 10)
                {
                    _moveDirection = new Vector2((float)(_target.Position.X - Position.X),
                                                 (float)(_target.Position.Z - Position.Z));
                    _moveDirection.Normalize();
                    Rotation = Quaternion.RotationMatrix(Matrix.LookAtRH(Position.AsVector3(), Position.AsVector3() + new Vector3(_moveDirection.X, 0, _moveDirection.Y), DVector3.Up.AsVector3()));
                }
                else
                {
                    _target = null;
                    _moveDirection = Vector2.Zero;
                    _moveDirection.Normalize();
                }
            }
            else
            {
                if (_checkCounter++ > 20)
                {
                    _checkCounter = 0;
                    // try to find target
                    _mapAreas.Find(area =>
                                       {
                                           foreach (var dynamicEntity in area.Enumerate())
                                           {
                                               if (dynamicEntity.GetType() != this.GetType() &&
                                                   DVector3.Distance(dynamicEntity.Position, Position) < 10)
                                               {
                                                   _target = dynamicEntity;
                                                   return true;
                                               }
                                           }
                                           return false;
                                       });
                }
            }

            if(_moveDirection.X == 0 && _moveDirection.Y == 0)
            {
                _moveDirection = new Vector2(_random.Next(-100, 100) / 100f, _random.Next(-100, 100) / 100f);
                _moveDirection.Normalize();
                Rotation = Quaternion.RotationMatrix(Matrix.LookAtRH(Position.AsVector3(), Position.AsVector3() + new Vector3(_moveDirection.X, 0, _moveDirection.Y), DVector3.Up.AsVector3()));
            }

            var nextPosition = Position + new DVector3(_moveDirection.X, 0, _moveDirection.Y) * _server.Clock.GameToReal(gameTime.ElapsedTime).TotalSeconds * 1.5;

            // check if we can go to desired position
            var cursor = _server.LandscapeManager.GetCursor(nextPosition);

            var next = cursor.Value;
            var nextDown = cursor.PeekDown();
            var nextUp = cursor.PeekUp();

            var current = _server.LandscapeManager.GetCursor(Position);
            if (!CubeProfile.CubesProfile[current.PeekDown()].IsSolidToEntity)
            {
                var pos = Position;
                pos.Y = Math.Round(Position.Y);
                pos += new DVector3(0, -1, 0);
                Position = pos;
            }


            // next down cube should be solid, and two upper cubes should be transparent
            if (!CubeProfile.CubesProfile[next].IsSolidToEntity && CubeProfile.CubesProfile[nextDown].IsSolidToEntity && !CubeProfile.CubesProfile[nextUp].IsSolidToEntity)
            {
                // move 
                Position = nextPosition;
            }
            else
            {
                _moveDirection = new Vector2();
            }

            

        }


    }
}
