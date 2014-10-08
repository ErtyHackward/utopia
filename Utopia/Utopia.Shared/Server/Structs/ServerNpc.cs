using System;
using System.Collections.Generic;
using System.Linq;
using S33M3Resources.Structs;
using Utopia.Shared.Entities.Concrete;
using Utopia.Shared.Entities.Dynamic;
using Utopia.Shared.Entities.Interfaces;
using Utopia.Shared.Entities.Inventory;
using Utopia.Shared.Structs;

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
        private object _syncRoot = new object();

        private readonly CharacterEntity _character;
        
        public MoveAI Movement { get; private set; }

        public GeneralAI GeneralAI { get; private set; }

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
        /// Entity will try to avoid get close to these entities
        /// </summary>
        public List<IDynamicEntity> DangerousEntities { get; private set; }

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
            
            var npc = _character as Npc;

            if (npc != null)
            {
                GeneralAI = npc.AI;
                GeneralAI.Npc = this;
            }
            _random = new Random();
            
            Movement = new MoveAI(this);
            Focus = new FocusAI(this);

            DangerousEntities = new List<IDynamicEntity>();
        }

        void _character_InventoryUpdated(object sender, EventArgs e)
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
            lock (_syncRoot)
            {
                if (gameTime.ElapsedTime.TotalSeconds < 2)
                    return;
                if (gameTime.ElapsedTime.TotalSeconds > 100)
                    return;

                Movement.Update(gameTime);
                Focus.Update(gameTime);

                if (GeneralAI == null)
                    return;

                GeneralAI.AISelect();
                GeneralAI.DoAction();
            }
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

    }
}
