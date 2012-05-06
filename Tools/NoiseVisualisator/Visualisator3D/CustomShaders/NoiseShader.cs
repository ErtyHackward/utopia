using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using S33M3DXEngine.Effects.HLSLFramework;
using System.Runtime.InteropServices;
using SharpDX;
using Device = SharpDX.Direct3D11.Device;
using Resource = SharpDX.Direct3D11.Resource;
using S33M3DXEngine.VertexFormat;
using S33M3Resources.Structs.Vertex;

namespace Samples.CustomShaders
{
    public class NoiseShader : HLSLShaderWrap
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
        [StructLayout(LayoutKind.Explicit, Size = 128)]
        public struct CBPerDraw_Struct
        {
            [FieldOffset(0)]
            public Matrix World;    // Matrix size = 16 float. (1 float = 32bits = 4 bytes) !! Matrix must be transposed before being send to this !!!!
        }
        public CBuffer<CBPerDraw_Struct> CBPerDraw;
        #endregion

        #region Define Shaders EntryPoints Names
        //Default Entry points names for this HLSL file
        EntryPoints _shadersEntryPoint = new EntryPoints()
        {
            VertexShader_EntryPoint = "VS",
            PixelShader_EntryPoint = "PS"
        };
        #endregion

        public NoiseShader(Device device, iCBuffer PerFrameShared)
            : base(device, @"Visualisator3D\CustomShaders\NoiseShader.hlsl", VertexPosition4Color.VertexDeclaration, PerFrameShared)
        {
            //Create Constant Buffers interfaces ==================================================
            CBPerDraw = ToDispose(new CBuffer<CBPerDraw_Struct>(device, "PerDraw"));
            CBuffers.Add(CBPerDraw);

            //Create the resource interfaces (Textures, ...) ==================================================

            //Create the Sampler interface (Texture samplers, ...) ==================================================

            //Load the shaders only after the CBuffer have been defined
            base.LoadShaders(_shadersEntryPoint);
        }

    }
}
