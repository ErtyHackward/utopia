using Ninject;
using S33M3DXEngine;
using S33M3DXEngine.Buffers;
using S33M3DXEngine.Main;
using S33M3DXEngine.RenderStates;
using S33M3Resources.Structs.Vertex;
using SharpDX;
using SharpDX.Direct3D;
using SharpDX.Direct3D11;
using SharpDX.DXGI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Utopia.Components;
using Utopia.Resources.Effects.PostEffects;
using Utopia.Shared.GameDXStates;
using Utopia.Shared.Settings;

namespace Utopia.PostEffects
{
    public class PostEffectComponent : DrawableGameComponent
    {
        #region Private properties
        private readonly D3DEngine _engine;
        private Texture2D _renderTexture;
        private ShaderResourceView _renderTextureView;
        private RenderTargetView _renderTargetView;
        private Color4 __renderTargetViewDefaultColor = new Color4(0.0f, 0.0f, 0.0f, 0.0f);
        private bool _postEffectStartDrawDone = false;

        private int _postEffectStartingDrawId = 0;
        private int _postEffectEndingDrawId;

        //Shadder related stuffs
        private IPostEffect _activatedEffect;
        private Dictionary<string, IPostEffect> _registeredEffects;
        private IndexBuffer<ushort> _iBuffer;
        private VertexBuffer<VertexPosition2Texture> _vBuffer;

        private StaggingBackBuffer _backBufferClouds;
        #endregion

        #region Public Properties
        public IPostEffect ActivatedEffect
        {
            get { return _activatedEffect; }
            set { _activatedEffect = value; }
        }

        public ShaderResourceView RenderTextureView
        {
            get { return _renderTextureView; }
            set { _renderTextureView = value; }
        }

        public Dictionary<string, IPostEffect> RegisteredEffects
        {
            get { return _registeredEffects; }
            set { _registeredEffects = value; }
        }
        #endregion

        public PostEffectComponent(D3DEngine engine, [Named("SkyBuffer")] StaggingBackBuffer backBufferClouds)
        {
            _engine = engine;
            _backBufferClouds = backBufferClouds;
            _engine.ScreenSize_Updated += engine_ScreenSize_Updated;

            _registeredEffects = new Dictionary<string, IPostEffect>();

            this.DrawOrders.UpdateIndex(0, 11, "FROM"); //Call at the beginning of what must be draw into this texture.
            _postEffectEndingDrawId = this.DrawOrders.AddIndex(9999, "TO"); //Call when the drawing is finished.
        }

        public override void Initialize()
        {
            CreateRenderTargets(_engine.BackBufferTex.Description);

            foreach (var kvp in RegisteredEffects)
            {
                kvp.Value.Initialize(_engine.Device);
                ToDispose(kvp.Value);
            }

            // Create the vertex buffer => Thread safe, can be done here
            _vBuffer = ToDispose(new VertexBuffer<VertexPosition2Texture>(_engine.Device, 4, PrimitiveTopology.TriangleList, "PostEffect_vBuffer", ResourceUsage.Immutable));

            // Create the index buffer => Thread safe, can be done here
            _iBuffer = ToDispose(new IndexBuffer<ushort>(_engine.Device, 6, "PostEffect_iBuffer"));
        }

        public override void LoadContent(DeviceContext context)
        {
            //Load data into the VB  => NOT Thread safe, MUST be done in the loadcontent
            VertexPosition2Texture[] vertices = { 
                                          new VertexPosition2Texture(new Vector2(-1.00f, -1.00f), new Vector2(0.0f, 1.0f)),
                                          new VertexPosition2Texture(new Vector2(1.00f, -1.00f), new Vector2(1.0f, 1.0f)),
                                          new VertexPosition2Texture(new Vector2(1.00f, 1.00f), new Vector2(1.0f, 0.0f)),
                                          new VertexPosition2Texture(new Vector2(-1.00f, 1.00f), new Vector2(0.0f, 0.0f))
                                      };
            _vBuffer.SetData(context, vertices);

            //Load data into the IB => NOT Thread safe, MUST be done in the loadcontent
            ushort[] indices = { 3, 2, 0, 0, 2, 1 };
            _iBuffer.SetData(context, indices);

            base.LoadContent(context);
        }

        public override void BeforeDispose()
        {
            if (_renderTexture != null) _renderTexture.Dispose();
            if (_renderTextureView != null) _renderTextureView.Dispose();
            _engine.ScreenSize_Updated -= engine_ScreenSize_Updated;
        }

        #region Public methods
        public void ActivateEffect(string Name)
        {
            if (_activatedEffect != null && _activatedEffect.Name == Name) return;
            if (_activatedEffect != null) _activatedEffect.Deactivate();
            if (!_registeredEffects.TryGetValue(Name, out _activatedEffect))
            {
                _activatedEffect = null;
            }
            else
            {
                _activatedEffect.Activate(_renderTextureView, this);
            }
        }

        public void DeactivateEffect()
        {
            if (_activatedEffect != null) _activatedEffect.Deactivate();
            //_activatedEffect = null;
        }

        public override void FTSUpdate(GameTime timeSpent)
        {
            if (_activatedEffect == null) return;
            _activatedEffect.FTSUpdate(timeSpent);
        }

        public override void VTSUpdate(double interpolationHd, float interpolationLd, float elapsedTime)
        {
            if (_activatedEffect == null) return;
            _activatedEffect.VTSUpdate(interpolationHd, interpolationLd, elapsedTime);
        }

        public override void Draw(SharpDX.Direct3D11.DeviceContext context, int index)
        {
            if (_activatedEffect == null)
            {
                if (_backBufferClouds.OffSreenRenderTarget != null) _backBufferClouds.OffSreenRenderTarget = null;
                return;
            }

            if (index == _postEffectStartingDrawId)
            {
                //Change RenderTarget to this offline texture
                //Clear renderTarger
                _engine.ImmediateContext.ClearRenderTargetView(_renderTargetView, __renderTargetViewDefaultColor);
                _engine.ImmediateContext.OutputMerger.SetTargets(_engine.DepthStencilTarget, _renderTargetView);
                _backBufferClouds.OffSreenRenderTarget = _renderTexture;
                _postEffectStartDrawDone = true;
            }
            else if (index == _postEffectEndingDrawId && _postEffectStartDrawDone)
            {
                //We have finished to draw into the offline texture, link back the screen backbuffer
                _engine.SetRenderTargets(context);
                //Start the Post effect code here
                RenderStatesRepo.ApplyStates(context, DXStates.Rasters.Default, DXStates.Blenders.Disabled, DXStates.DepthStencils.DepthDisabled);
                _vBuffer.SetToDevice(context, 0);
                _iBuffer.SetToDevice(context, 0);

                _activatedEffect.Render(context);

                context.DrawIndexed(6, 0, 0);
                _postEffectStartDrawDone = false;
            }
        }
        #endregion

        #region Private Methods
        private void engine_ScreenSize_Updated(SharpDX.ViewportF viewport, Texture2DDescription newBackBuffer)
        {
            CreateRenderTargets(newBackBuffer);
            if (_activatedEffect != null) _activatedEffect.RefreshBackBuffer(_renderTextureView);
        }

        private void CreateRenderTargets(Texture2DDescription backBufferDesciption)
        {
            if (_renderTextureView != null)
            {
                _renderTargetView.Dispose();
                _renderTextureView.Dispose();
                _renderTexture.Dispose();
            }

            Texture2DDescription StaggingBackBufferDescr = new Texture2DDescription()
            {
                Width = _engine.BackBufferTex.Description.Width,
                Height = _engine.BackBufferTex.Description.Height,
                MipLevels = 1,
                ArraySize = 1,
                Format = Format.R8G8B8A8_UNorm,
                SampleDescription = _engine.CurrentMSAASampling,
                Usage = ResourceUsage.Default,
                BindFlags = BindFlags.ShaderResource | BindFlags.RenderTarget,
                CpuAccessFlags = CpuAccessFlags.None,
                OptionFlags = ResourceOptionFlags.None
            };

            _renderTexture = new Texture2D(_engine.Device, StaggingBackBufferDescr);

            _renderTextureView = new ShaderResourceView(_engine.Device, _renderTexture);

            //Create the Depth Stencil View + View
            RenderTargetViewDescription renderTargetViewDescription = new RenderTargetViewDescription()
            {
                Format = _renderTexture.Description.Format,
                Dimension = _engine.CurrentMSAASampling.Count <= 1 ? RenderTargetViewDimension.Texture2D : RenderTargetViewDimension.Texture2DMultisampled
            };
            //Create RenderTargetView 
            _renderTargetView = new RenderTargetView(_engine.Device, _renderTexture, renderTargetViewDescription);
        }
        #endregion
    }
}
