using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using S33M3Engines.D3D;
using SharpDX;

namespace Utopia.Worlds.SkyDomes
{
    public interface ISkyDome : IDrawableComponent
    {
        Vector3 LightDirection { get; }
    }
}
