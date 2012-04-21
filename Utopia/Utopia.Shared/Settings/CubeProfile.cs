using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Utopia.Shared.Structs.Landscape;
using Utopia.Shared.Structs;
using Utopia.Shared.Enums;
using SharpDX;
using S33M3Resources.Structs;

namespace Utopia.Shared.Settings
{
    [Serializable]
    public partial class CubeProfile
    {
        public string Name;
        public byte Id;
        public bool IsPickable;
        public bool IsBlockingLight;
        public bool IsSeeThrough;
        public bool IsBlockingWater;
        public bool IsFloodPropagation;
        public bool IsSolidToEntity;
        public bool IsEmissiveColorLightSource;
        public bool IsFlooding;
        public int FloodingPropagationPower;
        public ByteColor EmissiveColor;
        public byte EmissiveColorA { get { return EmissiveColor.A; } set { EmissiveColor.A = value; } }
        public byte EmissiveColorR { get { return EmissiveColor.R; } set { EmissiveColor.R = value; } }
        public byte EmissiveColorG { get { return EmissiveColor.G; } set { EmissiveColor.G = value; } }
        public byte EmissiveColorB { get { return EmissiveColor.B; } set { EmissiveColor.B = value; } }
        public double YBlockOffset;
        public float EnvironmentViscosity;

        public enuCubeFamilly CubeFamilly { get; set; }

        //Texture id foreach face
        public byte[] Textures = new byte[6];
        public byte Tex_Front { get { return Textures[(int)CubeFaces.Front]; } set { Textures[(int)CubeFaces.Front] = value; } }
        public byte Tex_Back { get { return Textures[(int)CubeFaces.Back]; } set { Textures[(int)CubeFaces.Back] = value; } }
        public byte Tex_Left { get { return Textures[(int)CubeFaces.Left]; } set { Textures[(int)CubeFaces.Left] = value; } }
        public byte Tex_Right { get { return Textures[(int)CubeFaces.Right]; } set { Textures[(int)CubeFaces.Right] = value; } }
        public byte Tex_Top { get { return Textures[(int)CubeFaces.Top]; } set { Textures[(int)CubeFaces.Top] = value; } }
        public byte Tex_Bottom { get { return Textures[(int)CubeFaces.Bottom]; } set { Textures[(int)CubeFaces.Bottom] = value; } }
    }
}
