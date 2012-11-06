using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Utopia.Shared.Configuration
{
    public interface IProcessorParams
    {
        void CreateDefaultConfiguration();
        WorldConfiguration Config { get; set; }
    }
}
