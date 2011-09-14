using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using S33M3Engines.Shared.Math;
using SharpDX;
using Utopia.Server.Structs;
using Utopia.Shared.Chunks.Entities;
using Utopia.Shared.Chunks.Entities.Management;
using Utopia.Shared.Structs;

namespace Utopia.Server.Entities
{
    public class Zombie : DynamicEntity
    {
        private readonly Server _server;
        private List<MapArea> _mapAreas = new List<MapArea>();
        private Vector2 _moveDirection;

        public Zombie(Server server)
        {
            _server = server;
        }

        public override void AddArea(MapArea area)
        {
            _mapAreas.Add(area);
        }

        public override void RemoveArea(MapArea area)
        {
            _mapAreas.Remove(area);
        }

        public override void Update(DateTime gameTime)
        {
            ServerChunk chunk;
            Location3<int> pos;
            _server.LandscapeManager.GetBlockAndChunk(Position+new DVector3(0,-1,0), out chunk, out pos );
            // todo: implement zombie logic
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
