using System.Runtime.InteropServices;
using S33M3Engines;
using S33M3Engines.D3D.Effects;
using S33M3Engines.Struct.Vertex.Helper;
using SharpDX;

namespace UtopiaContent.Effects.Entities
{
    public class HLSLVoxelModel : HLSLShaderWrap
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
        [StructLayout(LayoutKind.Explicit, Size = 1168)]
        public struct CBPerFrameStructure
        {
            [FieldOffset(0)]
            public Matrix World;
            [FieldOffset(64)]
            public Matrix ViewProjection;
            [FieldOffset(128)]
            public Color3 SunColor;
            [FieldOffset(140)]
            public float FogDistance;
            [FieldOffset(144), MarshalAs(UnmanagedType.ByValArray, SizeConst = 64)]
            public Color4[] ColorMapping; //64 values
        }


        [StructLayout(LayoutKind.Explicit, Size = 64)]
        public struct CBPerPartStructure
        {
            [FieldOffset(0)]
            public Matrix Transform;
        }

        public CBuffer<CBPerFrameStructure> CBPerFrame;
        public CBuffer<CBPerPartStructure> CBPerPart;

        #endregion


        #region Define Shaders EntryPoints Names
        //Default Entry points names for this HLSL file
        EntryPoints _shadersEntryPoint = new EntryPoints()
        {
            VertexShader_EntryPoint = "VS",
            PixelShader_EntryPoint = "PS"
        };
        #endregion

        public HLSLVoxelModel(D3DEngine d3dEngine, string shaderPath, VertexDeclaration VertexDeclaration, EntryPoints shadersEntryPoint = null)
            : base(d3dEngine, shaderPath, VertexDeclaration)
        {
            //Create Constant Buffers interfaces ==================================================
            CBPerFrame = new CBuffer<CBPerFrameStructure>(_d3dEngine, "PerFrame");
            CBuffers.Add(CBPerFrame);
            CBPerFrame.MarshalUpdate = true;

            CBPerPart = new CBuffer<CBPerPartStructure>(_d3dEngine, "PerPart");
            CBuffers.Add(CBPerPart);
            
            //Load the shaders
            base.LoadShaders(shadersEntryPoint == null ? _shadersEntryPoint : shadersEntryPoint);
        }

    }
}
