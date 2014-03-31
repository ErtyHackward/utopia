using ProtoBuf;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing.Design;
using System.Linq;
using System.Text;
using Utopia.Shared.Configuration;
using Utopia.Shared.Services;

namespace Utopia.Shared.Chunks
{
    /// <summary>
    /// Class that will be used to stored information about an entity that could spawn on a chunk
    /// </summary>
    [ProtoContract]
    public partial class ChunkSpawnableEntity
    {
        [Browsable(false)]
        [ProtoMember(1)]
        public ushort BluePrintId { get; set; }
        [ProtoMember(2)]
        [Description("Chance of spawning for the entity [0;1]")]
        public double SpawningChance { get; set; }
        [ProtoMember(3)]
        [Description("Maximum amount of entity per chunk")]
        public ushort MaxEntityAmount { get; set; }
        [ProtoMember(4)]
        [Description("Does it require the chunk to be wild to spawn ?")]
        public bool isWildChunkNeeded { get; set; }
        [ProtoMember(5)]
        [Description("The spawning place")]
        public ChunkSpawningPlace SpawningPlace { get; set; }
        [ProtoMember(6)]
        [Description("The spawning time")]
        public ChunkSpawningDayTime SpawningDayTime { get; set; }
        [ProtoMember(7)]
        [Description("The spawning season(s), if empty will spawn inside all existing seasons")]
        //public List<string> SpawningSeasons { get; set; }

        private List<string> _spawningSeasons;
        [Editor(typeof(SpawningSeasonsEditor), typeof(UITypeEditor))]
        public List<string> SpawningSeasons
        {
            get { return _spawningSeasons; }
            set { _spawningSeasons = value; }
        }

        [ProtoMember(8)]
        [Browsable(false)]
        public bool isChunkGenerationSpawning { get; set; }

        public override string ToString()
        {
            return EntityName;
        }
    }

    public enum ChunkSpawningPlace
    {
        Surface,
        InsideCave,
        InsideGround
    }

    public enum ChunkSpawningDayTime
    {
        Day,
        Night,
        DayAndNight
    }
}
