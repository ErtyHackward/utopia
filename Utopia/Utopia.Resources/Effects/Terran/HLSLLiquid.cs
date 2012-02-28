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

namespace Utopia.Resources.Effects.Terran
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
        [StructLayout(LayoutKind.Explicit, Size = 96)]
        public struct CBPerDraw_Struct
        {
            [FieldOffset(0)]
            public Matrix World;
            [FieldOffset(64)]
            public float popUpYOffset;
            [FieldOffset(80)]
            public float Opaque;
        }
        public CBuffer<CBPerDraw_Struct> CBPerDraw;
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

        public HLSLLiquid(D3DEngine d3dEngine, string shaderPath, VertexDeclaration VertexDeclaration, iCBuffer CBPerFrame = null, EntryPoints shadersEntryPoint = null)
            : base(d3dEngine, shaderPath, VertexDeclaration)
        {
            //Create Constant Buffers interfaces ==================================================
            CBPerDraw = new CBuffer<CBPerDraw_Struct>(_d3dEngine, "PerDraw");
            CBuffers.Add(CBPerDraw);

            if (CBPerFrame != null) CBuffers.Add(CBPerFrame);

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
