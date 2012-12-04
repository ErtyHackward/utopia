using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using S33M3DXEngine.Effects.HLSLFramework;
using System.Runtime.InteropServices;
using Buffer = SharpDX.Direct3D11.Buffer;
using SharpDX;
using SharpDX.Direct3D11;
using S33M3DXEngine.VertexFormat;
using S33M3Resources.VertexFormats;

namespace S33M3Resources.Effects.Debug
{
    public class HLSLColumnChart : HLSLShaderWrap
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
        public struct CBPerBatch_Struct
        {
            [FieldOffset(0)]
            public Vector2 ViewportSize;
        }
        public CBuffer<CBPerBatch_Struct> CBPerDraw;

        #endregion

        #region Define Shaders EntryPoints Names
        //Default Entry points names for this HLSL file
        EntryPoints _shadersEntryPoint = new EntryPoints()
        {
            VertexShader_EntryPoint = "VS",
            PixelShader_EntryPoint = "PS"
        };
        #endregion

        public HLSLColumnChart(Device device)
            : base(device, @"Effects\Debug\ColumnChart.hlsl", VertexColumnChart.VertexDeclaration, null)
        {
            //Create Constant Buffers interfaces ==================================================
            CBPerDraw = ToDispose(new CBuffer<CBPerBatch_Struct>(device, "CBPerDraw"));
            CBuffers.Add(CBPerDraw);

            //Load the shaders
            base.LoadShaders(_shadersEntryPoint);
        }

    }
}
