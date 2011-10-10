using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using S33M3Engines.Cameras;
using SharpDX;

namespace Utopia.Resources.ModelComp
{
    public interface IDrawable
    {
        void Draw(ref ICamera camera, ref Matrix world);
        void Draw();
    }
}
