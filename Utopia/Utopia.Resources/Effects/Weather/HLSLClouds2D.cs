﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using SharpDX;
using S33M3DXEngine;
using S33M3DXEngine.Effects.HLSLFramework;
using S33M3DXEngine.VertexFormat;
using SharpDX.Direct3D11;

namespace Utopia.Resources.Effects.Weather
{

    public class HLSLClouds2D : HLSLShaderWrap
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
            public Matrix WorldViewProj;
            [FieldOffset(64)]
            public Vector2 UVOffset;
            [FieldOffset(72)]
            public float nbrLayers;
        }
        public CBuffer<CBPerDraw_Struct> CBPerDraw;
        #endregion

        #region Resources
        public ShaderResource CloudTexture;
        #endregion

        #region Sampler
        public ShaderSampler cloudSampler;
        #endregion

        #region Define Shaders EntryPoints Names
        //Default Entry points names for this HLSL file
        EntryPoints _shadersEntryPoint = new EntryPoints()
        {
            VertexShader_EntryPoint = "VS",
            PixelShader_EntryPoint = "PS"
        };
        #endregion

        public HLSLClouds2D(Device device, string shaderPath, VertexDeclaration VertexDeclaration, EntryPoints shadersEntryPoint = null)
            : base(device, shaderPath, VertexDeclaration)
        {
            //Create Constant Buffers interfaces ==================================================
            CBPerDraw = ToDispose(new CBuffer<CBPerDraw_Struct>(device, "PerDraw"));
            CBuffers.Add(CBPerDraw);

            //Create the resource interfaces ==================================================
            CloudTexture = new ShaderResource( "CloudTexture");
            ShaderResources.Add(CloudTexture);

            //Create the Sampler interface ==================================================
            cloudSampler = new ShaderSampler( "cloudSampler");
            ShaderSamplers.Add(cloudSampler);

            //Load the shaders
            base.LoadShaders(shadersEntryPoint == null ? _shadersEntryPoint : shadersEntryPoint);
        }

    }
}
