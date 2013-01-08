using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Utopia.Shared.Entities.Interfaces;
using Utopia.Shared.Settings;

namespace Utopia.Shared.Configuration
{
    public interface IProcessorParams
    {
        WorldConfiguration Config { get; set; }
        IEnumerable<BlockProfile> InjectDefaultCubeProfiles();
        IEnumerable<IEntity> InjectDefaultEntities();
        void CreateDefaultValues();
    }
}
