﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using S33M3Engines.Shared.Math;
using SharpDX;
using Utopia.Server.Structs;
using Utopia.Shared.Chunks.Entities;
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

        public Vector2 MoveVector
        {
            get { return _moveDirection; }
            set { _moveDirection = value; }
        }
        
        public ServerZombie(Server server, string name)
        {
            _server = server;
            CharacterName = name;
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



            if(_moveDirection.X == 0 && _moveDirection.Y == 0)
            {
                var r = new Random(DateTime.Now.Millisecond);
                _moveDirection = new Vector2(r.Next(-100, 100) / 100f, r.Next(-100, 100) / 100f);
                _moveDirection.Normalize();
            }

            var nextPosition = Position + new DVector3(_moveDirection.X, 0, _moveDirection.Y) * _server.Clock.GameToReal(gameTime.ElapsedTime).TotalSeconds;

            // check if we can go to desired position
            var cursor = _server.LandscapeManager.GetCursor(nextPosition);

            var next = cursor.Value;
            var nextDown = cursor.PeekDown();
            var nextUp = cursor.PeekUp();
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

        public override EntityClassId ClassId
        {
            get { return EntityClassId.Zombie; }
        }

        public override string DisplayName
        {
            get { return "Zombie Bob"; }
        }
    }
}
