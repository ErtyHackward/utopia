using System;
using System.Runtime.InteropServices;
using S33M3Engines.Buffers;
using S33M3Engines.Struct.Vertex;
using SharpDX;
using SharpDX.Direct3D;
using SharpDX.Direct3D11;

namespace S33M3Engines.D3D.Effects
{
    public class FadeEffect : IDisposable
    {
        private readonly D3DEngine _engine;
        private readonly HLSLFade _effect;

        private readonly IndexBuffer<short> _iBuffer;
        private readonly VertexBuffer<VertexPosition2> _vBuffer;

        public FadeEffect(D3DEngine engine)
        {
            _engine = engine;
            _effect = new HLSLFade(_engine);

            // Create the vertex buffer
            VertexPosition2[] vertices = { 
                                          new VertexPosition2(new Vector2(0.0f, 0.0f)),
                                          new VertexPosition2(new Vector2(1.0f, 0.0f)),
                                          new VertexPosition2(new Vector2(1.0f, 1.0f)),
                                          new VertexPosition2(new Vector2(0.0f, 1.0f))
                                      };

            _vBuffer = new VertexBuffer<VertexPosition2>(_engine, vertices.Length, VertexPosition2.VertexDeclaration, PrimitiveTopology.TriangleList, "SpriteRenderer_vBuffer", ResourceUsage.Immutable);
            _vBuffer.SetData(vertices);

            // Create the index buffer
            short[] indices = { 0, 1, 2, 3, 0, 2 };
            _iBuffer = new IndexBuffer<short>(_engine, indices.Length, SharpDX.DXGI.Format.R16_UInt, "SpriteRenderer_iBuffer");
            _iBuffer.SetData(indices);

        }

        public void Draw(Color4 color)
        {
            _vBuffer.SetToDevice(0);
            _iBuffer.SetToDevice(0);

            _effect.Begin();

            _effect.CBPerDraw.Values.Color = color;
            //_effect.CBPerDraw.Values.Transform = Matrix.Transpose(Matrix.Translation(destRect.Left, destRect.Top, 0));

            _effect.Apply();

            _engine.Context.DrawIndexed(6, 0, 0);
        }

        public void Dispose()
        {
            _effect.Dispose();
            _vBuffer.Dispose();
            _iBuffer.Dispose();
        }
    }

    internal class HLSLFade : HLSLShaderWrap
    {
        [StructLayout(LayoutKind.Explicit, Size = 80)]
        public struct CBFadePerDraw
        {
            [FieldOffset(0)]
            public Matrix Transform;
            [FieldOffset(64)]
            public Color4 Color;
        }

        public CBuffer<CBFadePerDraw> CBPerDraw;

        public HLSLFade(D3DEngine engine)
            : base(engine, @"D3D\Effects\Basics\Fade.hlsl", VertexPosition.VertexDeclaration)
        {
            LoadShaders(new EntryPoints
                            {
                                VertexShader_EntryPoint = "FadeVS",
                                PixelShader_EntryPoint = "FadePS"
                            });
        }
    }
}
