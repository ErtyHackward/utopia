using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using S33M3Engines.Struct.Vertex.Helper;
using System.Runtime.InteropServices;
using SharpDX;
using SharpDX.Direct3D11;
using Buffer = SharpDX.Direct3D11.Buffer;
using S33M3Engines.D3D.Effects;
using S33M3Engines.D3D;

namespace Liquid.plugin.LiquidsContent.Effects
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
            public float popUpYOffset;
        }
        public CBuffer<CBPerDraw_Struct> CBPerDraw;

        [StructLayout(LayoutKind.Explicit, Size = 112)]
        public struct CBPerFrame_Struct
        {
            [FieldOffset(0)]
            public Matrix ViewProjection;
            [FieldOffset(64)]
            public Vector3 SunColor;
            [FieldOffset(76)]
            public float dayTime;
            [FieldOffset(80)]
            public float WaveGlobalOffset;
            [FieldOffset(84)]
            public float LiquidOffset;
            [FieldOffset(88)]
            public Vector2 BackBufferSize;
            [FieldOffset(96)]
            public float fogdist;
        }
        public CBuffer<CBPerFrame_Struct> CBPerFrame;
        #endregion

        #region Resources
        public ShaderResource TerraTexture;
        public ShaderResource SolidBackBuffer;
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

        public HLSLLiquid(Game game, string shaderPath, VertexDeclaration VertexDeclaration, EntryPoints shadersEntryPoint = null)
            : base(game, shaderPath, VertexDeclaration)
        {
            //Create Constant Buffers interfaces ==================================================
            CBPerDraw = new CBuffer<CBPerDraw_Struct>(_game.D3dEngine, "PerDraw");
            CBuffers.Add(CBPerDraw);

            CBPerFrame = new CBuffer<CBPerFrame_Struct>(_game.D3dEngine, "PerFrame");
            CBuffers.Add(CBPerFrame);

            //Create the resource interfaces ==================================================
            TerraTexture = new ShaderResource(_game.D3dEngine, "TerraTexture");
            ShaderResources.Add(TerraTexture);

            SolidBackBuffer = new ShaderResource(_game.D3dEngine, "SolidBackBuffer");
            ShaderResources.Add(SolidBackBuffer);

            //Create the Sampler interface ==================================================
            SamplerDiffuse = new ShaderSampler(_game.D3dEngine, "SamplerDiffuse");
            ShaderSamplers.Add(SamplerDiffuse);

            SamplerBackBuffer = new ShaderSampler(_game.D3dEngine, "SamplerBackBuffer");
            ShaderSamplers.Add(SamplerBackBuffer);

            //Load the shaders
            base.LoadShaders(shadersEntryPoint == null ? _shadersEntryPoint : shadersEntryPoint);
        }

    }
}
