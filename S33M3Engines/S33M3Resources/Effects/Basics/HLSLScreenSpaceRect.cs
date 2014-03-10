using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using SharpDX;
using SharpDX.Direct3D11;
using Buffer = SharpDX.Direct3D11.Buffer;
using S33M3DXEngine.Effects.HLSLFramework;
using S33M3DXEngine;
using S33M3DXEngine.VertexFormat;
using S33M3Resources.Structs.Vertex;

namespace S33M3Resources.Effects.Basics
{
    public class HLSLScreenSpaceRect : HLSLShaderWrap
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
        public struct CBPerDraw_Struct
        {
            [FieldOffset(0)]
            public Color4 Color;
        }
        public CBuffer<CBPerDraw_Struct> CBPerDraw;

        #endregion

        #region Define Shaders EntryPoints Names
        //Default Entry points names for this HLSL file
        private EntryPoints _shadersEntryPoint = new EntryPoints()
        {
            VertexShader_EntryPoint = "VS",
            PixelShader_EntryPoint = "PS"
        };
        #endregion

        public HLSLScreenSpaceRect(Device device)
            : base(device, @"Effects\Basics\ScreenSpaceRect.hlsl", VertexPosition2.VertexDeclaration, null)
        {
            //Create Constant Buffers interfaces
            CBPerDraw = ToDispose(new CBuffer<CBPerDraw_Struct>(device, "PerDraw"));
            CBuffers.Add(CBPerDraw);

            //Load the shaders only after the CBuffer have been defined
            base.LoadShaders(_shadersEntryPoint);
        }

    }
}
