using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using S33M3Engines.Struct.Vertex.Helper;
using System.Runtime.InteropServices;
using SharpDX;
using S33M3Engines.D3D.Effects;
using S33M3Engines.D3D;
using S33M3Engines;

namespace UtopiaContent.Effects.Terran
{

    public class HLSLLiquid : HLSLShaderWrap
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
            public float popUpYOffset;
        }
        public CBuffer<CBPerDraw_Struct> CBPerDraw;

        [StructLayout(LayoutKind.Explicit, Size = 80)]
        public struct CBPerFrame_Struct
        {
            [FieldOffset(0)]
            public Matrix ViewProjection;   //64 (4*4 float)
            [FieldOffset(64)]
            public Vector3 SunColor;        //12 (3 float)
            [FieldOffset(76)]
            public float fogdist;           //4 (float)
        }
        public CBuffer<CBPerFrame_Struct> CBPerFrame;
        #endregion

        #region Resources
        public ShaderResource TerraTexture;
        #endregion

        #region Sampler
        public ShaderSampler SamplerDiffuse;
        #endregion

        #region Define Shaders EntryPoints Names
        //Default Entry points names for this HLSL file
        EntryPoints _shadersEntryPoint = new EntryPoints()
        {
            VertexShader_EntryPoint = "VS_LIQUID",
            PixelShader_EntryPoint = "PS"
        };
        #endregion

        public HLSLLiquid(D3DEngine d3dEngine, string shaderPath, VertexDeclaration VertexDeclaration, EntryPoints shadersEntryPoint = null)
            : base(d3dEngine, shaderPath, VertexDeclaration)
        {
            //Create Constant Buffers interfaces ==================================================
            CBPerDraw = new CBuffer<CBPerDraw_Struct>(_d3dEngine, "PerDraw");
            CBuffers.Add(CBPerDraw);

            CBPerFrame = new CBuffer<CBPerFrame_Struct>(_d3dEngine, "PerFrame");
            CBuffers.Add(CBPerFrame);

            //Create the resource interfaces ==================================================
            TerraTexture = new ShaderResource(_d3dEngine, "TerraTexture");
            ShaderResources.Add(TerraTexture);

            //Create the Sampler interface ==================================================
            SamplerDiffuse = new ShaderSampler(_d3dEngine, "SamplerDiffuse");
            ShaderSamplers.Add(SamplerDiffuse);

            //Load the shaders
            base.LoadShaders(shadersEntryPoint == null ? _shadersEntryPoint : shadersEntryPoint);
        }

    }
}
