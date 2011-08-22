using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Utopia.Shared.Interfaces;
using Ninject;

namespace Utopia.Shared.World.FlatWorld
{
    public class DummyWorldConfigurationConfig : IWorldProcessorConfig
    {
        private IWorldProcessor[] _worldProcessors;

        public IWorldProcessor[] WorldProcessors
        {
            get { return _worldProcessors; }
        }

        public DummyWorldConfigurationConfig([Named("FlatWorldProcessor")] IWorldProcessor FlatWorldProcessor)
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
