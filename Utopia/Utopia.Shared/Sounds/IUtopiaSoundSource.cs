using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Utopia.Shared.Sounds
{
    public interface IUtopiaSoundSource
    {
        TimeOfDaySound TimeOfDay { get; set; }
        string SoundFilePath { get; set; }
        string SoundAlias { get; set; }
        float DefaultVolume { get; set; }
        float Power { get; set; }
    }
}
