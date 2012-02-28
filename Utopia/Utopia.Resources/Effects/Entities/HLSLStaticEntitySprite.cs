using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using S33M3Engines.D3D.Effects;
using S33M3Engines.Struct.Vertex.Helper;
using System.Runtime.InteropServices;
using SharpDX;
using S33M3Engines;

namespace Utopia.Resources.Effects.Entities
{
    public class HLSLStaticEntitySprite : HLSLShaderWrap
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
        public struct CBPerFrame_Struct
        {
            [FieldOffset(0)]
            public Matrix WorldFocus;
            [FieldOffset(64)]
            public Matrix View;
            [FieldOffset(128)]
            public Vector3 WindPower;
            [FieldOffset(140)]
            public float KeyFrameAnimation;
        }
        public CBuffer<CBPerFrame_Struct> CBPerFrameLocal;
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

        public HLSLStaticEntitySprite(D3DEngine d3dEngine, string shaderPath, VertexDeclaration VertexDeclaration, iCBuffer CBPerFrame = null, EntryPoints shadersEntryPoint = null)
            : base(d3dEngine, shaderPath, VertexDeclaration)
        {
            //Create Constant Buffers interfaces ==================================================
            CBPerFrameLocal = new CBuffer<CBPerFrame_Struct>(_d3dEngine, "PerFrameLocal");
            CBuffers.Add(CBPerFrameLocal);

            if (CBPerFrame != null) CBuffers.Add(CBPerFrame.Clone());

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
