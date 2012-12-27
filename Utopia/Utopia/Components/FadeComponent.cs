using S33M3DXEngine;
using S33M3DXEngine.Buffers;
using S33M3DXEngine.Main;
using S33M3DXEngine.RenderStates;
using S33M3Resources.Effects.Basics;
using S33M3Resources.Structs.Vertex;
using SharpDX;
using SharpDX.Direct3D;
using SharpDX.Direct3D11;
using Utopia.Shared.GameDXStates;

namespace Utopia.Components
{
    /// <summary>
    /// Fills the screen with custom color with alpha channel
    /// </summary>
    public class FadeComponent : DrawableGameComponent
    {
        private readonly D3DEngine _engine;
        private IndexBuffer<short> _iBuffer;
        private VertexBuffer<VertexPosition2> _vBuffer;
        HLSLScreenSpaceRect _effect;

        public Color4 Color { get; set; }

        public FadeComponent(D3DEngine engine)
        {
            _engine = engine;
            DrawOrders.UpdateIndex(0, 10000);
        }

        public override void Initialize()
        {
            _effect = ToDispose(new HLSLScreenSpaceRect(_engine.Device));

            // Create the vertex buffer => Thread safe, can be done here
            _vBuffer = ToDispose(new VertexBuffer<VertexPosition2>(_engine.Device, 4, VertexPosition2.VertexDeclaration, PrimitiveTopology.TriangleList, "Fade_vBuffer", ResourceUsage.Immutable));

            // Create the index buffer => Thread safe, can be done here
            _iBuffer = ToDispose(new IndexBuffer<short>(_engine.Device, 6, SharpDX.DXGI.Format.R16_UInt, "Fade_iBuffer"));
        }

        public override void LoadContent(DeviceContext context)
        {
            //Load data into the VB  => NOT Thread safe, MUST be done in the loadcontent
            VertexPosition2[] vertices = { 
                                          new VertexPosition2(new Vector2(-1.00f, -1.00f)),
                                          new VertexPosition2(new Vector2(1.00f, -1.00f)),
                                          new VertexPosition2(new Vector2(1.00f, 1.00f)),
                                          new VertexPosition2(new Vector2(-1.00f, 1.00f))
                                      };
            _vBuffer.SetData(context, vertices);

            //Load data into the IB => NOT Thread safe, MUST be done in the loadcontent
            short[] indices = { 3, 0, 2, 0, 1, 2 };
            _iBuffer.SetData(context, indices);

            base.LoadContent(context);
        }

        public override void Draw(DeviceContext context, int index)
        {
            RenderStatesRepo.ApplyStates(context, DXStates.Rasters.CullNone, DXStates.Blenders.Enabled);

            _vBuffer.SetToDevice(context, 0);
            _iBuffer.SetToDevice(context, 0);

            _effect.Begin(context);

            _effect.CBPerDraw.Values.Color = Color;
            _effect.CBPerDraw.IsDirty = true;

            _effect.Apply(context);

            context.DrawIndexed(6, 0, 0);
        }
    }
}
