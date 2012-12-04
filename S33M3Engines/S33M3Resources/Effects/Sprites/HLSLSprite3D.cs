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

namespace S33M3Resources.Effects.Sprites
{
    public class HLSLSprite3D : HLSLShaderWrap
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
        public struct CBPerFrameLocal_Struct
        {
            [FieldOffset(0)]
            public Matrix ViewProjection;
        }
        public CBuffer<CBPerFrameLocal_Struct> CBPerFrameLocal;
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
            GeometryShader_EntryPoint = "GS",
            PixelShader_EntryPoint = "PS"
        };
        #endregion

        public HLSLSprite3D(Device device, string shaderPath, VertexDeclaration VertexDeclaration, EntryPoints shadersEntryPoint = null)
            : base(device, shaderPath, VertexDeclaration, null)
        {
            //Create Constant Buffers interfaces ==================================================
            CBPerFrameLocal = ToDispose(new CBuffer<CBPerFrameLocal_Struct>(device, "PerFrameLocal"));
            CBuffers.Add(CBPerFrameLocal);

            //Create the resource interfaces ==================================================
            DiffuseTexture = new ShaderResource("DiffuseTexture");
            ShaderResources.Add(DiffuseTexture);

            //Create the Sampler interface ==================================================
            SamplerDiffuse = new ShaderSampler("SamplerDiffuse");
            ShaderSamplers.Add(SamplerDiffuse);

            //Load the shaders
            base.LoadShaders(shadersEntryPoint == null ? _shadersEntryPoint : shadersEntryPoint);
        }

    }
}
