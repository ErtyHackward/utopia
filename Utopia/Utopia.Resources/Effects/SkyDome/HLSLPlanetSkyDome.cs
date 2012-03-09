using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using SharpDX;
using S33M3_DXEngine.Effects.HLSLFramework;
using SharpDX.Direct3D11;
using S33M3_DXEngine.VertexFormat;

namespace Utopia.Resources.Effects.Skydome
{

    public class HLSLPlanetSkyDome : HLSLShaderWrap
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
        [StructLayout(LayoutKind.Explicit, Size = 160)]
        public struct CBPerDraw_Struct
        {
            [FieldOffset(0)]
            public Matrix World;
            [FieldOffset(64)]
            public Matrix ViewProj;
            [FieldOffset(128)]
            public Vector3 CameraWorldPosition;
            [FieldOffset(140)]
            public float time;
            [FieldOffset(144)]
            public Vector3 LightDirection;
        }
        public CBuffer<CBPerDraw_Struct> CBPerDraw;
        #endregion

        #region Resources
        public ShaderResource TerraTexture;
        #endregion

        #region Sampler
        public ShaderSampler SamplerDiffuse;
        #endregion

        #region Define Shaders EntryPoints Names
        //Default Entry points names for this HLSL file
        EntryPoints _shadersEntryPoint = new EntryPoints()
        {
            VertexShader_EntryPoint = "mainVS",
            PixelShader_EntryPoint = "mainPS"
        };
        #endregion

        public HLSLPlanetSkyDome(Device device, string shaderPath, VertexDeclaration VertexDeclaration, EntryPoints shadersEntryPoint = null)
            : base(device, shaderPath, VertexDeclaration)
        {
            //Create Constant Buffers interfaces ==================================================
            CBPerDraw = new CBuffer<CBPerDraw_Struct>(device, "PerDraw");
            CBuffers.Add(CBPerDraw);

            //Create the resource interfaces ==================================================
            TerraTexture = new ShaderResource("DiffuseTexture");
            ShaderResources.Add(TerraTexture);

            //Create the Sampler interface ==================================================
            SamplerDiffuse = new ShaderSampler("SkySampler");
            ShaderSamplers.Add(SamplerDiffuse);

            //Load the shaders
            base.LoadShaders(shadersEntryPoint == null ? _shadersEntryPoint : shadersEntryPoint);
        }

    }
}
