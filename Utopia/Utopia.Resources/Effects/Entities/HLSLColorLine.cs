using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using S33M3Engines;
using S33M3Engines.D3D.Effects;
using S33M3Engines.Struct.Vertex.Helper;
using SharpDX;

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

        public HLSLColorLine(D3DEngine d3dEngine, string shaderPath, VertexDeclaration VertexDeclaration, EntryPoints shadersEntryPoint = null)
            : base(d3dEngine, shaderPath, VertexDeclaration)
        {
            //Create Constant Buffers interfaces ==================================================
            CBPerDraw = new CBuffer<CBPerDrawStructure>(_d3dEngine, "PerDraw");
            CBuffers.Add(CBPerDraw);
            
            //Load the shaders
            base.LoadShaders(shadersEntryPoint == null ? _shadersEntryPoint : shadersEntryPoint);
        }
    }
}
