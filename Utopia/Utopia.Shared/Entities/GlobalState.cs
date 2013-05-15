using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using ProtoBuf;

namespace Utopia.Shared.Entities
{
    /// <summary>
    /// Root entity for the global stuff
    /// </summary>
    [ProtoContract]
    public class GlobalState
    {
        /// <summary>
        /// Contains all game factions
        /// </summary>
        [ProtoMember(1)]
        public FactionCollection Factions { get; set; }

        public GlobalState()
        {
            Factions = new FactionCollection();
        }
    }

    [ProtoContract]
    public class FactionCollection : IEnumerable<Faction>
    {
        private List<Faction> _factions = new List<Faction>();

        public Faction this[uint factionId]
        {
            get { return _factions.First(f => f.FactionId == factionId); }
        }

        public int Count
        {
            get { return _factions.Count; }
        }

        public Faction GetFaction(uint factionId)
        {
            return _factions.FirstOrDefault(f => f.FactionId == factionId);
        }

        public void Add(Faction faction)
        {
            if (faction == null)
                throw new ArgumentNullException("faction");

            if (_factions.Any(f => f.FactionId == faction.FactionId))
                throw new InvalidOperationException(string.Format("We already have a faction with the same id ({0})",faction.FactionId));

            _factions.Add(faction);
        }

        public IEnumerator<Faction> GetEnumerator()
        {
            return _factions.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}