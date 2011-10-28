using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using S33M3Engines.Struct.Vertex.Helper;
using System.Runtime.InteropServices;
using SharpDX;
using SharpDX.D3DCompiler;
using SharpDX.Direct3D11;
using Buffer = SharpDX.Direct3D11.Buffer;
using RectangleF = System.Drawing.RectangleF;
using S33M3Engines.D3D.Effects;
using S33M3Engines.D3D;

namespace S33M3Engines.D3D.Effects.Basics
{

    public class HLSLSprites : HLSLShaderWrap
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
        public struct CBPerBatch_Struct
        {
            [FieldOffset(0)]
            public Vector2 TextureSize;
            [FieldOffset(8)]
            public Vector2 ViewportSize;
        }
        public CBuffer<CBPerBatch_Struct> CBPerDraw;

        [StructLayout(LayoutKind.Explicit, Size = 112)]
        public struct CBPerInstance_Struct
        {
            [FieldOffset(0)]
            public Matrix Transform;
            [FieldOffset(64)]
            public Color4 Color;
            [FieldOffset(80)]
            public RectangleF SourceRect;
            [FieldOffset(96)]
            public uint TextureArrayIndex;
            [FieldOffset(100)]
            public float Depth;
        }

        public CBuffer<CBPerInstance_Struct> CBPerInstance;
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

        public HLSLSprites(D3DEngine d3dEngine, string shaderPath, VertexDeclaration VertexDeclaration, EntryPoints shadersEntryPoint = null)
            : base(d3dEngine, shaderPath, VertexDeclaration)
        {
            //Create Constant Buffers interfaces ==================================================
            CBPerDraw = new CBuffer<CBPerBatch_Struct>(_d3dEngine, "PerBatch");
            CBuffers.Add(CBPerDraw);

            CBPerInstance = new CBuffer<CBPerInstance_Struct>(_d3dEngine, "PerInstance");
            CBuffers.Add(CBPerInstance);

            //Create the resource interfaces ==================================================
            SpriteTexture = new ShaderResource(_d3dEngine, "SpriteTexture");
            ShaderResources.Add(SpriteTexture);

            //Create the Sampler interface ==================================================
            SpriteSampler = new ShaderSampler(_d3dEngine, "SpriteSampler");
            ShaderSamplers.Add(SpriteSampler);

            //Load the shaders
            base.LoadShaders(shadersEntryPoint == null ? _shadersEntryPoint : shadersEntryPoint);
        }

    }
}
