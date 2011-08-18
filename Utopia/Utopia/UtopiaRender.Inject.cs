using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ninject;
using Utopia.Worlds.GameClocks;

namespace Utopia
{
    public partial class UtopiaRender
    {
        private void InjectionContainersCreation()
        {
            using (IKernel kernel = new StandardKernel())
            {
                kernel.Bind<IClock>()
                      .To<WorldClock>()
                      .InSingletonScope()
                      .WithConstructorArgument("Game", this)
                      .WithConstructorArgument("clockSpeed", 480)
                      .WithConstructorArgument("startTime", (float)Math.PI * 1f);
            }
        }
    }
}
