using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using S33M3Resources.Effects.Basics;
using S33M3DXEngine;
using S33M3Resources.Structs.Vertex;

namespace S33M3Resources.Effects.Debug
{
    public static class DebugEffect
    {
        private static HLSLVertexPositionColor _debugEffect;

        public static HLSLVertexPositionColor DebugEffectVPC
        {
            get { return _debugEffect; }
        }

        public static void Init(D3DEngine d3dEngine)
        {
            _debugEffect = new HLSLVertexPositionColor(d3dEngine.Device);
        }
        public static void Dispose()
        {
            if (_debugEffect != null) 
                _debugEffect.Dispose();
        }
    }
}
