using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using SharpDX;
using S33M3DXEngine.Effects.HLSLFramework;
using SharpDX.Direct3D11;
using S33M3DXEngine.VertexFormat;

namespace Utopia.Resources.Effects.Entities
{
    public class HLSLSprites3D : HLSLShaderWrap
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
        [StructLayout(LayoutKind.Explicit, Size = 128)]
        public struct CBPerFrame_Struct
        {
            [FieldOffset(0)]
            public Matrix WorldFocus;
            [FieldOffset(64)]
            public Matrix View;
        }
        public CBuffer<CBPerFrame_Struct> CBPerFrameLocal;

        [StructLayout(LayoutKind.Explicit, Size = 96)]
        public struct CBPerFrame2_Struct
        {
            [FieldOffset(0)]
            public Matrix ViewProjection;   //64 (4*4 float)
            [FieldOffset(64)]
            public Color3 SunColor;        //12 (3 float)
            [FieldOffset(76)]
            public float fogdist;           //4 (float)
            [FieldOffset(80)]
            public Vector2 BackBufferSize;
            [FieldOffset(88)]
            public Vector2 Various;
        }
        public CBuffer<CBPerFrame2_Struct> CBPerFrame2Remove;
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

        public HLSLSprites3D(Device device, string shaderPath, VertexDeclaration VertexDeclaration, iCBuffer CBPerFrame = null, EntryPoints shadersEntryPoint = null)
            : base(device, shaderPath, VertexDeclaration)
        {
            //Create Constant Buffers interfaces ==================================================
            CBPerFrameLocal = ToDispose(new CBuffer<CBPerFrame_Struct>(device, "PerFrameLocal"));
            CBuffers.Add(CBPerFrameLocal);

            CBPerFrame2Remove = ToDispose(new CBuffer<CBPerFrame2_Struct>(device, "PerFrame"));
            CBuffers.Add(CBPerFrame2Remove);

            if (CBPerFrame != null) CBuffers.Add(CBPerFrame.Clone());
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
