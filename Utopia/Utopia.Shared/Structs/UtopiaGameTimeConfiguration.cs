using System.Collections.Generic;
using System.ComponentModel;
using ProtoBuf;
using Utopia.Shared.Services;

namespace Utopia.Shared.Structs
{
    /// <summary>
    /// Contains game time parameters
    /// </summary>
    [ProtoContract]
    public class UtopiaGameTimeConfiguration
    {
        [Description("All possible seasons")]
        [ProtoMember(1, OverwriteList = true)]
        public List<Season> Seasons { get; set; }

        [Description("How many days in each season")]
        [ProtoMember(2)]
        public int DaysPerSeason { get; set; }

        [Description("How many real seconds in each game day")]
        [ProtoMember(3)]
        public int DayLength { get; set; }

        public int DaysPerYear
        {
            get { return Seasons == null ? 0 : Seasons.Count * DaysPerSeason; }
        }
    }
}