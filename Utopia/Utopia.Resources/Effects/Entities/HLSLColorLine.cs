using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

using SharpDX;
using S33M3DXEngine.VertexFormat;
using S33M3DXEngine.Effects.HLSLFramework;
using SharpDX.Direct3D11;

namespace UtopiaContent.Effects.Entities
{
    public class HLSLColorLine : HLSLShaderWrap
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
        public struct CBPerDrawStructure
        {
            [FieldOffset(0)]
            public Matrix World;
            [FieldOffset(64)]
            public Matrix ViewProjection;
            [FieldOffset(128)]
            public Color4 Color;
        }

        public CBuffer<CBPerDrawStructure> CBPerDraw;

        #endregion


        #region Define Shaders EntryPoints Names
        //Default Entry points names for this HLSL file
        EntryPoints _shadersEntryPoint = new EntryPoints()
        {
            VertexShader_EntryPoint = "VS",
            PixelShader_EntryPoint = "PS"
        };
        #endregion

        public HLSLColorLine(Device engine, string shaderPath, VertexDeclaration VertexDeclaration, EntryPoints shadersEntryPoint = null)
            : base(engine, shaderPath, VertexDeclaration, null)
        {
            //Create Constant Buffers interfaces ==================================================
            CBPerDraw = new CBuffer<CBPerDrawStructure>(engine, "PerDraw");
            CBuffers.Add(CBPerDraw);
            
            //Load the shaders
            base.LoadShaders(shadersEntryPoint == null ? _shadersEntryPoint : shadersEntryPoint);
        }
    }
}
