using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using S33M3DXEngine.Effects.HLSLFramework;
using System.Runtime.InteropServices;
using SharpDX;
using SharpDX.Direct3D11;
using S33M3Resources.VertexFormats;

namespace UtopiaContent.Effects.Weather
{
    public class HLSLClouds3D : HLSLShaderWrap
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
        [StructLayout(LayoutKind.Explicit, Size = 64)]
        public struct CBPerDraw_Struct
        {
            [FieldOffset(0)]
            public Matrix World;
        }
        public CBuffer<CBPerDraw_Struct> CBPerDraw;

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

        public HLSLClouds3D(Device device, string effectPath, params iCBuffer[] externalCBuffers)
            : base(device, effectPath, VertexPosition3Color.VertexDeclaration, externalCBuffers)
        {
            //Create Constant Buffers interfaces
            CBPerDraw = ToDispose(new CBuffer<CBPerDraw_Struct>(device, "PerDraw"));
            CBuffers.Add(CBPerDraw);

            //Create the resource interfaces ==================================================
            SamplerBackBuffer = new ShaderSampler("SamplerBackBuffer");
            ShaderSamplers.Add(SamplerBackBuffer);

            SolidBackBuffer = new ShaderResource("SolidBackBuffer", false);
            ShaderResources.Add(SolidBackBuffer);

            //Load the shaders only after the CBuffer have been defined
            base.LoadShaders(_shadersEntryPoint);
        }
    }
}
