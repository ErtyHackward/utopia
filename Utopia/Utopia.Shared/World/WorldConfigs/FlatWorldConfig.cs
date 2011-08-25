using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Utopia.Shared.Interfaces;
using Ninject;

namespace Utopia.Shared.World.WorldConfigs
{
    /// <summary>
    /// Wrapper arround the needed processor to build a so called "FlatWorld world generator"
    /// </summary>
    public class FlatWorldConfig : IWorldProcessorConfig
    {
        private IWorldProcessor[] _worldProcessors;

        public IWorldProcessor[] WorldProcessors
        {
            get { return _worldProcessors; }
        }

        public FlatWorldConfig([Named("FlatWorldProcessor")] IWorldProcessor FlatWorldProcessor)
        {
            _worldProcessors = new IWorldProcessor[1];
            _worldProcessors[0] = FlatWorldProcessor;
        }

        public void Dispose()
        {
            foreach (IWorldProcessor processor in _worldProcessors) processor.Dispose();
        }
    }
}
