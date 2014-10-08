using System.Runtime.InteropServices;
using SharpDX;
using S33M3DXEngine.VertexFormat;
using S33M3DXEngine.Effects.HLSLFramework;
using SharpDX.Direct3D11;


namespace Utopia.Resources.Effects.PostEffects
{
    public class HLSLGhost: HLSLShaderWrap
    {
        #region Define Constant Buffer Structs !
        // follow the packing rules from here:
        // http://msdn.microsoft.com/en-us/library/bb509632(VS.85).aspx
        //
        // WARNING Mapping of array : 			
        //  [FieldOffset(16), MarshalAs(UnmanagedType.ByValArray, SizeConst = MaxLights)]
        //  public BasicEffectDirectionalLight[] DirectionalLights;
        //
        // !! Set the Marshaling update flag to one in this case !
        //
        [StructLayout(LayoutKind.Explicit, Size = 16)]
        public struct CBPerDrawStructure
        {
            [FieldOffset(0)]
            public Vector4 Params;
        }
        public CBuffer<CBPerDrawStructure> CBPerDraw;
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

        public HLSLGhost(Device device, string shaderPath, VertexDeclaration VertexDeclaration, EntryPoints shadersEntryPoint = null)
            : base(device, shaderPath, VertexDeclaration, new UtopiaIncludeHandler())
        {
            //Create Contstant Buffers interfaces ==================================================
            CBPerDraw = ToDispose(new CBuffer<CBPerDrawStructure>(device, "PerDraw"));
            CBuffers.Add(CBPerDraw);

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
