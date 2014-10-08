using ProtoBuf;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing.Design;
using Utopia.Shared.Configuration;
using Utopia.Shared.Entities;
using Utopia.Shared.Services;
using Utopia.Shared.Tools;

namespace Utopia.Shared.Chunks
{
    /// <summary>
    /// Class that will be used to stored information about an entity that could spawn on a chunk
    /// </summary>
    [ProtoContract]
    public partial class ChunkSpawnableEntity
    {
        private float _dynamicEntitySpawnRadius;

        [Editor(typeof(BlueprintTypeEditor<Entity>), typeof(UITypeEditor))]
        [TypeConverter(typeof(BlueprintTextHintConverter))]
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
        public bool IsWildChunkNeeded { get; set; }

        [ProtoMember(5)]
        [Description("The spawning place")]
        public ChunkSpawningPlace SpawningPlace { get; set; }

        [ProtoMember(6)]
        [Description("The spawning time")]
        [Editor(typeof(FlagsEditor), typeof(UITypeEditor))]
        public ChunkSpawningDayTime SpawningDayTime { get; set; }

        [ProtoMember(7)]
        [Description("The spawning season(s), if empty will spawn inside all existing seasons")]
        [Editor(typeof(Season.SeasonsEditor), typeof(UITypeEditor))]
        public List<string> SpawningSeasons { get; set;}

        [ProtoMember(8)]
        [Browsable(false)]
        public bool IsChunkGenerationSpawning { get; set; }

        /// <summary>
        /// Defines the radius for NPC's area where MaxEntityAmount is used
        /// </summary>
        [ProtoMember(9)]
        [Description("Defines the radius for NPC's area where MaxEntityAmount is used")]
        public float DynamicEntitySpawnRadius
        {
            get { return _dynamicEntitySpawnRadius; }
            set
            {
                if (value < 8) value = 8;
                _dynamicEntitySpawnRadius = value;
            }
        }

        public ChunkSpawnableEntity()
        {
            //Assign default values
            SpawningDayTime = ChunkSpawningDayTime.Day | ChunkSpawningDayTime.Night;
            MaxEntityAmount = 1;
            SpawningChance = 1.0;
            IsWildChunkNeeded = true;
            SpawningPlace = ChunkSpawningPlace.Surface;
            SpawningSeasons = new List<string>();
            DynamicEntitySpawnRadius = 32;
        }

        public override string ToString()
        {
            //return EntityName;
            return BluePrintId < 256 ? EditorConfigHelper.Config.BlockProfiles[BluePrintId].Name : EditorConfigHelper.Config.BluePrints[BluePrintId].Name;
        }
    }

    public enum ChunkSpawningPlace
    {
        Surface,
        FloorInsideCave,
        CeilingInsideCave,
        AirAboveSurface
    }

    [Flags]
    public enum ChunkSpawningDayTime
    {
        Day = 1,
        Night = 2
    }
}
