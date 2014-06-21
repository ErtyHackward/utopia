using System.Runtime.InteropServices;
using SharpDX;
using S33M3DXEngine.VertexFormat;
using S33M3DXEngine.Effects.HLSLFramework;
using SharpDX.Direct3D11;

namespace Utopia.Resources.Effects.Terran
{

    public class HLSLTerran : HLSLShaderWrap
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
        [StructLayout(LayoutKind.Explicit, Size = 160)]
        public struct CBPerDraw_Struct
        {
            [FieldOffset(0)]
            public Matrix World;

            [FieldOffset(64)]
            public Matrix LightViewProjection;

            /// <summary>
            /// Allows to create chunk pop-up effect
            /// </summary>
            [FieldOffset(128)]
            public float PopUpValue;

            [FieldOffset(132)]
            public Vector3 SunVector;

            [FieldOffset(144)] 
            public Vector3 ShadowMapVars;

            /// <summary>
            /// Indicates if shadow map is enabled
            /// </summary>
            [FieldOffset(156)]
            public bool UseShadowMap;

        }
        public CBuffer<CBPerDraw_Struct> CBPerDraw;
        #endregion

        #region Resources
        public ShaderResource TerraTexture;
        public ShaderResource SkyBackBuffer;
        public ShaderResource BiomesColors;
        public ShaderResource ShadowMap;
        #endregion

        #region Sampler
        public ShaderSampler SamplerDiffuse;
        public ShaderSampler SamplerBackBuffer;
        #endregion

        #region Define Shaders EntryPoints Names
        //Default Entry points names for this HLSL file
        EntryPoints _shadersEntryPoint = new EntryPoints()
        {
            VertexShader_EntryPoint = "VS",
            PixelShader_EntryPoint = "PS"
        };
        #endregion

        public HLSLTerran(Device device, string shaderPath, VertexDeclaration VertexDeclaration, iCBuffer CBPerFrame = null, EntryPoints shadersEntryPoint = null)
            : base(device, shaderPath, VertexDeclaration, new UtopiaIncludeHandler())
        {
            //Create Contstant Buffers interfaces ==================================================
            CBPerDraw = ToDispose(new CBuffer<CBPerDraw_Struct>(device, "PerDraw"));
            CBuffers.Add(CBPerDraw);

            if (CBPerFrame != null) CBuffers.Add(CBPerFrame.Clone());

            //Create the resource interfaces ==================================================
            TerraTexture = new ShaderResource("TerraTexture");
            ShaderResources.Add(TerraTexture);

            SkyBackBuffer = new ShaderResource("SkyBackBuffer");
            ShaderResources.Add(SkyBackBuffer);

            BiomesColors = new ShaderResource("BiomesColors");
            ShaderResources.Add(BiomesColors);

            ShadowMap = new ShaderResource("ShadowMap");
            ShaderResources.Add(ShadowMap);

            //Create the Sampler interface ==================================================
            SamplerDiffuse = new ShaderSampler("SamplerDiffuse");
            ShaderSamplers.Add(SamplerDiffuse);

            SamplerBackBuffer = new ShaderSampler("SamplerBackBuffer");
            ShaderSamplers.Add(SamplerBackBuffer);

            //Load the shaders
            base.LoadShaders(shadersEntryPoint == null ? _shadersEntryPoint : shadersEntryPoint);
        }

    }
}
