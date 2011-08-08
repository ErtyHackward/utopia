using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using S33M3Engines.D3D;
using S33M3Engines.Struct.Vertex;

namespace S33M3Engines.D3D.Effects.Basics
{
    public static class DebugEffect
    {
        private static HLSLVertexPositionColor _debugEffect;

        public static HLSLVertexPositionColor DebugEffectVPC
        {
            get { return _debugEffect; }
        }

        public static void Init(Game game)
        {
            _debugEffect = new HLSLVertexPositionColor(game, @"D3D/Effects/Basics/VertexPositionColor.hlsl", VertexPositionColor.VertexDeclaration);
        }
        public static void Dispose()
        {
            if (_debugEffect != null) _debugEffect.Dispose();
        }
    }
}
