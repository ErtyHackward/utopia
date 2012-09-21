using S33M3DXEngine.Effects.HLSLFramework;
using S33M3Resources.Structs.Vertex;
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

        public HLSLBlur(Device device)
            : base(device, @"Effects\Sprites\Blur.hlsl", VertexPosition2Texture.VertexDeclaration)
        {
            
            //Create the resource interfaces ==================================================
            SpriteTexture = new ShaderResource("SpriteTexture") {IsStaticResource = false};
            ShaderResources.Add(SpriteTexture);

            //Create the Sampler interface ==================================================
            SpriteSampler = new ShaderSampler("SpriteSampler") {IsStaticResource = false};
            ShaderSamplers.Add(SpriteSampler);

            //Load the shaders
            LoadShaders(_shadersEntryPoint);
        }

    }
}
