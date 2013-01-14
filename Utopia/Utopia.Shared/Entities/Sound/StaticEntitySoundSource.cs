using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using ProtoBuf;
using Utopia.Shared.Settings;
using Utopia.Shared.Tools;

namespace Utopia.Shared.Entities.Sound
{
    [ProtoContract]
    public class StaticEntitySoundSource : SoundSource
    {
        [Description("Sound will be play in loop"), Category("Sound")]
        [ProtoMember(1)]
        public bool isLooping { get; set; }

        [Description("Min sound deffered starting (ms)"), Category("Sound")]
        [ProtoMember(2)]
        public uint minDeferredStart { get; set; }

        [Description("Max sound deffered starting (ms)"), Category("Sound")]
        [ProtoMember(3)]
        public uint maxDeferredStart { get; set; }

        public StaticEntitySoundSource()
            :base()
        {
            //Default values
            isLooping = false;
            minDeferredStart = 0;
            maxDeferredStart = 0;

            base.isStreamed = false;
            base.Category = S33M3CoreComponents.Sound.SourceCategory.FX;
        }
    }
}
