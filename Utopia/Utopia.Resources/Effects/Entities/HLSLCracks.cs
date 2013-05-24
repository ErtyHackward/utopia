using System.Runtime.InteropServices;
using S33M3DXEngine.Effects.HLSLFramework;
using S33M3DXEngine.VertexFormat;
using SharpDX;
using SharpDX.Direct3D11;

namespace Utopia.Resources.Effects.Entities
{
    public class HLSLCracks : HLSLShaderWrap
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
        [StructLayout(LayoutKind.Explicit, Size = 144)]
        public struct CBPerDraw_Struct
        {
            [FieldOffset(0)]
            public Matrix World;
            [FieldOffset(64)]
            public Matrix ViewProjection;
            [FieldOffset(128)]
            public Color3 LightColor;
            [FieldOffset(140)]
            public float TextureIndex;
        }
        public CBuffer<CBPerDraw_Struct> CBPerDraw;
        #endregion

        #region Resources
        public ShaderResource DiffuseTexture;
        #endregion

        #region Sampler
        public ShaderSampler SamplerDiffuse;
        #endregion

        #region Define Shaders EntryPoints Names
        //Default Entry points names for this HLSL file
        EntryPoints _shadersEntryPoint = new EntryPoints()
                                             {
                                                 VertexShader_EntryPoint = "VS",
                                                 PixelShader_EntryPoint = "PS"
                                             };
        #endregion

        public HLSLCracks(Device device, string shaderPath, VertexDeclaration VertexDeclaration, EntryPoints shadersEntryPoint = null)
            : base(device, shaderPath, VertexDeclaration, null)
        {
            //Create Constant Buffers interfaces ==================================================
            CBPerDraw = ToDispose(new CBuffer<CBPerDraw_Struct>(device, "PerDraw"));
            CBuffers.Add(CBPerDraw);

            //Create the resource interfaces ==================================================
            DiffuseTexture = new ShaderResource("DiffuseTexture");
            ShaderResources.Add(DiffuseTexture);

            //Create the Sampler interface ==================================================
            SamplerDiffuse = new ShaderSampler("SamplerDiffuse");
            ShaderSamplers.Add(SamplerDiffuse);

            //Load the shaders
            base.LoadShaders(shadersEntryPoint == null ? _shadersEntryPoint : shadersEntryPoint);
        }

    }
}