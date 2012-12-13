using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using SharpDX;
using S33M3DXEngine.Effects.HLSLFramework;
using S33M3DXEngine;
using S33M3DXEngine.VertexFormat;
using SharpDX.Direct3D11;
using SharpDX.D3DCompiler;

namespace S33M3Resources.Effects.Sprites
{
    public class HLSLCubeColorParticule : HLSLShaderWrap
    {
        #region Define Constant Buffer Structs !
        #endregion

        #region Resources
        #endregion

        #region Sampler
        #endregion

        #region Define Shaders EntryPoints Names
        //Default Entry points names for this HLSL file
        EntryPoints _shadersEntryPoint = new EntryPoints()
        {
            VertexShader_EntryPoint = "VS",
            GeometryShader_EntryPoint = "GS",
            PixelShader_EntryPoint = "PS"
        };
        #endregion

        public HLSLCubeColorParticule(Device device, string shaderPath, VertexDeclaration VertexDeclaration, iCBuffer CBPerFrame, Include includeHandler, EntryPoints shadersEntryPoint = null)
            : base(device, shaderPath, VertexDeclaration, includeHandler)
        {
            //Create Constant Buffers interfaces ==================================================
            CBuffers.Add(CBPerFrame.Clone());

            //Load the shaders
            base.LoadShaders(shadersEntryPoint == null ? _shadersEntryPoint : shadersEntryPoint);
        }

    }
}
