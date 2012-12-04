using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using SharpDX;
using SharpDX.Direct3D11;
using Buffer = SharpDX.Direct3D11.Buffer;
using S33M3DXEngine.Effects.HLSLFramework;
using S33M3DXEngine.VertexFormat;
using S33M3DXEngine;
using S33M3Resources.Structs.Vertex;

namespace S33M3Resources.Effects.Sprites
{

    public class HLSLSprites2 : HLSLShaderWrap
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
        public struct CBPerDraw_Struct
        {
            [FieldOffset(0)]
            public Matrix OrthoProjection;
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
        EntryPoints _shadersEntryPoint = new EntryPoints()
        {
            VertexShader_EntryPoint = "SpriteVS",
            PixelShader_EntryPoint = "SpritePS"
        };
        #endregion

        public HLSLSprites2(Device device) : this(device, @"Effects\Sprites\Sprites2.hlsl")
        {
            
        }

        public HLSLSprites2(Device device, string shaderFilePath)
            : base(device, shaderFilePath, VertexSprite2.VertexDeclaration, null)
        {
            //Create Constant Buffers interfaces ==================================================
            CBPerDraw = ToDispose(new CBuffer<CBPerDraw_Struct>(device, "PerDraw"));
            CBuffers.Add(CBPerDraw);

            //Create the resource interfaces ==================================================
            SpriteTexture = new ShaderResource("SpriteTexture") ;
            ShaderResources.Add(SpriteTexture);

            //Create the Sampler interface ==================================================
            SpriteSampler = new ShaderSampler("SpriteSampler") ;
            ShaderSamplers.Add(SpriteSampler);

            //Load the shaders
            base.LoadShaders(_shadersEntryPoint);
        }

    }
}
