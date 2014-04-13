using System.ComponentModel;
using ProtoBuf;

namespace Utopia.Shared.Services
{
    [ProtoContract]
    public partial class Season
    {
        [Description("Name of the season")]
        [ProtoMember(1)]
        public string Name { get; set; }

        [Description("Median temperature offset for this season in range [-1;1]")]
        [ProtoMember(2)]
        public float Temperature { get; set; }

        [Description("Median moisture offset for this season in range [-1;1]")]
        [ProtoMember(3)]
        public float Moisture { get; set; }

        public override string ToString()
        {
            return Name;
        }
    }
}