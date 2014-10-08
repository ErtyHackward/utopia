using S33M3DXEngine.Main;
using S33M3DXEngine.RenderStates;
using S33M3Resources.Structs;
using S33M3Resources.Structs.Vertex;
using SharpDX.Direct3D11;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Utopia.Resources.Effects.PostEffects;
using Utopia.Shared.GameDXStates;
using Utopia.Shared.Settings;
using System.Diagnostics;
using S33M3CoreComponents.Maths;
using SharpDX;

namespace Utopia.PostEffects
{
    public class PostEffectGhost : BaseComponent, IPostEffect
    {
        private HLSLGhost _effect;
        private FTSValue<float> _fadder = new FTSValue<float>();
        private Stopwatch _fadderTimer = new Stopwatch();
        private long _fadeTimer = 10000;
        private float _fadeType;
        private Vector4 _shadderParams;
        private bool _deactivationRequested;
        private PostEffectComponent _parentHolder;

        public string Name { get; set; }

        public void Initialize(Device device)
        {
            _effect = ToDispose(new HLSLGhost(device, ClientSettings.EffectPack + @"PostEffects\Ghost.hlsl", VertexPosition2Texture.VertexDeclaration));
            _effect.SamplerPostEffectBackBuffer.Value = RenderStatesRepo.GetSamplerState(DXStates.Samplers.UVClamp_MinMagMipPoint);
        }

        public void RefreshBackBuffer(ShaderResourceView backbuffer)
        {
            _effect.PostEffectBackBuffer.Value = backbuffer;
        }

        public void Activate(ShaderResourceView backbuffer, PostEffectComponent parent)
        {
            _effect.PostEffectBackBuffer.Value = backbuffer;
            _fadderTimer.Restart();
            _fadder.Initialize(0.0f);
            _fadeType = 1.0f;
            _deactivationRequested = false;
            _parentHolder = parent;
        }

        public void Deactivate()
        {
            _deactivationRequested = true;
            _fadeType = 2.0f;
            _fadder.Initialize(0.0f);
            _fadderTimer.Restart();
        }

        public void FTSUpdate(GameTime timeSpent)
        {
            if (_fadderTimer.IsRunning)
            {
                _fadder.BackUpValue();

                _fadder.Value = _fadderTimer.ElapsedMilliseconds / (float)_fadeTimer;

                if (_fadder.Value >= 1)
                {
                    _fadder.Value = 1;
                    _fadder.ValueInterp = 1;
                    _fadderTimer.Stop();
                }
            }
        }

        public void VTSUpdate(double interpolationHd, float interpolationLd, float elapsedTime)
        {
            if (_fadderTimer.IsRunning)
            {
                _fadder.ValueInterp = MathHelper.Lerp(_fadder.ValuePrev, _fadder.Value, interpolationLd);
            }
        }

        public void Render(SharpDX.Direct3D11.DeviceContext context)
        {
            _shadderParams.X = _fadder.ValueInterp;
            _shadderParams.Y = _fadeType;

            _effect.Begin(context);
            _effect.CBPerDraw.Values.Params = _shadderParams;
            _effect.CBPerDraw.IsDirty = true;

            _effect.Apply(context);

            //Stop effect
            if (_fadder.ValueInterp == 1 && _deactivationRequested)
            {
                _effect.PostEffectBackBuffer.Value = null;
                if (_parentHolder.ActivatedEffect == this) _parentHolder.ActivatedEffect = null;
            }
        }
    }
}
