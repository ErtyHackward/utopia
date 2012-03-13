using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SharpDX;
using S33M3DXEngine.Main.Interfaces;

namespace Utopia.Worlds.SkyDomes
{
    public interface ISkyDome : IDrawableComponent, IGameComponent
    {
        Vector3 LightDirection { get; }
        Vector3 SunColor { get; }
    }
}
