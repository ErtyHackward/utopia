﻿using System.Collections.Generic;
using ProtoBuf;
using S33M3Resources.Structs;

namespace Utopia.Shared.Entities
{
    /// <summary>
    /// Describes a faction and contain general stuff for it
    /// like a list of selected blocks to remove, attributes of the faction
    /// </summary>
    [ProtoContract]
    public class Faction
    {
        [ProtoMember(1)]
        public uint FactionId { get; set; }

        [ProtoMember(2)]
        public string Name { get; set; }

        [ProtoMember(3)]
        public HashSet<Vector3I> BlocksToRemove { get; set; }

        [ProtoMember(4)]
        public HashSet<uint> MembersIds { get; set; }

        public Faction()
        {
            BlocksToRemove = new HashSet<Vector3I>();
            MembersIds = new HashSet<uint>();
        }
    }
}
