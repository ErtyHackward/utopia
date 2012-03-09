using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SharpDX;
using S33M3_CoreComponents.Cameras.Interfaces;

namespace Utopia.Resources.ModelComp
{
    public interface IDrawable
    {
        void Draw(ref ICamera camera, ref Matrix world);
        void Draw();
    }
}
