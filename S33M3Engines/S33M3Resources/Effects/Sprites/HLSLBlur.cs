using System.Runtime.InteropServices;
using S33M3DXEngine.Effects.HLSLFramework;
using S33M3Resources.Structs.Vertex;
using SharpDX;
using SharpDX.Direct3D11;

namespace S33M3Resources.Effects.Sprites
{
    /// <summary>
    /// Allows to blur some 2d texture
    /// </summary>
    public class HLSLBlur : HLSLShaderWrap
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
            public int Size;
        }
        public CBuffer<CBPerDraw_Struct> CBPerDraw;

        #endregion

        #region Resources

        public ShaderResource SpriteTexture;

        #endregion

        #region Sampler

        public ShaderSampler SpriteSampler;

        #endregion

        public enum BlurPass
        {
            Horizontal,
            Vertical
        }

        #region Define Shaders EntryPoints Names

        //Default Entry points names for this HLSL file
        private EntryPoints _shadersEntryPoint;

        #endregion

        public HLSLBlur(Device device, BlurPass pass)
            : this(device, pass, @"Effects\Sprites\Blur.hlsl")
        {
            
        }

        public HLSLBlur(Device device, BlurPass pass, string effectFilePath)
            : base(device, effectFilePath, VertexPosition2Texture.VertexDeclaration, null)
        {

            //Create Constant Buffers interfaces
            CBPerDraw = ToDispose(new CBuffer<CBPerDraw_Struct>(device, "PerDraw"));
            CBuffers.Add(CBPerDraw);

            //Create the resource interfaces ==================================================
            SpriteTexture = new ShaderResource("SpriteTexture");
            ShaderResources.Add(SpriteTexture);

            //Create the Sampler interface ==================================================
            SpriteSampler = new ShaderSampler("SpriteSampler");
            ShaderSamplers.Add(SpriteSampler);

            //Load the shaders
            switch (pass)
            {
                case BlurPass.Horizontal:
                    _shadersEntryPoint = new EntryPoints()
                    {
                        VertexShader_EntryPoint = "VS",
                        PixelShader_EntryPoint = "PS_BlurHorizontal"
                    };
                    break;
                case BlurPass.Vertical:
                    _shadersEntryPoint = new EntryPoints()
                    {
                        VertexShader_EntryPoint = "VS",
                        PixelShader_EntryPoint = "PS_BlurVertical"
                    };
                    break;
            }
            base.LoadShaders(_shadersEntryPoint);
        }

    }
}
