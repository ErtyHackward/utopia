using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace S33M3CoreComponents.Sound
{
    public interface ISoundDataSourceBase
    {
        SourceCategory Category { get; set; }
        string FilePath { get; set; }
        string Alias { get; set; }
        float Volume { get; set; }
        float Power { get; set; }
        bool isStreamed { get; set; }
        int Priority { get; set; }
    }
}
