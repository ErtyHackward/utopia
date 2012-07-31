using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using SharpDX;
using S33M3DXEngine.VertexFormat;
using S33M3DXEngine.Effects.HLSLFramework;
using SharpDX.Direct3D11;

namespace Utopia.Resources.Effects.Terran
{

    public class HLSLTerran : HLSLShaderWrap
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

            /// <summary>
            /// Allows to create chunk pop-up effect
            /// </summary>
            [FieldOffset(64)]
            public float Opaque;
        }
        public CBuffer<CBPerDraw_Struct> CBPerDraw;
        #endregion

        #region Resources
        public ShaderResource TerraTexture;
        public ShaderResource SkyBackBuffer;
        public ShaderResource BiomesColors;
        #endregion

        #region Sampler
        public ShaderSampler SamplerDiffuse;
        public ShaderSampler SamplerBackBuffer;
        #endregion

        #region Define Shaders EntryPoints Names
        //Default Entry points names for this HLSL file
        EntryPoints _shadersEntryPoint = new EntryPoints()
        {
            VertexShader_EntryPoint = "VS",
            PixelShader_EntryPoint = "PS"
        };
        #endregion

        public HLSLTerran(Device device, string shaderPath, VertexDeclaration VertexDeclaration, iCBuffer CBPerFrame = null, EntryPoints shadersEntryPoint = null)
            : base(device, shaderPath, VertexDeclaration)
        {
            //Create Contstant Buffers interfaces ==================================================
            CBPerDraw = ToDispose(new CBuffer<CBPerDraw_Struct>(device, "PerDraw"));
            CBuffers.Add(CBPerDraw);

            //CBPerFrame = new CBuffer<CBPerFrame_Struct>(_d3dEngine, "PerFrame");
            if (CBPerFrame != null) CBuffers.Add(CBPerFrame.Clone());

            //Create the resource interfaces ==================================================
            TerraTexture = new ShaderResource("TerraTexture");
            ShaderResources.Add(TerraTexture);

            SkyBackBuffer = new ShaderResource("SkyBackBuffer", false);
            ShaderResources.Add(SkyBackBuffer);

            BiomesColors = new ShaderResource("BiomesColors");
            ShaderResources.Add(BiomesColors);

            //Create the Sampler interface ==================================================
            SamplerDiffuse = new ShaderSampler("SamplerDiffuse");
            ShaderSamplers.Add(SamplerDiffuse);

            SamplerBackBuffer = new ShaderSampler("SamplerBackBuffer");
            ShaderSamplers.Add(SamplerBackBuffer);

            //Load the shaders
            base.LoadShaders(shadersEntryPoint == null ? _shadersEntryPoint : shadersEntryPoint);
        }

    }
}
