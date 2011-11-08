using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Utopia.Shared.Structs.Landscape;
using Utopia.Shared.Structs;
using Utopia.Shared.Enums;

namespace Utopia.Shared.Settings
{
    [Serializable]
    public partial class CubeProfileNEW
    {
        public string Name { get; set; }
        public byte Id { get; set; }
        public bool IsPickable { get; set; }
        public bool IsBlockingLight { get; set; }
        public bool IsSeeThrough { get; set; }
        public bool IsBlockingWater { get; set; }
        public bool IsFloodPropagation { get; set; }
        public bool IsSolidToEntity { get; set; }
        public bool IsEmissiveColorLightSource { get; set; }
        public bool IsFlooding { get; set; }
        public int FloodingPropagationPower { get; set; }
        public Color EmissiveColor { get; set; }
        public float YBlockOffset { get; set; }
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
