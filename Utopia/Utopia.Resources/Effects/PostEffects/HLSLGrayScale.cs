using System.Runtime.InteropServices;
using SharpDX;
using S33M3DXEngine.VertexFormat;
using S33M3DXEngine.Effects.HLSLFramework;
using SharpDX.Direct3D11;


namespace Utopia.Resources.Effects.PostEffects
{
    public class HLSLGrayScale: HLSLShaderWrap
    {
        #region Define Constant Buffer Structs !

        #endregion

        #region Resources
        public ShaderResource PostEffectBackBuffer;
        #endregion

        #region Sampler
        public ShaderSampler SamplerPostEffectBackBuffer;
        #endregion

        #region Define Shaders EntryPoints Names
        //Default Entry points names for this HLSL file
        EntryPoints _shadersEntryPoint = new EntryPoints()
        {
            VertexShader_EntryPoint = "VS",
            PixelShader_EntryPoint = "PS"
        };
        #endregion

        public HLSLGrayScale(Device device, string shaderPath, VertexDeclaration VertexDeclaration, EntryPoints shadersEntryPoint = null)
            : base(device, shaderPath, VertexDeclaration, new UtopiaIncludeHandler())
        {
            //Create Contstant Buffers interfaces ==================================================

            //Create the resource interfaces ==================================================
            PostEffectBackBuffer = new ShaderResource("PostEffectBackBuffer");
            ShaderResources.Add(PostEffectBackBuffer);

            //Create the Sampler interface ==================================================
            SamplerPostEffectBackBuffer = new ShaderSampler("SamplerPostEffectBackBuffer");
            ShaderSamplers.Add(SamplerPostEffectBackBuffer);

            //Load the shaders
            base.LoadShaders(shadersEntryPoint == null ? _shadersEntryPoint : shadersEntryPoint);
        }

    }
}
