﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using SharpDX;
using S33M3DXEngine.VertexFormat;
using S33M3DXEngine.Effects.HLSLFramework;
using S33M3DXEngine;
using SharpDX.Direct3D11;

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
        [StructLayout(LayoutKind.Explicit, Size = 80)]
        public struct CBPerDraw_Struct
        {
            [FieldOffset(0)]
            public Matrix World;
            [FieldOffset(64)]
            public float PopUpValue;
            [FieldOffset(68)]
            public float FogType;
        }
        public CBuffer<CBPerDraw_Struct> CBPerDraw;
        #endregion

        #region Resources
        public ShaderResource TerraTexture;
        public ShaderResource SolidBackBuffer;
        public ShaderResource SkyBackBuffer;
        public ShaderResource BiomesColors;
        public ShaderResource AnimatedTextures;
        #endregion

        #region Sampler
        public ShaderSampler SamplerDiffuse;
        public ShaderSampler SamplerBackBuffer;
        public ShaderSampler SamplerOverlay;
        #endregion

        #region Define Shaders EntryPoints Names
        //Default Entry points names for this HLSL file
        EntryPoints _shadersEntryPoint = new EntryPoints()
        {
            VertexShader_EntryPoint = "VS_LIQUID",
            PixelShader_EntryPoint = "PS"
        };
        #endregion

        public HLSLLiquid(Device device, string shaderPath, VertexDeclaration VertexDeclaration, iCBuffer CBPerFrame = null, EntryPoints shadersEntryPoint = null)
            : base(device, shaderPath, VertexDeclaration, new UtopiaIncludeHandler())
        {

            //Create Constant Buffers interfaces ==================================================
            CBPerDraw = ToDispose(new CBuffer<CBPerDraw_Struct>(device, "PerDraw"));
            CBuffers.Add(CBPerDraw);

            if (CBPerFrame != null) CBuffers.Add(CBPerFrame.Clone());

            //Create the resource interfaces ==================================================
            TerraTexture = new ShaderResource("TerraTexture");
            ShaderResources.Add(TerraTexture);

            SolidBackBuffer = new ShaderResource("SolidBackBuffer");
            ShaderResources.Add(SolidBackBuffer);

            SkyBackBuffer = new ShaderResource("SkyBackBuffer");
            ShaderResources.Add(SkyBackBuffer);

            BiomesColors = new ShaderResource("BiomesColors");
            ShaderResources.Add(BiomesColors);

            AnimatedTextures = new ShaderResource("AnimatedTextures");
            ShaderResources.Add(AnimatedTextures);

            //Create the Sampler interface ==================================================
            SamplerDiffuse = new ShaderSampler("SamplerDiffuse");
            ShaderSamplers.Add(SamplerDiffuse);

            SamplerBackBuffer = new ShaderSampler("SamplerBackBuffer");
            ShaderSamplers.Add(SamplerBackBuffer);

            SamplerOverlay = new ShaderSampler("SamplerOverlay");
            ShaderSamplers.Add(SamplerOverlay);

            //Load the shaders
            base.LoadShaders(shadersEntryPoint == null ? _shadersEntryPoint : shadersEntryPoint);
        }
    }
}
