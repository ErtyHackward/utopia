using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using S33M3Engines.D3D.Effects;
using S33M3Engines.Struct.Vertex.Helper;
using System.Runtime.InteropServices;
using SharpDX;
using S33M3Engines;

namespace UtopiaContent.Effects.Entities
{
    public class HLSLStaticEntitySpriteInstanced : HLSLShaderWrap
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
        [StructLayout(LayoutKind.Explicit, Size = 160)]
        public struct CBPerFrame_Struct
        {
            [FieldOffset(0)]
            public Matrix WorldFocus;
            [FieldOffset(64)]
            public Matrix ViewProjection;
            [FieldOffset(128)]
            public float WindPower;
            [FieldOffset(132)]
            public Vector3 SunColor;			  // Diffuse lighting color
            [FieldOffset(144)]
            public float fogdist;
        }
        public CBuffer<CBPerFrame_Struct> CBPerFrame;
        #endregion

        #region Resources
        public ShaderResource DiffuseTexture;
        #endregion

        #region Sampler
        public ShaderSampler SamplerDiffuse;
        #endregion

        #region Define Shaders EntryPoints Names
        //Default Entry points names for this HLSL file
        EntryPoints _shadersEntryPoint = new EntryPoints()
        {
            VertexShader_EntryPoint = "VS",
            PixelShader_EntryPoint = "PS"
        };
        #endregion

        public HLSLStaticEntitySpriteInstanced(D3DEngine d3dEngine, string shaderPath, VertexDeclaration VertexDeclaration, EntryPoints shadersEntryPoint = null)
            : base(d3dEngine, shaderPath, VertexDeclaration)
        {
            //Create Constant Buffers interfaces ==================================================
            CBPerFrame = new CBuffer<CBPerFrame_Struct>(_d3dEngine, "PerFrame");
            CBuffers.Add(CBPerFrame);

            //Create the resource interfaces ==================================================
            DiffuseTexture = new ShaderResource(_d3dEngine, "DiffuseTexture");
            ShaderResources.Add(DiffuseTexture);

            //Create the Sampler interface ==================================================
            SamplerDiffuse = new ShaderSampler(_d3dEngine, "SamplerDiffuse");
            ShaderSamplers.Add(SamplerDiffuse);

            //Load the shaders
            base.LoadShaders(shadersEntryPoint == null ? _shadersEntryPoint : shadersEntryPoint);
        }

    }
}
