using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using S33M3DXEngine.Effects.HLSLFramework;
using S33M3DXEngine.Main;
using S33M3DXEngine.Main.Interfaces;
using S33M3Resources.Structs;
using SharpDX.Direct3D11;

namespace S33M3CoreComponents.Particules.Interfaces
{
    public interface IEmitter : IDrawable
    {
        ParticuleEngine ParentParticuleEngine { get; set; }

        void Initialize(DeviceContext context, iCBuffer sharedFrameBuffer);

        //The emmiter has been stopped, and can be cleaned up
        bool isStopped { get; }

        /// <summary>
        /// Genere landscape collision check for the emmitted particles
        /// </summary>
        bool WithLandscapeCollision { get; set; }

        /// <summary>
        /// Will stop this particules emmiter and all its particules will be removed
        /// </summary>
        void Stop();

        double MaxRenderingDistance { get; set; }
    }
}
