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
        [Editor(typeof(FlagsEditor), typeof(UITypeEditor))]
        public ChunkSpawningDayTime SpawningDayTime { get; set; }
        [ProtoMember(7)]
        [Description("The spawning season(s), if empty will spawn inside all existing seasons")]
        [Editor(typeof(SpawningSeasonsEditor), typeof(UITypeEditor))]
        public List<string> SpawningSeasons { get; set;}
        [ProtoMember(8)]
        [Browsable(false)]
        public bool isChunkGenerationSpawning { get; set; }

        public ChunkSpawnableEntity()
        {
            //Assign default values
            SpawningDayTime = ChunkSpawningDayTime.Day | ChunkSpawningDayTime.Night;
            MaxEntityAmount = 1;
            SpawningChance = 1.0;
            isWildChunkNeeded = true;
            SpawningPlace = ChunkSpawningPlace.Surface;
            SpawningSeasons = new List<string>();
        }

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

    [Flags]
    public enum ChunkSpawningDayTime
    {
        Never = 0,
        Day = 1,
        Night = 2
    }
}
