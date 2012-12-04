using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using S33M3DXEngine.Main.Interfaces;

namespace S33M3CoreComponents.Particules.Interfaces
{
    public interface IEmitter : IDrawable
    {
        List<Particule> Particules { get; }

        /// <summary>
        /// Genere landscape collision check for the emmitted particles
        /// </summary>
        bool WithLandscapeCollision { get; set; }

        /// <summary>
        /// Will stop this particules emmiter and all its particules will be removed
        /// </summary>
        void Stop();
    }
}
