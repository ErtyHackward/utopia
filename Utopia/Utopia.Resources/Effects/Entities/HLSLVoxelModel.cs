using System.Runtime.InteropServices;
using SharpDX;
using S33M3DXEngine.Effects.HLSLFramework;
using SharpDX.Direct3D11;
using S33M3DXEngine.VertexFormat;

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
        [StructLayout(LayoutKind.Explicit, Size = 1184)]
        public struct CBPerFrameStructure
        {
            [FieldOffset(0)]
            public Matrix World;
            [FieldOffset(64)]
            public Matrix ViewProjection;
            [FieldOffset(128)]
            public Color3 LightColor;
            [FieldOffset(140)]
            public float FogDistance;
            [FieldOffset(144), MarshalAs(UnmanagedType.ByValArray, SizeConst = 64)]
            public Color4[] ColorMapping; //64 values
            [FieldOffset(1168)]
            public Vector3 LightDirection;
            [FieldOffset(1180)]
            public float LightIntensity;
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

        public HLSLVoxelModel(Device device, string shaderPath, VertexDeclaration VertexDeclaration, EntryPoints shadersEntryPoint = null)
            : base(device, shaderPath, VertexDeclaration)
        {
            //Create Constant Buffers interfaces ==================================================
            CBPerFrame = new CBuffer<CBPerFrameStructure>(device, "VoxelModelPerFrame", true);
            CBuffers.Add(CBPerFrame);

            CBPerPart = new CBuffer<CBPerPartStructure>(device, "VoxelModelPerPart");
            CBuffers.Add(CBPerPart);
            
            //Load the shaders
            base.LoadShaders(shadersEntryPoint == null ? _shadersEntryPoint : shadersEntryPoint);
        }

    }
}
