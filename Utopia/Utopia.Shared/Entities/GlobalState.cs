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
        public List<Faction> Factions { get; set; }

        public Faction GetFaction(uint factionId)
        {
            return Factions.FirstOrDefault(f => f.FactionId == factionId);
        }
    }
}