using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Utopia.Shared.Interfaces;
using Ninject;

namespace Utopia.Shared.World.WorldConfigs
{
    public class s33m3WorldConfig : IWorldProcessorConfig
    {
        private IWorldProcessor[] _worldProcessors;

        public IWorldProcessor[] WorldProcessors
        {
            get { return _worldProcessors; }
        }

        public s33m3WorldConfig([Named("s33m3WorldProcessor")] IWorldProcessor MainWorldProcessor,
                                [Named("LandscapeLayersProcessor")] IWorldProcessor LandscapeLayersProcessor)
        {
            _worldProcessors = new IWorldProcessor[2];
            _worldProcessors[0] = MainWorldProcessor;
            _worldProcessors[1] = LandscapeLayersProcessor;
        }

        public void Dispose()
        {
            foreach (IWorldProcessor processor in _worldProcessors) processor.Dispose();
        }
    }
}
