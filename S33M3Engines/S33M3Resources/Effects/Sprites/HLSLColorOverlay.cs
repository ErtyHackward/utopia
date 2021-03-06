﻿using System.Runtime.InteropServices;
using S33M3DXEngine.Effects.HLSLFramework;
using S33M3Resources.Effects.Basics;
using S33M3Resources.Structs.Vertex;
using SharpDX;
using SharpDX.Direct3D11;

namespace S33M3Resources.Effects.Sprites
{
    /// <summary>
    /// Allows to fill some 2d texture with a color
    /// </summary>
    public class HLSLColorOverlay : HLSLShaderWrap
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
        [StructLayout(LayoutKind.Explicit, Size = 16)]
        public struct CBPerDraw_Struct
        {
            [FieldOffset(0)]
            public Color4 Color;
        }
        public CBuffer<CBPerDraw_Struct> CBPerDraw;

        #endregion

        #region Resources

        public ShaderResource SpriteTexture;

        #endregion

        #region Sampler

        public ShaderSampler SpriteSampler;

        #endregion

        #region Define Shaders EntryPoints Names

        //Default Entry points names for this HLSL file
        private EntryPoints _shadersEntryPoint = new EntryPoints()
        {
            VertexShader_EntryPoint = "VS",
            PixelShader_EntryPoint = "PS"
        };

        #endregion

        public HLSLColorOverlay(Device device) : this(device, @"Effects\Sprites\ColorOverlay.hlsl")
        {
            
        }

        public HLSLColorOverlay(Device device, string shaderFilePath)
            : base(device, shaderFilePath, VertexPosition2Texture.VertexDeclaration, null)
        {

            //Create Constant Buffers interfaces
            CBPerDraw = ToDispose(new CBuffer<CBPerDraw_Struct>(device, "PerDraw"));
            CBuffers.Add(CBPerDraw);

            //Create the resource interfaces ==================================================
            SpriteTexture = new ShaderResource("SpriteTexture") ;
            ShaderResources.Add(SpriteTexture);

            //Create the Sampler interface ==================================================
            SpriteSampler = new ShaderSampler("SpriteSampler") ;
            ShaderSamplers.Add(SpriteSampler);

            //Load the shaders
            LoadShaders(_shadersEntryPoint);
        }

    }
}
