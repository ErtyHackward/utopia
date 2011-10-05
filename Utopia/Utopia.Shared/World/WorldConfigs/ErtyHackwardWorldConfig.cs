using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ninject;
using Utopia.Shared.Interfaces;

namespace Utopia.Shared.World.WorldConfigs
{
    /// <summary>
    /// Wrapper arround the needed processor to build a so called "FlatWorld world generator"
    /// </summary>
    public class ErtyHackwardWorldConfig : IWorldProcessorConfig
    {
        private IWorldProcessor[] _worldProcessors;

        public IWorldProcessor[] WorldProcessors
        {
            get { return _worldProcessors; }
        }

        public ErtyHackwardWorldConfig([Named("ErtyHackwardPlanWorldProcessor")] IWorldProcessor ertyHackwardWorldProcessor)
        {
            _worldProcessors = new IWorldProcessor[1];
            _worldProcessors[0] = ertyHackwardWorldProcessor;
        }

        public void Dispose()
        {
            foreach (IWorldProcessor processor in _worldProcessors) processor.Dispose();
        }
    }
}
