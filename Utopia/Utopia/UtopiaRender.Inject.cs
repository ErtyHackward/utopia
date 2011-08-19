using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ninject;
using Utopia.Worlds.GameClocks;
using Utopia.Worlds.Weather;

namespace Utopia
{
    public partial class UtopiaRender
    {
        private void ContainersBindings(IKernel iocContainer)
        {
            iocContainer.Bind<IClock>().To<WorldClock>().InSingletonScope();
            iocContainer.Bind<IWeather>().To<Weather>().InSingletonScope();

        }
    }
}
