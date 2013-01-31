using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using SharpDX;
using S33M3DXEngine.Effects.HLSLFramework;
using S33M3DXEngine;
using S33M3DXEngine.VertexFormat;
using SharpDX.Direct3D11;
using SharpDX.D3DCompiler;

namespace LtreeVisualizer.Shadder
{
    public class HLSLLTree : HLSLShaderWrap
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

        [StructLayout(LayoutKind.Explicit, Size = 64)]
        public struct CBPerFrame_Struct
        {
            [FieldOffset(0)]
            public Matrix ViewProjection;
        }
        public CBuffer<CBPerFrame_Struct> CBPerFrame;
        #endregion

        #region Resources
        #endregion

        #region Sampler
        #endregion

        #region Define Shaders EntryPoints Names
        //Default Entry points names for this HLSL file
        EntryPoints _shadersEntryPoint = new EntryPoints()
        {
            VertexShader_EntryPoint = "VS",
            GeometryShader_EntryPoint = "GS",
            PixelShader_EntryPoint = "PS"
        };
        #endregion

        public HLSLLTree(Device device, string shaderPath, VertexDeclaration VertexDeclaration)
            : base(device, shaderPath, VertexDeclaration, null)
        {
            //Create Constant Buffers interfaces
            CBPerDraw = ToDispose(new CBuffer<CBPerDraw_Struct>(device, "PerDraw"));
            CBuffers.Add(CBPerDraw);

            CBPerFrame = ToDispose(new CBuffer<CBPerFrame_Struct>(device, "PerFrame"));
            CBuffers.Add(CBPerFrame);

            //Load the shaders
            base.LoadShaders(_shadersEntryPoint);
        }

    }
}
