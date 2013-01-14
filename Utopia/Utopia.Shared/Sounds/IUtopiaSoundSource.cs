using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Utopia.Shared.Sounds
{
    public interface IUtopiaSoundSource
    {
        TimeOfDaySound TimeOfDay { get; set; }
        string FilePath { get; set; }
        string Alias { get; set; }
        float Volume { get; set; }
        float Power { get; set; }
    }
}
