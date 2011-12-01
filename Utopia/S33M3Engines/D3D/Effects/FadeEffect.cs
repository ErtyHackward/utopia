using System;
using System.Runtime.InteropServices;
using S33M3Engines.Buffers;
using S33M3Engines.StatesManager;
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

        private int _rasterStateId, _blendStateId;

        public FadeEffect(D3DEngine engine)
        {
            _engine = engine;
            _effect = new HLSLFade(_engine);
            
            // Create the vertex buffer
            VertexPosition2[] vertices = { 
                                          new VertexPosition2(new Vector2(-0.10f, -0.10f)),
                                          new VertexPosition2(new Vector2(0.10f, -0.10f)),
                                          new VertexPosition2(new Vector2(0.10f, 0.10f)),
                                          new VertexPosition2(new Vector2(-0.10f, 0.10f))
                                      };

            _vBuffer = new VertexBuffer<VertexPosition2>(_engine, vertices.Length, VertexPosition2.VertexDeclaration, PrimitiveTopology.TriangleList, "Fade_vBuffer", ResourceUsage.Immutable);
            _vBuffer.SetData(vertices);

            // Create the index buffer
            short[] indices = { 3, 0, 2, 0, 1, 2 };
            _iBuffer = new IndexBuffer<short>(_engine, indices.Length, SharpDX.DXGI.Format.R16_UInt, "Fade_iBuffer");
            _iBuffer.SetData(indices);

            _rasterStateId = StatesRepository.AddRasterStates(new RasterizerStateDescription
            {
                CullMode = CullMode.None,
                FillMode = FillMode.Solid 
            });

            var blendDescr = new BlendStateDescription { IndependentBlendEnable = false, AlphaToCoverageEnable = false };
            for (var i = 0; i < 8; i++)
            {
                blendDescr.RenderTarget[i].IsBlendEnabled = true;
                blendDescr.RenderTarget[i].SourceBlend = BlendOption.SourceAlpha;
                blendDescr.RenderTarget[i].DestinationBlend = BlendOption.InverseSourceAlpha;
                blendDescr.RenderTarget[i].BlendOperation = BlendOperation.Add;
                blendDescr.RenderTarget[i].SourceAlphaBlend = BlendOption.One;
                blendDescr.RenderTarget[i].DestinationAlphaBlend = BlendOption.One;
                blendDescr.RenderTarget[i].AlphaBlendOperation = BlendOperation.Add;
                blendDescr.RenderTarget[i].RenderTargetWriteMask = ColorWriteMaskFlags.All;
            }
            _blendStateId = StatesRepository.AddBlendStates(blendDescr);

        }

        public void Draw(Color4 color)
        {
            StatesRepository.ApplyStates(_rasterStateId, _blendStateId);

            _vBuffer.SetToDevice(0);
            _iBuffer.SetToDevice(0);

            _effect.Begin();
            
            _effect.CBPerDraw.Values.Color = color;
            _effect.CBPerDraw.IsDirty = true;
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
        [StructLayout(LayoutKind.Explicit, Size = 16)]
        public struct CBFadePerDraw
        {
            [FieldOffset(0)]
            public Color4 Color;
        }

        public CBuffer<CBFadePerDraw> CBPerDraw;

        public HLSLFade(D3DEngine engine) : base(engine, @"D3D\Effects\Basics\Fade.hlsl", VertexPosition2.VertexDeclaration)
        {
            CBPerDraw = new CBuffer<CBFadePerDraw>(engine, "PerDraw");
            CBuffers.Add(CBPerDraw);

            LoadShaders(new EntryPoints
                            {
                                VertexShader_EntryPoint = "FadeVS",
                                PixelShader_EntryPoint = "FadePS"
                            });
        }
    }
}
