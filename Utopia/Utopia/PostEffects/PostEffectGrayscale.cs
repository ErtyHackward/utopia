using S33M3DXEngine.Main;
using S33M3DXEngine.RenderStates;
using S33M3Resources.Structs.Vertex;
using SharpDX.Direct3D11;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Utopia.Resources.Effects.PostEffects;
using Utopia.Shared.GameDXStates;
using Utopia.Shared.Settings;

namespace Utopia.PostEffects
{
    public class PostEffectGrayscale : BaseComponent, IPostEffect
    {
        private HLSLGrayScale _effect;

        public string Name { get; set; }

        public void Initialize(Device device)
        {
            _effect = ToDispose(new HLSLGrayScale(device, ClientSettings.EffectPack + @"PostEffects\GrayScale.hlsl", VertexPosition2Texture.VertexDeclaration));
            _effect.SamplerPostEffectBackBuffer.Value = RenderStatesRepo.GetSamplerState(DXStates.Samplers.UVClamp_MinMagMipPoint);
        }

        public void Activate(ShaderResourceView backbuffer)
        {
            _effect.PostEffectBackBuffer.Value = backbuffer;
        }

        public void Deactivate()
        {
            _effect.PostEffectBackBuffer.Value = null;
        }

        public void Render(SharpDX.Direct3D11.DeviceContext context)
        {
            _effect.Begin(context);
            _effect.Apply(context);
        }
    }
}
