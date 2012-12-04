using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using SharpDX;
using S33M3DXEngine.Effects.HLSLFramework;
using SharpDX.Direct3D11;
using S33M3DXEngine.VertexFormat;
using UtopiaContent.Effects;

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
        public ShaderResource BiomesColors;
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

        public HLSLStaticEntitySprite(Device device, string shaderPath, VertexDeclaration VertexDeclaration, iCBuffer CBPerFrame = null, EntryPoints shadersEntryPoint = null)
            : base(device, shaderPath, VertexDeclaration, new UtopiaIncludeHandler())
        {
            //Create Constant Buffers interfaces ==================================================
            CBPerFrameLocal = ToDispose(new CBuffer<CBPerFrame_Struct>(device, "PerFrameLocal"));
            CBuffers.Add(CBPerFrameLocal);

            if (CBPerFrame != null) CBuffers.Add(CBPerFrame.Clone());

            //Create the resource interfaces ==================================================
            DiffuseTexture = new ShaderResource( "DiffuseTexture");
            ShaderResources.Add(DiffuseTexture);

            BiomesColors = new ShaderResource("BiomesColors");
            ShaderResources.Add(BiomesColors);

            //Create the Sampler interface ==================================================
            SamplerDiffuse = new ShaderSampler( "SamplerDiffuse");
            ShaderSamplers.Add(SamplerDiffuse);

            //Load the shaders
            base.LoadShaders(shadersEntryPoint == null ? _shadersEntryPoint : shadersEntryPoint);
        }

    }
}
