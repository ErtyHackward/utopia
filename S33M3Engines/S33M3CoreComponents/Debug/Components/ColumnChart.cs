using System;
using System.Collections.Generic;
using System.Linq;
using S33M3DXEngine;
using S33M3DXEngine.Buffers;
using S33M3DXEngine.Main;
using S33M3DXEngine.RenderStates;
using S33M3Resources.Effects.Debug;
using S33M3Resources.Struct.Vertex;
using S33M3Resources.VertexFormats;
using SharpDX;
using SharpDX.Direct3D;
using SharpDX.Direct3D11;

namespace S33M3CoreComponents.Debug.Components
{
    public class ColumnChart : DrawableGameComponent
    {
        #region Private variable
        private Rectangle _screenPosition;
        private D3DEngine _engine;
        private float[] _values;
        private List<VertexColumnChart> _lines = new List<VertexColumnChart>();
        private int cursorLocation;
        private HLSLColumnChart _effect;
        private VertexBuffer<VertexPosition2> _vBuffer;
        private InstancedVertexBuffer<VertexPosition2, VertexColumnChart> _vBufferInstanced;

        private int _rasterStateId, _blendStateId, _depthStateId;
        #endregion

        #region Public variable
        public Rectangle ScreenPosition
        {
            get { return _screenPosition; }
            set
            {
                //If width change
                if (value.Width != _screenPosition.Width)
                {
                    _values = new float[value.Width];
                    cursorLocation = 0;
                }
                _screenPosition = value;
            }
        }
        #endregion

        public ColumnChart(D3DEngine engine, Rectangle screenPosition)
        {
            _engine = engine;
            this.ScreenPosition = screenPosition;
        }

        #region Private methods
        private void Render(DeviceContext context, VertexColumnChart[] dataPerInstance)
        {
            if (dataPerInstance.Length == 0) return;

            RenderStatesRepo.ApplyStates(_rasterStateId, _blendStateId, _depthStateId);

            _effect.Begin(context);
            _effect.CBPerDraw.Values.ViewportSize = new Vector2(_engine.ViewPort.Width, _engine.ViewPort.Height);
            _effect.CBPerDraw.IsDirty = true;
            _effect.Apply(context);

            _vBufferInstanced.SetInstancedData(context, dataPerInstance);
            _vBufferInstanced.SetToDevice(context, 0);

            context.DrawInstanced(2, dataPerInstance.Length, 0, 0);
        }
        #endregion

        #region Public methods

        public override void Initialize()
        {
            
            _rasterStateId = RenderStatesRepo.AddRasterStates(new RasterizerStateDescription
            {
                IsAntialiasedLineEnabled = false,
                CullMode = CullMode.None,
                DepthBias = 0,
                DepthBiasClamp = 1f,
                IsDepthClipEnabled = true,
                FillMode = FillMode.Solid,
                IsFrontCounterClockwise = false,
                IsMultisampleEnabled = true,
                IsScissorEnabled = false,
                SlopeScaledDepthBias = 0,
            });

            var blendDescr = new BlendStateDescription { IndependentBlendEnable = false, AlphaToCoverageEnable = false };
            for (var i = 0; i < 8; i++)
            {
                blendDescr.RenderTarget[i].IsBlendEnabled = false;
                blendDescr.RenderTarget[i].BlendOperation = BlendOperation.Add;
                blendDescr.RenderTarget[i].AlphaBlendOperation = BlendOperation.Add;
                blendDescr.RenderTarget[i].DestinationBlend = BlendOption.InverseSourceAlpha;
                blendDescr.RenderTarget[i].DestinationAlphaBlend = BlendOption.One;
                blendDescr.RenderTarget[i].SourceBlend = BlendOption.One;
                blendDescr.RenderTarget[i].SourceAlphaBlend = BlendOption.One;
                blendDescr.RenderTarget[i].RenderTargetWriteMask = ColorWriteMaskFlags.All;
            }
            _blendStateId = RenderStatesRepo.AddBlendStates(blendDescr);

            _depthStateId = RenderStatesRepo.AddDepthStencilStates(new DepthStencilStateDescription
            {
                IsDepthEnabled = false,
                DepthComparison = Comparison.Less,
                DepthWriteMask = DepthWriteMask.All,
                IsStencilEnabled = false,
                BackFace = new DepthStencilOperationDescription { Comparison = Comparison.Always, DepthFailOperation = StencilOperation.Keep, FailOperation = StencilOperation.Keep, PassOperation = StencilOperation.Keep },
                FrontFace = new DepthStencilOperationDescription { Comparison = Comparison.Always, DepthFailOperation = StencilOperation.Keep, FailOperation = StencilOperation.Keep, PassOperation = StencilOperation.Keep }
            });
        }

        public override void LoadContent(SharpDX.Direct3D11.DeviceContext context)
        {
            _effect = ToDispose(new HLSLColumnChart(_engine.Device));

            // Create the vertex buffer
            VertexPosition2[] vertices = { 
                                          new VertexPosition2(new Vector2(0.0f, 0.0f)),
                                          new VertexPosition2(new Vector2(0.0f, 1.0f))
                                         };

            _vBuffer = ToDispose(new VertexBuffer<VertexPosition2>(_engine.Device, vertices.Length, VertexPosition2.VertexDeclaration, PrimitiveTopology.LineList, "ColumnChart_vBuffer", ResourceUsage.Immutable));
            _vBuffer.SetData(_engine.ImmediateContext, vertices);

            _vBufferInstanced = ToDispose(new InstancedVertexBuffer<VertexPosition2, VertexColumnChart>(_engine.Device, VertexColumnChart.VertexDeclaration, PrimitiveTopology.LineList));
            _vBufferInstanced.SetFixedData(vertices);

            base.LoadContent(context);
        }

        public override void Update(GameTime timeSpent)
        {
            //Refresh the Graphique by creation of the VertexColumnChart collection
            _lines.Clear();

            Vector4 tranform;

            //transform.xy = Screenspace translation
            //transform.z = Y scaling
            //transform.w = 0 for vertical bar, 1 for horiz. bar

            //Get the average value of received values
            float avg = _values.Average(x => x);
            float limit = avg + (avg * 0.05f);

            for (int i = 0; i < _values.Length; i++)
            {
                tranform = new Vector4(_screenPosition.Left + i, _screenPosition.Top, Math.Min( (_values[i] / (avg * 2f)) * _screenPosition.Height, _screenPosition.Height), 0.0f);
                if (_values[i] > limit)
                {
                    _lines.Add(new VertexColumnChart(tranform, Colors.Red));
                }
                else
                {
                    _lines.Add(new VertexColumnChart(tranform, Colors.Green));
                }
            }

            //ADD axis
            tranform = new Vector4(_screenPosition.Left, _screenPosition.Top, _screenPosition.Height, 0.0f);
            _lines.Add(new VertexColumnChart(tranform, Colors.Black));
            tranform = new Vector4(_screenPosition.Left, _screenPosition.Top, _screenPosition.Width, 1.0f);
            _lines.Add(new VertexColumnChart(tranform, Colors.Black));

            //Add Cursor
            tranform = new Vector4(_screenPosition.Left + cursorLocation, _screenPosition.Top, _screenPosition.Height, 0.0f);
            _lines.Add(new VertexColumnChart(tranform, Colors.DarkBlue));
        }

        public override void Draw(DeviceContext context, int index)
        {
            Render(context, _lines.ToArray());
        }

        public void AddValue(float value)
        {
            if (cursorLocation >= _screenPosition.Width) cursorLocation = 0;
            _values[cursorLocation] = value;
            cursorLocation += 1;
        }
        #endregion
    }
}
