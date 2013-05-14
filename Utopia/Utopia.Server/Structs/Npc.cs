using System;
using System.Collections.Generic;
using Utopia.Shared.Entities.Concrete;
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
        public static Vector3D Near = new Vector3D(0.02d);
        private List<MapArea> _mapAreas = new List<MapArea>();
        private int _seed;
        private Random _random;
        
        /// <summary>
        /// Gets current NPC state
        /// </summary>
        public NpcState State { get; private set; }
        
        public MoveAI Movement { get; private set; }

        IMoveAI INpc.Movement
        {
            get { return Movement; }
        }
        
        public FocusAI Focus { get; private set; }

        public Random Random { get { return _random; } }

        public int Seed
        {
            get { return _seed; }
            set
            {
                _seed = value;
                _random = new Random(_seed);
            }
        }
        
        public Npc(Server server, Dwarf z) : base(server, z)
        {
            Seed = 0;

            Movement = new MoveAI(this);
            Focus = new FocusAI(this);
        }

        public void Goto(Vector3I location)
        {
            
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

        /// <summary>
        /// Choose what to do next
        /// </summary>
        private void AISelect()
        {
            if (!Movement.IsMooving && Movement.Leader == null)
            {
                
            }
        }
    }
}
