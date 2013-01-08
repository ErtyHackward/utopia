using System.Runtime.InteropServices;
using SharpDX;
using S33M3DXEngine.Effects.HLSLFramework;
using SharpDX.Direct3D11;
using S33M3DXEngine.VertexFormat;

namespace Utopia.Resources.Effects.Entities
{
    public class HLSLVoxelModelInstanced : HLSLShaderWrap
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
        public struct CBPerFrameStructure
        {
            [FieldOffset(0)]
            public Matrix ViewProjection;
            [FieldOffset(64)]
            public Vector3 LightDirection;
            [FieldOffset(76)]
            public float FogDistance;
        }

        [StructLayout(LayoutKind.Explicit, Size = 1024)]
        public struct CBPerModelStructure
        {
            [FieldOffset(0), MarshalAs(UnmanagedType.ByValArray, SizeConst = 64)]
            public Color4[] ColorMapping; //64 values
        }

        public CBuffer<CBPerFrameStructure> CBPerFrame;
        public CBuffer<CBPerModelStructure> CBPerModel;

        #endregion

        #region Define Shaders EntryPoints Names
        //Default Entry points names for this HLSL file
        EntryPoints _shadersEntryPoint = new EntryPoints()
        {
            VertexShader_EntryPoint = "VS",
            PixelShader_EntryPoint = "PS"
        };
        #endregion

        public HLSLVoxelModelInstanced(Device device, string shaderPath, VertexDeclaration VertexDeclaration, EntryPoints shadersEntryPoint = null)
            : base(device, shaderPath, VertexDeclaration, null)
        {
            //Create Constant Buffers interfaces ==================================================
            CBPerFrame = ToDispose(new CBuffer<CBPerFrameStructure>(device, "VoxelModelPerFrame"));
            CBuffers.Add(CBPerFrame);

            CBPerModel = ToDispose(new CBuffer<CBPerModelStructure>(device, "VoxelModel", true));
            CBuffers.Add(CBPerModel);
            
            //Load the shaders
            base.LoadShaders(shadersEntryPoint == null ? _shadersEntryPoint : shadersEntryPoint);
        }

    }
}
