using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using SharpDX;
using S33M3DXEngine.VertexFormat;
using S33M3DXEngine.Effects.HLSLFramework;
using SharpDX.Direct3D11;

namespace UtopiaContent.Effects.Entities
{
    public class HLSLLoadingCube : HLSLShaderWrap
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
        public struct CBPerDraw_Struct
        {
            [FieldOffset(0)]
            public Matrix World;
            [FieldOffset(64)]
            public Color4 Color;
        }
        public CBuffer<CBPerDraw_Struct> CBPerDraw;

        [StructLayout(LayoutKind.Explicit, Size = 144)]
        public struct CBPerFrame_Struct
        {
            [FieldOffset(0)]
            public Matrix View;
            [FieldOffset(64)]
            public Matrix Projection;
            [FieldOffset(128)]
            public Vector3 DiffuseLightDirection;
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
            PixelShader_EntryPoint = "PSColoredCube"
        };
        #endregion

        public HLSLLoadingCube(Device device, string shaderPath, VertexDeclaration VertexDeclaration, EntryPoints shadersEntryPoint = null)
            : base(device, shaderPath, VertexDeclaration)
        {
            //Create Constant Buffers interfaces ==================================================
            CBPerDraw = ToDispose(new CBuffer<CBPerDraw_Struct>(device, "PerDraw"));
            CBuffers.Add(CBPerDraw);

            CBPerFrame = ToDispose(new CBuffer<CBPerFrame_Struct>(device, "PerFrame"));
            CBuffers.Add(CBPerFrame);

            //Create the resource interfaces ==================================================

            //Create the Sampler interface ==================================================

            //Load the shaders
            base.LoadShaders(shadersEntryPoint == null ? _shadersEntryPoint : shadersEntryPoint);
        }

    }
}
