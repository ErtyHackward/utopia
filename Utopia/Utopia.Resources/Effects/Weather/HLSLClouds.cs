using S33M3DXEngine.Effects.HLSLFramework;
using System.Runtime.InteropServices;
using S33M3DXEngine.VertexFormat;
using SharpDX;
using SharpDX.Direct3D11;
using S33M3Resources.VertexFormats;

namespace Utopia.Resources.Effects.Weather
{
    public class HLSLClouds : HLSLShaderWrap
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
        [StructLayout(LayoutKind.Explicit, Size = 80)]
        public struct CBPerDrawStruct
        {
            [FieldOffset(0)]
            public Matrix World;
            [FieldOffset(64)]
            public float Brightness;
        }
        public CBuffer<CBPerDrawStruct> CBPerDraw;

        public ShaderResource SolidBackBuffer;
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

        public HLSLClouds(Device device, string effectPath, VertexDeclaration vertexDeclaration, params iCBuffer[] externalCBuffers)
            : base(device, effectPath, vertexDeclaration, new UtopiaIncludeHandler(), externalCBuffers)
        {
            //Create Constant Buffers interfaces
            CBPerDraw = ToDispose(new CBuffer<CBPerDrawStruct>(device, "PerDraw"));
            CBuffers.Add(CBPerDraw);

            //Create the resource interfaces ==================================================
            SamplerBackBuffer = new ShaderSampler("SamplerBackBuffer");
            ShaderSamplers.Add(SamplerBackBuffer);

            SolidBackBuffer = new ShaderResource("SkyBackBuffer");
            ShaderResources.Add(SolidBackBuffer);

            //Load the shaders only after the CBuffer have been defined
            base.LoadShaders(_shadersEntryPoint);
        }
    }
}
