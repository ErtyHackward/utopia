using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using S33M3Engines.Struct.Vertex.Helper;
using System.Runtime.InteropServices;
using SharpDX;
using SharpDX.D3DCompiler;
using SharpDX.Direct3D11;
using Buffer = SharpDX.Direct3D11.Buffer;

namespace S33M3Engines.D3D.Effects.Basics
{

    public class HLSLVertexPositionColor : HLSLShaderWrap
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

        [StructLayout(LayoutKind.Explicit, Size = 128)]
        public struct CBPerFrame_Struct
        {
            [FieldOffset(0)]
            public Matrix View;
            [FieldOffset(64)]
            public Matrix Projection;
        }
        public CBuffer<CBPerFrame_Struct> CBPerFrame;
        #endregion

        #region Define Shaders EntryPoints Names
        //Default Entry points names for this HLSL file
        EntryPoints _shadersEntryPoint = new EntryPoints()
        {
            VertexShader_EntryPoint = "VS",
            PixelShader_EntryPoint = "PS"
        };
        #endregion

        public HLSLVertexPositionColor(D3DEngine _d3dEngine, string shaderPath, VertexDeclaration VertexDeclaration, EntryPoints shadersEntryPoint = null)
            : base(_d3dEngine, shaderPath, VertexDeclaration)
        {
            //Create Constant Buffers interfaces
            CBPerDraw = new CBuffer<CBPerDraw_Struct>(_d3dEngine, "PerDraw");
            CBuffers.Add(CBPerDraw);

            CBPerFrame = new CBuffer<CBPerFrame_Struct>(_d3dEngine, "PerFrame");
            CBuffers.Add(CBPerFrame);

            //Load the shaders only after the CBuffer have been defined
            base.LoadShaders(shadersEntryPoint == null ? _shadersEntryPoint : shadersEntryPoint);
        }

    }
}
